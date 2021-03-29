using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    public enum eFATType
    {
        FAT16,
        FAT32,

        Unknown = -1
    }

    public class BootSector
    {
        /// <summary>FAT type</summary>
        public eFATType FatType;
        /// <summary>Base of boot sector for this partition</summary>
        public int SectorNumber;

        /// <summary>Oem ID - Name of the formatting OS</summary>
        public string OSName;
        /// <summary>Bytes per Sector on the physical medium - Normally 512 bytes</summary>
        public int BytesPerSector;
        /// <summary>Sectors per Cluster - 1, 2, 4, 8, 16, 32, 64 or 128 sectors</summary>
        public int SectorsPerCluster;
        /// <summary>Reserved sectors in front of the FAT(s) incl. the Boot sector</summary>
        public int ReservedSectors;
        /// <summary>Number of FAT copies - Normaly 2</summary>
        public int FATCopies;
        /// <summary>Sectors per Track - The disc geometry used when formatting the partition.</summary>
        public int SectorsPerTrack;
        /// <summary>Heads - The disc geometry used when formatting the partition.</summary>
        public int Heads;
        /// <summary>The number of sectors on the disk from the start of the partition to the beginning of the first FAT.</summary>
        public int NumberOfSectors;
        /// <summary>Number of sectors in the partition</summary>
        public int PartitionSectors;
        /// <summary>Sectors per FAT</summary>
        public int SectorsPerFAT;
        /// <summary>FAT handling flags</summary>
        public int FATFlags;
        /// <summary>FAT32 Drive Version (High byte = Major version, Low byte = Minor version)</summary>
        public int FATDriveVersion;
        /// <summary>Cluster number for the start of the Root Directory Table</summary>
        public int RootDriveTableCluster;
        /// <summary>Sector number from the start of the partition, for the File System Information Sector</summary>
        public int FileSystemInfoSector;
        /// <summary>Sector number from the start of the partition, for the Backup Boot Sector</summary>
        public int BackUpBootSector;
        /// <summary>Logical Drive Number - Normaly 00h for floppies and 80h for hard drives.</summary>
        public int LogicalDriveNumber;
        /// <summary></summary>
        public int CurrentHead;
        /// <summary>This value must be either 28h or 29h in order for Windows NT to recognize the partition</summary>
        public int Signature;
        /// <summary>ID - Random generated serial number</summary>
        public long ID;
        /// <summary>Volume Label - The same as stored in a special file in the root directory.</summary>
        public string VolumeLabel;
        /// <summary>System ID - This is the string 'FAT32 '</summary>
        public string SystemID;

        /// <summary>Max clusters in this partition</summary>
        public int FAT32_MaxClusters;

        /// <summary>Small number of sectors - Used when volume size is less than 32 Mb.</summary>
        public int FAT16_SmallNumberSectors;
        /// <summary>When the volume is on a media that is partitioned, this value contains the number of sectors preceeding the first sector of the volume.</summary>
        public int FAT16_HiddenSectors;
        /// <summary>When the volume is on a media that is partitioned, this value contains the number of sectors preceeding the first sector of the volume.</summary>
        public int FAT16_DriveNumber;
        /// <summary>Number of ROOT directory entries - 0 in FAT32</summary>
        public int FAT16_NumRootEntries;

        /// <summary>The base sector clusters of this partition</summary>
        public long SectorBase;

        public int FATSector;

        // ****************************************************************************************************************************************
        /// <summary>
        ///     Create a new boot sector class - won't change, but handy to havce around
        /// </summary>
        /// <param name="_sector"></param>
        // ****************************************************************************************************************************************
        public BootSector(Sector _sector)
        {
            SectorNumber = (int)_sector.Number;

            byte[] b = _sector.buffer;

            // OEM ID
            OSName = "";
            for (int i = 0; i < 8; i++)
            {
                char c = (char)b[3 + i];
                if (c == 0) break;
                OSName += (char)c;
            }


            SectorsPerFAT = (int)b[0x16] + (((int)b[0x17]) << 8);
            FAT16_NumRootEntries = (int)b[0x11] + (((int)b[0x12]) << 8);

            if (SectorsPerFAT!=0)
            {
                FatType = eFATType.FAT16;

                BytesPerSector = (int)b[0x0b] + (((int)b[0x0c]) << 8);
                SectorsPerCluster = b[0x0d];
                ReservedSectors = (int)b[0x0e] + (((int)b[0x0f]) << 8);
                FATCopies = (int)b[0x10];

                FAT16_NumRootEntries = (int)b[0x11] + (((int)b[0x12]) << 8);
                int num_sectors_small = (int)b[0x13] + (((int)b[0x14]) << 8);

                int MediaDescription = b[0x15];

                SectorsPerFAT = (int)b[0x16] + (((int)b[0x17]) << 8);
                SectorsPerTrack = (int)b[0x19] + (((int)b[0x19]) << 8);
                Heads = (int)b[0x1a] + (((int)b[0x1b]) << 8);
                FAT16_HiddenSectors = (int)b[0x1c] + (((int)b[0x1d]) << 8) + (((int)b[0x1e]) << 16) + (((int)b[0x1f]) << 24);
                PartitionSectors = (int)b[0x20] + (((int)b[0x21]) << 8) + (((int)b[0x22]) << 16) + (((int)b[0x23]) << 24);
                FAT16_DriveNumber = (int)b[0x20];
                int ExtBootSig = (int)b[0x26];
                if (ExtBootSig == 0x29)
                {
                    VolumeLabel = "";
                    for (int i = 0; i < 11; i++)
                    {
                        char c = (char)b[0x2b + i];
                        if (c == 0) break;
                        VolumeLabel += (char)c;
                    }

                    // get FAT type (16 or 32)
                    SystemID = "";
                    for (int i = 0; i < 8; i++)
                    {
                        char c = (char)b[0x36 + i];
                        if (c == 0) break;
                        SystemID += (char)c;
                    }
                }
                // ROOT dir in fixed location after FATs
                RootDriveTableCluster = -1;
                FAT32_MaxClusters = (PartitionSectors - (SectorsPerFAT + ReservedSectors + SectorNumber)) / SectorsPerCluster;

            }
            else 
            {
                FatType = eFATType.FAT32;
                FAT16_NumRootEntries = 0;
                BytesPerSector = (int)b[0x0b] + (((int)b[0x0c]) << 8);
                SectorsPerCluster = b[0x0d];
                ReservedSectors = (int)b[0x0e] + (((int)b[0x0f]) << 8);
                FATCopies = (int)b[0x10];
                int MediaDescription = b[0x15];

                SectorsPerTrack = (int)b[0x18] + (((int)b[0x19]) << 8);
                Heads = (int)b[0x1a] + (((int)b[0x1b]) << 8);
                NumberOfSectors = (int)b[0x1c] + (((int)b[0x1d]) << 8) + (((int)b[0x1e]) << 16) + (((int)b[0x1f]) << 24);
                PartitionSectors = (int)b[0x20] + (((int)b[0x21]) << 8) + (((int)b[0x22]) << 16) + (((int)b[0x23]) << 24);
                SectorsPerFAT = (int)b[0x24] + (((int)b[0x25]) << 8) + (((int)b[0x26]) << 16) + (((int)b[0x27]) << 24);
                FATFlags = (int)b[0x28] + (((int)b[0x29]) << 8);
                FATDriveVersion = (int)b[0x2a] + (((int)b[0x2b]) << 8);
                // Root directory
                RootDriveTableCluster = (int)b[0x2c] + (((int)b[0x2d]) << 8) + (((int)b[0x2e]) << 16) + (((int)b[0x2f]) << 24);
                FileSystemInfoSector = (int)b[0x30] + (((int)b[0x31]) << 8);
                BackUpBootSector = (int)b[0x32] + (((int)b[0x33]) << 8);
                int logical_drive = (int)b[0x40];
                CurrentHead = (int)b[0x41];
                Signature = (int)b[0x42];
                ID = (int)b[0x43] + (((int)b[0x44]) << 8) + (((int)b[0x45]) << 16) + (((int)b[0x46]) << 24);

                FAT32_MaxClusters = (PartitionSectors- (SectorsPerFAT+ ReservedSectors + SectorNumber))/ SectorsPerCluster;
                VolumeLabel = "";
                for (int i = 0; i < 11; i++)
                {
                    char c = (char)b[0x47 + i];
                    if (c == 0) break;
                    VolumeLabel += (char)c;
                }

                SystemID = "";
                for (int i = 0; i < 8; i++)
                {
                    char c = (char)b[0x52 + i];
                    if (c == 0) break;
                    SystemID += (char)c;
                }
            }

            // Now work out base of everything
            FATSector = ReservedSectors + SectorNumber;
            SectorBase = (SectorsPerFAT * FATCopies) + ReservedSectors + SectorNumber;
            //Cluster.ClusterSize = SectorsPerCluster * BytesPerSector;
        }

    }
}
