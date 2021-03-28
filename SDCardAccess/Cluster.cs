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
        Sector[] sectors;
        /// <summary>Base Sector used in this cluster</summary>
        int SectorBase;
        /// <summary>Handle to sector manager</summary>
        SectorManager mSectorManager;
        /// <summary>Cluster manager handle</summary>
        ClusterManager mClusterManager;


        /// <summary>Lock this cluster and keep it in memory</summary>
        bool mLocked;

        // ***********************************************************************************************************************
        /// <summary>
        ///     Lock a cluster, and don't free it from the cache
        /// </summary>
        // ***********************************************************************************************************************
        public bool Locked
        {
            get { return mLocked; }
            set
            {
                if (mLocked == value) return;

                if( value )
                {
                    mClusterManager.Lock(this);
                }else
                {
                    mClusterManager.Unlock(this);
                }
                mLocked = value;
            }
        }

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

                int sec = _index/mSectorManager.SectorSize;
                int off = _index % mSectorManager.SectorSize;
                Sector sector = sectors[sec];
                if (sector == null)
                {
                    // if the sector we want has been unloaded, load it back in again
                    sectors[sec] = sector = mSectorManager.ReadSector(SectorBase + sec);
                }

                return sector[off];
            }
            set {
                if (_index > ClusterSize) return;

                int sec = _index / mSectorManager.SectorSize;
                int off = _index % mSectorManager.SectorSize;
                Sector sector = sectors[sec];
                if (sector == null)
                {
                    // if the sector we want has been unloaded, load it back in again
                    sectors[sec] = sector = mSectorManager.ReadSector(SectorBase + sec);
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
            sysFree();

            int num = (_size + (SectorSize - 1)) / SectorSize;
            ClusterSize = _size;
            sectors = new Sector[num];
            for (int i = 0; i < num; i++)
            {
                sectors[i] = mSectorManager.ReadSector(SectorBase + i);
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
        ///     Free This cluster
        /// </summary>
        // ***********************************************************************************************************************
        internal void sysFree()
        {
            foreach(Sector sector in sectors)
            {
                if (sector != null)
                {
                    mSectorManager.Free(sector);
                }
            }
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     Free This cluster
        /// </summary>
        // ***********************************************************************************************************************
        public void Free()
        {
            mClusterManager.Free(this);
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     Sector free callback
        /// </summary>
        /// <param name="_sector">the sector being freed</param>
        // ***********************************************************************************************************************
        private void SectorManager_OnFree(Sector _sector)
        {
            int num = (int)_sector.Number - SectorBase;
            if( num>=0 && num<sectors.Length){
                sectors[num] = null;
            }
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     Create a new cluster
        /// </summary>
        /// <param name="_cluster">Cluster number</param>
        /// <param name="_ClusterByteSize">cluster size in bytes</param>
        /// <param name="_SectorManager">pointer to the sector manager</param>
        // ***********************************************************************************************************************
        public Cluster(int _cluster, int _SectorBaser, int _ClusterByteSize, SectorManager _SectorManager, ClusterManager _ClusterManager)
        {
            mLength = _ClusterByteSize;
            mSectorManager = _SectorManager;
            mClusterManager = _ClusterManager;
            SectorSize = mSectorManager.SectorSize;
            ClusterSize = _ClusterByteSize;
            SectorBase = _SectorBaser;
            int count = (ClusterSize+(SectorSize-1)) / SectorSize;
            Number = _cluster;
            mLength = ClusterSize;
            sectors = new Sector[count];
            _SectorManager.OnFree += SectorManager_OnFree;
            for (int i = 0; i < count; i++){
                sectors[i] = null;
            }

            for (int i=0;i< count; i++)
            {
                sectors[i] = mSectorManager.ReadSector(SectorBase + i);
            }

        }

    }
}
