using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    public class Cluster
    {
        /// <summary>Size of a cluster in bytes</summary>
        public static int DefaultClusterSize = 0;
        /// <summary>Cluster number</summary>
        public long Number;

        /// <summary>Sector size in bytes</summary>
        int SectorSize;
        /// <summary>Cluster size in bytes</summary>
        int ClusterSize;
        /// <summary>Sectors used by this cluster</summary>
        List<Sector> sectors;
        /// <summary>Base Sector used in this cluster</summary>
        int SectorBase;
        /// <summary>Handle to sector manager</summary>
        SectorManager SectorManager;

        // ***********************************************************************************************************************
        /// <summary>
        ///     Length of cluster in bytes
        /// </summary>
        // ***********************************************************************************************************************
        int mLength = 0;
        public int Length
        {
            get { return mLength; }
            set
            {
                SetSize(value);
            }
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     DEBUG: get the cluster in a text string
        /// </summary>
        // ***********************************************************************************************************************
        private string debug_txt
        {
            get
            {
                return this.ToString();
            }
        }




        // ***********************************************************************************************************************
        /// <summary>
        ///     Read/Write cluster data
        /// </summary>
        /// <param name="_index">index into sector</param>
        // ***********************************************************************************************************************
        public byte this[int _index]
        {
            get {
                if (_index > ClusterSize) return 0;

                int sec = _index/SectorManager.SectorSize;
                int off = _index % SectorManager.SectorSize;
                Sector sector = sectors[sec];
                if (sector == null)
                {
                    // if the sector we want has been unloaded, load it back in again
                    sectors[sec] = sector = SectorManager.ReadSector(SectorBase + sec);
                }

                return sector[off];
            }
            set {
                if (_index > ClusterSize) return;

                int sec = _index / SectorManager.SectorSize;
                int off = _index % SectorManager.SectorSize;
                Sector sector = sectors[sec];
                if (sector == null)
                {
                    // if the sector we want has been unloaded, load it back in again
                    sectors[sec] = sector = SectorManager.ReadSector(SectorBase + sec);
                }
                sector[off] = value;

            }
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     For FAT16 Root directory (16k)
        /// </summary>
        /// <param name="_size"></param>
        // ***********************************************************************************************************************
        public void SetSize(int _size)
        {
            sectors.Clear();
            int num = (_size + (SectorSize - 1)) / SectorSize;
            ClusterSize = _size;
            for(int i = 0; i < num; i++)
            {
                Sector s = SectorManager.ReadSector(SectorBase + i);
                sectors.Add(s);
            }
            mLength = _size;
        }


        // ***********************************************************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // ***********************************************************************************************************************
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(ClusterSize);
            for (int i = 0; i < ClusterSize; i++)
            {
                int b = this[i];
                if (b >= 32 && b < 256) sb.Append((char)b);
            }
            return sb.ToString();
        }


        // ***********************************************************************************************************************
        /// <summary>
        ///     Create a new cluster
        /// </summary>
        /// <param name="_cluster">Cluster number</param>
        /// <param name="_ClusterByteSize">cluster size in bytes</param>
        /// <param name="_SectorManager">pointer to the sector manager</param>
        // ***********************************************************************************************************************
        public Cluster(int _cluster, int _SectorBaser, int _ClusterByteSize, SectorManager _SectorManager)
        {
            mLength = _ClusterByteSize;
            SectorManager = _SectorManager;
            SectorSize = SectorManager.SectorSize;
            ClusterSize = _ClusterByteSize;
            SectorBase = _SectorBaser;
            int count = (ClusterSize+(SectorSize-1)) / SectorSize;
            Number = _cluster;
            mLength = ClusterSize;
            sectors = new List<Sector>(count);
            for(int i=0;i< count; i++)
            {
                sectors.Add( SectorManager.ReadSector(SectorBase + i) );
            }
        }


    }
}
