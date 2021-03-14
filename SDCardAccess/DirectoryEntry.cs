using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    // *********************************************************************************************************
    /// <summary>
    ///     Possible file attributes
    /// </summary>
    // *********************************************************************************************************
    [Flags]
    public enum eFileAttribute
    {
        /// <summary>Long filename entry</summary>
        LongFilename = 0xf,
        /// <summary>Should not allow writing</summary>
        ReadOnly = 1,
        /// <summary>Should not show in dir listing</summary>
        Hidden = 2,
        /// <summary>File is operating system</summary>
        System = 4,
        /// <summary>Filename is Volume ID</summary>
        VolumeID = 8,
        /// <summary>Is a subdirectory (32-byte records)</summary>
        Directory = 16,
        /// <summary>Has been changed since last backup</summary>
        Archive = 32
    }

    // *********************************************************************************************************
    /// <summary>
    ///     A single entry in the Directory (file or directory)
    /// </summary>
        // *********************************************************************************************************
    public class DirectoryEntry
    {
        /// <summary>Long filename</summary>
        public string Filename;
        /// <summary>Short 8.3 filename</summary>
        public string ShortName;
        /// <summary>File attributes</summary>
        public eFileAttribute Attribute;
        /// <summary>Start cluster</summary>
        public int Cluster;
        /// <summary>File size in bytes</summary>
        public long FileSize;
        /// <summary>Date/Time of the file/directory</summary>
        public DateTime time;

        // *********************************************************************************************************
        /// <summary>
        ///     Simple debugging output
        /// </summary>
        /// <returns>
        ///     String summery
        /// </returns>
        // *********************************************************************************************************
        public override string ToString()
        {
            return Filename + ":(c)" + Cluster.ToString() + ":(s)" + FileSize.ToString();
        }
    }
}
