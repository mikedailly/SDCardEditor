using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardEditor
{
    unsafe struct Plus3DOS
    {
        /// <summary>Bytes 0...7	- +3DOS signature - 'PLUS3DOS'</summary>
        public fixed byte Sig[8];

        /// <summary>Byte 8		- 1Ah(26) Soft-EOF(end of file)</summary>
        public byte SoftEOF;

        /// <summary></summary>Byte 9		- Issue number</summery>
        public byte IssueNumber;

        /// <summery>Byte 10		- Version number</summery>
        public byte VersionNumber;

        /// <summery>Bytes 11...14	- Length of the file in bytes, 32 bit number, least significant byte in lowest address</summery>
        public uint FileSize;

        /// <summery>Bytes 15...22	- +3 BASIC header data</summery>
        public fixed byte BasicHeader[8];

        /// <summery>Bytes 23...126	- Reserved(set to 0)</summery>
        public fixed byte Reserved[104];

        /// <summery>Byte 127	- Checksum(sum of bytes 0...126 modulo 256)</summery>
        public byte Checksum;            
    }
}
