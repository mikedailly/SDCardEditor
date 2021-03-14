using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
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

        /// <summary>Status or physical drive (bit 7 set is for active or bootable, old MBRs only accept 0x80, 0x00 means inactive, and 0x01–0x7F stand for invalid)</summary>
        public int BootFlags;
        /// <summary>CHS address of first absolute sector in partition. The format is described by three bytes, see the next three rows.</summary>
        public int CHSBegin;
        /// <summary>Partition type</summary>
        public int TypeCode;
        /// <summary>CHS address of last absolute sector in partition. The format is described by 3 bytes, see the next 3 rows.</summary>
        public int CHSEnd;
        /// <summary>LBA of first absolute sector in the partition</summary>
        public long LBABegin;
        /// <summary>Number of sectors in partition</summary>
        public int SectorCount;

        // ************************************************************************************************************************************
        /// <summary>
        ///     Create a new partition entry
        /// </summary>
        /// <param name="_sector">The sector holding the partition table</param>
        /// <param name="_partition_number">Which partition are we reading out (1 to 3)</param>
        // ************************************************************************************************************************************
        public Partition(Sector _sector, int _partition_number)
        {
            byte[] b = _sector.buffer;
            int index = PARTITION_OFFSET + (PARTITION_SIZE * _partition_number);

            BootFlags = b[index++];

            CHSBegin = (int)b[index] + (((int)b[index+1])<<8) | (((int)b[index + 2]) << 16);
            index += 3;

            TypeCode = b[index++];

            CHSEnd = (int)b[index] + (((int)b[index + 1]) << 8) | (((int)b[index + 2]) << 16);
            index += 3;

            LBABegin = (long)b[index] + (((long)b[index + 1]) << 8) | (((long)b[index + 2]) << 16) | (((long)b[index + 3]) << 24);
            index += 4;

            SectorCount = (int)b[index] + (((int)b[index + 1]) << 8) | (((int)b[index + 2]) << 16) | (((int)b[index + 3]) << 24);
        }
    }
}
