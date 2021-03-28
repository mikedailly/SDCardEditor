using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    public class SectorManager
    {
        /// <summary>Number of sectors to "cache"</summary>
        public int SectorCacheCount;
        /// <summary>Sector size in bytes</summary>
        public int SectorSize;
        /// <summary>The boot sector</summary>
        public BootSector Boot = null;

        /// <summary>File handle</summary>
        FileStream Card;
        /// <summary>Cached Sectors</summary>
        List<Sector> Sectors;


        // ********************************************************************************************
        /// <summary>
        ///     Read a sector
        /// </summary>
        /// <param name="_number">the sector number to read</param>
        /// <returns>
        ///     The sector being read
        /// </returns>
        // ********************************************************************************************
        public Sector ReadSector(long _number)
        {
            // first 
            for (int i = 0; i < Sectors.Count; i++)
            {
                Sector s = Sectors[i];
                if (s.Number == _number)
                {
                    // move most recent to head of the list
                    Sectors.RemoveAt(i);
                    Sectors.Insert(0, s);
                    return s;
                }
            }

            // Cache is maxed out, free the least used one....
            if(Sectors.Count>= SectorCacheCount)
            {
                Sectors[Sectors.Count - 1].Dispose();
                Sectors.RemoveAt(Sectors.Count - 1);
            }

            // seek to the sector
            Card.Seek(_number * SectorSize, SeekOrigin.Begin);
            Sector sect = new Sector(_number,Card);
            Card.Read(sect.buffer, 0, SectorSize);

            // if we're fully cached out, drop the least used...
            if (Sectors.Count >= SectorCacheCount)
            {
                Sectors.RemoveAt(SectorCacheCount - 1);
            }
            // move most recent to head of the list
            Sectors.Insert(0, sect);

            return sect;
        }




        // *******************************************************************************************************************
        /// <summary>
        ///     Create a sector manager
        /// </summary>
        /// <param name="_CacheSize">number of sectors to keep in the cache</param>
        // *******************************************************************************************************************
        public SectorManager(int _CacheSize, FileStream _file)
        {
            Card = _file;
            SectorSize = 512;
            SectorCacheCount = _CacheSize;
            Sectors = new List<Sector>(SectorCacheCount);

        }
    }
}
