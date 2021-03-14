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
        public static int ClusterSize = 0;



        /// <summary>Sector number</summary>
        public long Number;
        /// <summary>Actual sector data</summary>
        public byte[] buffer;

        public bool dirty;

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
            get { return buffer[_index]; }
            set { buffer[_index] = value; dirty = true; }
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     For FAT16 Root directory (16k)
        /// </summary>
        /// <param name="_size"></param>
        // ***********************************************************************************************************************
        public void SetSize(int _size)
        {
            buffer = new byte[_size];
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     Create a new cluster
        /// </summary>
        /// <param name="_cluster"></param>
        // ***********************************************************************************************************************
        public Cluster(int _cluster)
        {
            Number = _cluster;
            buffer = new byte[ClusterSize];
            dirty = false;
        }


        public override string ToString()
        {
            if (buffer == null) return null;

            StringBuilder sb = new StringBuilder(32768);
            for(int i=0;i< buffer.Length; i++)
            {
                int b = buffer[i];
                if (b >= 32 && b < 256) sb.Append( (char)b);
            }
            return sb.ToString();
        }
    }
}
