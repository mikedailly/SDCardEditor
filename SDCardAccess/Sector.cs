using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    public class Sector
    {
        /// <summary>Size of an SD card sector</summary>
        public const int DEFAULT_SECTOR_SIZE = 512;
        /// <summary>Size of an SD card sector</summary>
        public static int SectorSize = DEFAULT_SECTOR_SIZE;



        /// <summary>Sector number</summary>
        long mNumber;

        /// <summary>Sector number</summary>
        public long Number {
            get { return mNumber; }
        }

        /// <summary>Actual sector data</summary>
        public byte[] buffer;

        /// <summary>Has this sector been modified</summary>
        public bool dirty;

        // ***********************************************************************************************************************
        /// <summary>
        ///     Read/Write sector data
        /// </summary>
        /// <param name="_index">index into sector</param>
        // ***********************************************************************************************************************
        public byte this[int _index]
        {
            get { return buffer[_index]; }
            set { buffer[_index] = value; dirty = true; }
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     Create a new sector
        /// </summary>
        /// <param name="_number">Sectore number</param>
        // ***********************************************************************************************************************
        public Sector(long  _number)
        {
            mNumber = _number;
            buffer = new byte[SectorSize];
            dirty = false;
        }
    }
}
