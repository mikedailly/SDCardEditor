﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SDCardAccess
{
    public class ClusterManager
    {
        int ClusterCacheCount;
        int ClusterSize;
        SectorManager mSectorManager;
        FileStream mCard;
        BootSector BootSector;
        int root_dir_first_sector;

        public List<Cluster> Clusters;

        // ********************************************************************************************
        /// <summary>
        ///     Read a cluster
        /// </summary>
        /// <param name="_cluster">The cluster to read</param>
        /// <returns>
        /// </returns>
        // ********************************************************************************************
        public Cluster ReadCluster(int _cluster, int _bytesToRead = -1)
        {
            // first check to see if it's in the cache
            for (int i = 0; i < Clusters.Count; i++)
            {
                Cluster s = Clusters[i];
                if (s.Number == _cluster)
                {
                    // move most recent to head of the list
                    Clusters.RemoveAt(i);
                    Clusters.Insert(0, s);
                    return s;
                }
            }

            int base_sector = (int)root_dir_first_sector;
            int BytesPerSector = BootSector.BytesPerSector;

            int bytes_to_read = BootSector.SectorsPerCluster * BytesPerSector;
            // Number of bytes required...rounded up to a whole sector, but LESS or EQUAL than a cluster only
            if (_bytesToRead != -1 && _bytesToRead < bytes_to_read)
            {
                bytes_to_read = _bytesToRead;
                bytes_to_read = (bytes_to_read + (BytesPerSector - 1)) / BytesPerSector;
                bytes_to_read = bytes_to_read * BytesPerSector;
            }

            if (BootSector.FatType == eFATType.FAT32 && _cluster == -1)
            {
                _cluster = BootSector.RootDriveTableCluster;
            }
            // cluster numbers start at 2.... so -2
            int root_dir_size = 32 * BootSector.FAT16_NumRootEntries;
            int sector = ((_cluster - 2) * BootSector.SectorsPerCluster) + base_sector + (root_dir_size / BootSector.BytesPerSector);
            int ClusterSize = BootSector.SectorsPerCluster * BootSector.BytesPerSector;
            if (_cluster == -1)
            {
                // if -1 then ROOT directory
                sector = (int)root_dir_first_sector; // (BootSector.FATCopies * BootSector.SectorsPerFAT) + BootSector.ReservedSectors + (int)CurrentPartition.LBABegin;
                if (BootSector.FatType == eFATType.FAT16)
                {
                    ClusterSize = 16384;
                }
            }
            Cluster cluster = new Cluster(_cluster, sector, ClusterSize, mSectorManager);


            // if we're fully cached out, drop the least used...
            if (Clusters.Count >= ClusterCacheCount)
            {
                // check for dirty here....
                Clusters.RemoveAt(ClusterCacheCount - 1);
            }
            // move most recent to head of the list
            Clusters.Insert(0, cluster);
            return cluster;
        }


        // ************************************************************************************************************************
        /// <summary>
        ///     Create a new cluster manager
        /// </summary>
        /// <param name="_CacheSize">Number of cache entries to keep around</param>
        /// <param name="_bootsector">The boot sector</param>
        /// <param name="_SectorManager">The Sector manager</param>
        /// <param name="_card"></param>
        // ************************************************************************************************************************
        public ClusterManager(int _CacheSize, BootSector _bootsector, SectorManager _SectorManager, FileStream _card)
        {
            ClusterCacheCount = _CacheSize;
            BootSector = _bootsector;
            mSectorManager = _SectorManager;
            mCard = _card;
            root_dir_first_sector = (int)_bootsector.SectorBase;
            Clusters = new List<Cluster>(_CacheSize);
        }
    }
}
