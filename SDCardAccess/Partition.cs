using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    public enum ePartitionType : int
    {
        /// <summary>Unknown or Nothing</summary>
        Unknown = 0x00,          
        /// <summary>23 Bit FAT</summary>
        FAT12 = 0x01,
        /// <summary>16-bit FAT (Partition Smallerthan 32MB)</summary>
        FAT16_SMall = 0x04,
        /// <summary> Extended MS-DOS Partition</summary>
        ExtendedMSDOS = 0x05,
        /// <summary>16-bit FAT (Partition Largerthan 32MB)</summary>
        FAT16 = 0x06,
        /// <summary>32-bit FAT (Partition Up to2048GB)</summary>
        FAT32 = 0x0B,
        /// <summary> Same as 0BH, but uses LBA1 13h Extensions</summary>
        FAT32_LBA1 = 0x0C,
        /// <summary> Same as 06H, but uses LBA1 13h Extensions</summary>
        FAT16_LBA1 = 0x0E,
        /// <summary>Same as 05H, but uses LBA1 13h Extensions</summary>
        ExtendedMSDOS_LBA1 = 0x0F
    }


    // ************************************************************************************************************************************
    /// <summary>
    ///     Holds info on a single partition
    /// </summary>
    // ************************************************************************************************************************************
    public class Partition
    {
        /// <summary>Offset to partition data</summary>
        public const int PARTITION_OFFSET = 446;
        /// <summary>Offset to partition data</summary>
        public const int PARTITION_SIZE = 16;

        /// <summary>Is this a FAT32 VolumeID sector?</summary>
        public bool IsFAT32 = false;

        /// <summary>There is no MBR, first block is block 0</summary>
        public bool NoPartitionTable = false;


        /// <summary>Status or physical drive (bit 7 set is for active or bootable, old MBRs only accept 0x80, 0x00 means inactive, and 0x01–0x7F stand for invalid)</summary>
        public int BootFlags;
        /// <summary>CHS address of first absolute sector in partition. The format is described by three bytes, see the next three rows.</summary>
        public int CHSBegin;
        /// <summary>Partition type</summary>
        public ePartitionType TypeCode;
        /// <summary>CHS address of last absolute sector in partition. The format is described by 3 bytes, see the next 3 rows.</summary>
        public int CHSEnd;
        /// <summary>LBA of first absolute sector in the partition</summary>
        public long LBABegin;
        /// <summary>Number of sectors in partition</summary>
        public int SectorCount;


        // FAT32 detasils

        /// <summary>Bytes Per Sector</summary>
        public int BytesPerSector = 0;
        /// <summary>Number of Sectors Per Cluster</summary>
        public int SectorsPerClusters= 0;
        /// <summary>Number of Reserved Sectors</summary>
        public int ReservedSectors = 0;
        /// <summary>Number of FATs</summary>
        public int NumberOfFATs= 0;
        /// <summary>Sectors Per FAT</summary>
        public int SectorsPerFAT = 0;
        /// <summary>Root Directory First Cluster</summary>
        public int DirectoryRoot = 0;

        // ************************************************************************************************************************************
        /// <summary>
        ///     Create a new partition entry
        /// </summary>
        /// <param name="_sector">The sector holding the partition table</param>
        /// <param name="_partition_number">Which partition are we reading out (1 to 3)</param>
        // ************************************************************************************************************************************
        public Partition(Sector _sector, int _partition_number)
        {
            //IsFAT32 = FAT32;
            byte[] b = _sector.buffer;

            // Check to see if this is a FAT32 Volume ID with no partition
            // This is only valid in FAT32 - so check, are we a FAT32 disk?
            string Name = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                Name += (char)_sector.buffer[0x52 + i];
            }

            if (!Name.StartsWith("FAT32"))
            {
                Name = string.Empty;
                for (int i = 0; i < 8; i++)
                {
                    Name += (char)_sector.buffer[0x36 + i];
                }
            }

            // if NOT FAT32 and NOT FAT16.... then partition table
            if(!Name.StartsWith("FAT16"))
            {
                NoPartitionTable = false;
                IsFAT32 = false;
                int index = PARTITION_OFFSET + (PARTITION_SIZE * _partition_number);

                BootFlags = b[index++];

                CHSBegin = (int)b[index] + (((int)b[index + 1]) << 8) | (((int)b[index + 2]) << 16);
                index += 3;

                TypeCode = (ePartitionType) b[index++];

                CHSEnd = (int)b[index] + (((int)b[index + 1]) << 8) | (((int)b[index + 2]) << 16);
                index += 3;

                LBABegin = (long)b[index] + (((long)b[index + 1]) << 8) | (((long)b[index + 2]) << 16) | (((long)b[index + 3]) << 24);
                index += 4;

                SectorCount = (int)b[index] + (((int)b[index + 1]) << 8) | (((int)b[index + 2]) << 16) | (((int)b[index + 3]) << 24);
            }
            else
            {
                // No Master Boot Record (no partition table)
                NoPartitionTable = true;
                if (Name.StartsWith("FAT16"))
                {
                    IsFAT32 = false;

                    // FAT16 
                    BytesPerSector = (int)b[0x0b] + (((int)b[0x0c]) << 8);       // should be 0x200 (512)

                    SectorsPerClusters = (int)b[0x0d];                          // 1,2,4,8,16,32,64 or 128

                    ReservedSectors = (int)b[0x0e] + (((int)b[0x0f]) << 8);

                    NumberOfFATs = (int)b[0x10];

                    SectorsPerFAT = (int)((long)b[0x16] + (((long)b[0x17]) << 8));

                    SectorCount = (int)((long)b[0x13] + (((long)b[0x14]) << 8));
                    if (SectorCount == 0)
                    {
                        SectorCount = (int)((long)b[0x20] + (((long)b[0x21]) << 8) | (((long)b[0x22]) << 16) | (((long)b[0x23]) << 24));
                    }

                    int ReservedRegion = 0; // Volume Start
                    DirectoryRoot = ReservedRegion + ReservedSectors + (SectorsPerFAT * NumberOfFATs);

                    LBABegin = 0; // ReservedSectors;
                    TypeCode = ePartitionType.FAT16;
                    SectorCount = 0;
                }
                else
                {
                    IsFAT32 = true;

                    // FAT32 Volume ID
                    BytesPerSector = (int)b[0x0b] + (((int)b[0x0c]) << 8);       // should be 0x200 (512)

                    SectorsPerClusters = (int)b[0x0d];                          // 1,2,4,8,16,32,64 or 128

                    ReservedSectors = (int)b[0x0e] + (((int)b[0x0f]) << 8);

                    NumberOfFATs = (int)b[0x10];

                    SectorsPerFAT = (int)((long)b[0x24] + (((long)b[0x25]) << 8) | (((long)b[0x26]) << 16) | (((long)b[0x27]) << 24));

                    DirectoryRoot = (int)((long)b[0x2c] + (((long)b[0x2d]) << 8) | (((long)b[0x2e]) << 16) | (((long)b[0x2f]) << 24));

                    LBABegin = 0; // ReservedSectors;
                    TypeCode = ePartitionType.FAT32;
                    SectorCount = 0;
                }
            }
        }
    }
}
