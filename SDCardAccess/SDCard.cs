//
//  SD card access code. Copyright Mike Dailly, 2020
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    public class SDCard  
    {
        /// <summary>Number of sectors to "cache" by default</summary>
        public const int SECTOR_CACHE_DEFAULT = 2048;
        /// <summary>Number of sectors to "cache" by default</summary>
        public const int CLUSTER_CACHE_DEFAULT = 256;
        /// <summary>anything over this is End of file (fat32)</summary>
        public const long FAT_32_EOF = 0xFFFFFFF8;
        /// <summary>anything over this is End of file (fat32)</summary>
        public const long FAT_16_EOF = 0xFFFF;
        /// <summary>The internal path seperator we'll use</summary>
        public const char PATH_SEPERATOR = '\\';

        
        /// <summary>Number of sectors to "cache"</summary>
        public int ClusterCacheCount;

        /// <summary>The currently opened SD Card</summary>
        FileStream SDFile;
        /// <summary>The Name of the current SD Card image</summary>
        string Filename;
        /// <summary>Size of the SD card image in bytes</summary>
        long CardSize = 0;
        /// <summary>Number of sectors in this card image</summary>
        long TotalNumberSectors = 0;

        /// <summary>Number of sectors in this card image</summary>
        //long fat_begin_lba;
        //long cluster_begin_lba;
        //long sectors_per_cluster;
        //long root_dir_first_sector;


        /// <summary>Number of bytes per cluster</summary>
        int BytesPerCluster;
        /// <summary>Number of bytes per sector (512)</summary>
        int BytesPerSector;
        /// <summary>Name of the image (inside the SD Card)</summary>
        string DiskName = "";
        /// <summary>Current card directory</summary>
        public string CurrentDirectory;

        /// <summary>Boot sector</summary>
        BootSector BootSector;
        /// <summary>The currently selected partition</summary>
        Partition CurrentPartition;
        /// <summary>List of upto 4 partitions</summary>
        List<Partition> Partitions;

        /// <summary>The sector manager and sector cache</summary>
        SectorManager SectorManager;
        /// <summary>The Cluster manager and cluster cache</summary>
        ClusterManager ClusterManager;

        ///<summary>Return number of sectors per fat</summary>
        public int SectorsPerFat
        {
            get
            {
                if (BootSector == null) return -1;
                return BootSector.SectorsPerFAT;
            }
        }

        ///<summary>sd card FAT format</summary>
        public eFATType FatType
        {
            get
            {
                if (BootSector == null) return eFATType.Unknown;
                return BootSector.FatType;
            }
        }

        // ********************************************************************************************
        /// <summary>
        ///     Read Master Boot Rector
        /// </summary>
        // ********************************************************************************************
        private void ReadMBR()
        {
            Partitions = new List<Partition>(4);
            Sector s = SectorManager.ReadSector(0);

            Partition p = new Partition(s, 0);
            if (p.LBABegin != 0) Partitions.Add(p);
            p = new Partition(s, 1);
            if (p.LBABegin != 0) Partitions.Add(p);
            p = new Partition(s, 2);
            if (p.LBABegin != 0) Partitions.Add(p);
            p = new Partition(s, 3);
            if (p.LBABegin != 0) Partitions.Add(p);

            CurrentPartition = Partitions[0];
        }

        // ********************************************************************************************
        /// <summary>
        ///     Read BOOT sector info
        /// </summary>
        /// <param name="_sector">The LBABegin of this partition</param>
        // ********************************************************************************************
        private void ReadBootSector(long _sector)
        {
            Sector s = SectorManager.ReadSector(_sector);
            BootSector = new BootSector(s);

            BytesPerSector = BootSector.BytesPerSector;
            SectorManager.SectorSize = BytesPerSector;
            SectorManager.Boot = BootSector;
            BytesPerCluster = BootSector.SectorsPerCluster * BytesPerSector;

            //fat_begin_lba = _sector + BootSector.ReservedSectors;
            //cluster_begin_lba = CurrentPartition.LBABegin + BootSector.ReservedSectors + (BootSector.FATCopies * BootSector.SectorsPerFAT);
            //sectors_per_cluster = BootSector.SectorsPerCluster;
            //root_dir_first_sector = BootSector.SectorBase;
        }

        // ********************************************************************************************
        /// <summary>
        ///     Read part of a long filename
        /// </summary>
        /// <param name="_cluster">The cluster</param>
        /// <param name="_index">index into cluster</param>
        /// <param name="_count">number of characters</param>
        /// <returns>
        ///     String fragment
        /// </returns>
        // ********************************************************************************************
        private string Longfilenames_GetNamePartial(Cluster _cluster, int _index, int _count)
        {
            var s = "";
            _count *= 2;
            for (var cc = 0; cc < _count; cc += 2) {
                int n = ((int)_cluster[_index + cc]) | ((int)_cluster[_index + cc + 1] << 8);
                if (n == 0x0000) break;
                if (n != 0xffff){
                    s = s + (char)n;
                }   
            }
            return s;
        }
        // ********************************************************************************************
        /// <summary>
        ///     Get a FAT sector
        /// </summary>
        /// <param name="_index">which FAT sector to read</param>
        /// <returns>
        ///     The FAT sector or null for past the end of FAT
        /// </returns>
        // ********************************************************************************************
        public Sector ReadFATBlock(int _index)
        {
            if (BootSector.SectorsPerFAT < _index) return null;

            int sector_index = BootSector.FATSector + _index;
            Sector sector = SectorManager.ReadSector(sector_index);
            return sector;
        }

        // ********************************************************************************************
        /// <summary>
        ///     Get the next cluster in the "chain"
        /// </summary>
        /// <param name="_cluster"></param>
        /// <returns>
        ///     The next cluster, or null for end of chain
        /// </returns>
        // ********************************************************************************************
        public Cluster GetNextCluster(Cluster _cluster)
        {
            int cluster_number = (int)_cluster.Number;
            long NextCluster;

            if ( BootSector.FatType == eFATType.FAT32 )
            {
                // Look up the next cluster in the FAT
                // Where does this cluster live? ((*4 for FAT32) / number of bytes per sector) = FAT sector offset
                var sector = (cluster_number * 4) / BytesPerSector;      // which sector is this entry in?
                var ClusterOffset = (cluster_number * 4) - (sector * BytesPerSector);
                sector += BootSector.FATSector;

                Sector sector_data = SectorManager.ReadSector( sector );
                NextCluster = sector_data[ClusterOffset] | (sector_data[ClusterOffset + 1] << 8) | (sector_data[ClusterOffset + 2] << 16) | (sector_data[ClusterOffset + 3] << 24);
                if (NextCluster == 0xffffffff) return null;

                NextCluster = (int)(NextCluster & 0xffffffff);
                if (NextCluster > FAT_32_EOF) return null;
                if (NextCluster > BootSector.FAT32_MaxClusters) return null;

            } else {
                // Look up the next cluster in the FAT
                var sector = (cluster_number * 2) / BytesPerSector;      // which sector is this entry in?
                var ClusterOffset = (cluster_number * 2) - (sector * BytesPerSector);
                sector += BootSector.FATSector;

                Sector sector_data = SectorManager.ReadSector(sector);
                NextCluster = sector_data[ClusterOffset] | (sector_data[ClusterOffset + 1] << 8);
                NextCluster = (int)(NextCluster & 0xffff);
                if (NextCluster == FAT_16_EOF) return null;
            }
            Cluster c = ClusterManager.ReadCluster((int)NextCluster);
            return c;
        }

        // ********************************************************************************************
        /// <summary>
        ///     Read a directory from a sector
        /// </summary>
        /// <param name="_cluster">Cluster to read from -1 for root directory</param>
        // ********************************************************************************************
        public List<DirectoryEntry> ReadDirectory(int _cluster)
        {
            bool first_sector = true;
            bool root = false;

            int root_entries = -1;
            if (_cluster == BootSector.RootDriveTableCluster)
            {
                if (BootSector.FatType == eFATType.FAT16)
                {
                    root_entries = BootSector.FAT16_NumRootEntries;
                }
                root = true;
            }

            List<DirectoryEntry> directory = new List<DirectoryEntry>();
            while (true)
            {
                Cluster cluster = ClusterManager.ReadCluster(_cluster);
                int DirEntries = BytesPerCluster / 32;

                // If first sector, then read diskname
                int index = 0;
                if (first_sector && root)
                {
                    DiskName = "";
                    for (int i = 0; i <= 11; i++)
                    {
                        byte b = cluster[i];
                        if (b > 31)
                        {
                            DiskName = DiskName + (char)b;
                        }
                    }
                    DiskName = DiskName.Trim();
                    index += (32);

                    DirEntries--;
                    root_entries--;
                    first_sector = false;
                }


                // Now read directory
                while (DirEntries > 0)
                {
                    // Proper directory entry? (deleted or not used?)
                    if (cluster[index] !=0xE5 && cluster[index] !=0x00 )
                    {
                        var longfilename = false;
                        DirectoryEntry Entry = new DirectoryEntry();
                        Entry.Attribute = (eFileAttribute) cluster[index + 0xb];


                        // Long filename? This is the 1st entry... so scan forward and get the proper name, then skip to the 8.3 for ATTR and size
                        if( (Entry.Attribute&eFileAttribute.LongFilename)== eFileAttribute.LongFilename)
                        {
                            longfilename = true;
                            var name = "";
                            //Check for long filename
                            var seq = cluster[index] & 0x3f;   // get sequance - remove $40 terminator
                            for (int i = 0; i < seq; i++)
                            {
                                var s = Longfilenames_GetNamePartial(cluster, index + 0x01, 5);
                                s = s + Longfilenames_GetNamePartial(cluster, index + 0x0e, 6);
                                s = s + Longfilenames_GetNamePartial(cluster, index + 0x1c, 2);
                                name = s + name;
                                //debug(string(root_entries)+" : partial -  "+name);

                                root_entries--;
                                if (root_entries == 0) break;

                                index += 32;
                                DirEntries--;
                                if (DirEntries == 0)
                                {
                                    // If long filename overruns a cluster, get the next one....
                                    if (BootSector.FatType== eFATType.FAT32 || !root)
                                    {
                                        cluster = GetNextCluster(cluster);
                                        if (cluster == null) break;
                                    }
                                    else
                                    {
                                        _cluster++;
                                    }
                                    
                                    DirEntries = BootSector.SectorsPerCluster * BytesPerSector / 32;
                                    index = 0;
                                }
                            }
                            if (name.EndsWith(".")) {
                                name = name.Substring(0, name.Length - 1);
                            }

                            Entry.Filename= name;
                        }

                        // First get SHORT filename
                        string shortname = "";
                        for (var i = 0; i < 11; i++)
                        {
                            if (i == 8)
                            {
                                shortname = shortname.Trim();
                                shortname += ".";
                            }
                            if (cluster[index + i] > 31)
                            {
                                shortname += (char)(cluster[index + i]);
                            }
                            else
                            {
                                shortname+=" ";
                            }
                        }
                        shortname = shortname.Trim();
                        if (shortname.EndsWith(".")){
                            shortname = shortname.Substring(0, shortname.Length - 1);
                        }

                        if (longfilename == false) Entry.Filename = shortname;          // If not longfilename, short name IS name
                        Entry.ShortName = shortname;                                    // 8.3 name


                        Entry.Attribute = (eFileAttribute) cluster[index +0xb];           // attribute
                        Entry.Cluster = (cluster[index +0x1b] << 8) | cluster[index +0x1a];  // Cluster
                        if (BootSector.FatType == eFATType.FAT32) Entry.Cluster |=  (cluster[index +0x15] << 24) | (cluster[index +0x14] << 16);  // 32bit Cluster
                        Entry.FileSize = (cluster[index +0x1f] << 24) | (cluster[index +0x1e] << 16) | (cluster[index +0x1d] << 8) | cluster[index +0x1c];  // file size        
                        int CreationTime = cluster[index + 0x0e] | (cluster[index + 0x0f] << 8);
                        int CreationDate = cluster[index + 0x10] | (cluster[index + 0x11] << 8);
                        int WriteTime = cluster[index + 0x16] | (cluster[index + 0x17] << 8);
                        int WriteDate = cluster[index + 0x18] | (cluster[index + 0x19] << 8);
                        Entry.SetCreationDate(CreationDate);
                        Entry.SetCreationTime(CreationTime);
                        Entry.SetLastWriteDate(WriteDate);
                        Entry.SetLastWriteTime(WriteTime);

                        directory.Add(Entry);
                    }
                    index += 32;
                    DirEntries--;
                    root_entries--;
                    if (root_entries == 0) break;
                }


                if (BootSector.FatType == eFATType.FAT32 || !root)
                {
                    cluster = GetNextCluster(cluster);
                    if (cluster == null) break;
                }
                else
                {
                    if (root_entries == 0) break;
                    _cluster++;
                }
            }
            return directory;
        }

        #region Path and Filename functions
        // ****************************************************************************************************
        /// <summary>
        ///     Make sure the path uses consistent "\" character
        /// </summary>
        /// <param name="_path"></param>
        /// <returns>
        /// </returns>
        // ****************************************************************************************************
        public string FixPath(string _path)
        {
            string p = "";
            for(int i = 0; i < _path.Length; i++)
            {
                char c = _path[i];
                if( c=='/' || c=='\\' ) c= PATH_SEPERATOR;
                p += c;
            }
            return p;
        }

        // ****************************************************************************************************
        /// <summary>
        ///     Convert a path from relative to a full absolute path
        /// </summary>
        /// <param name="_path">relative file + path    </param>
        /// <returns></returns>
        // ****************************************************************************************************
        public string GetFullPath(string _path)
        {
            if( _path[0]!= '\\' && _path[0] != '/'){
                _path = CurrentDirectory + PATH_SEPERATOR + _path;
            }

            string[] segments = _path.Split(new[] { '\\', '/' });
            List<string> dirs = new List<string>();

            // work out relative to absolute path
            int index = 0;
            while (index< segments.Length)
            {
                if ( string.IsNullOrEmpty( segments[index] )) {
                    // ignore empty ones
                }
                else if (segments[index] == ".")
                {
                    // ignore
                }
                else if (segments[index] == "..")
                {
                    if (dirs.Count > 0)
                    {
                        dirs.RemoveAt(dirs.Count - 1);
                    }
                }
                else
                {
                    dirs.Add(segments[index]);
                }
                index++;
            }

            // make final path
            string s = "";
            for(int i=0;i<dirs.Count;i++)
            {
                string d = dirs[i];
                if(i==0) {
                    s = d;
                }else{
                    s += PATH_SEPERATOR + d;
                }
            }
            return s;
        }
        #endregion

        // ****************************************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_path"></param>
        // ****************************************************************************************************
        public DirectoryEntry FindItem(string _path)
        {
            // fix the path
            _path = FixPath(_path);

            string[] dir = _path.Split(PATH_SEPERATOR);
            int dir_index = 0;

            // Find the directory
            int CurrentCluster = BootSector.RootDriveTableCluster;
            DirectoryEntry CurrentEntry = null;
            while (dir_index<dir.Length)
            {
                List<DirectoryEntry> entries = ReadDirectory(CurrentCluster);

                string name = dir[dir_index].ToLower();
                bool found = false;
                foreach (DirectoryEntry e in entries)
                {
                    if( name == e.Filename.ToLower() )
                    {
                        CurrentEntry = e;
                        CurrentCluster = e.Cluster;
                        found = true;
                        break;
                    }
                }
                if (!found) return null;
                dir_index++;
            }

            return CurrentEntry;
        }

        // ********************************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_folder"></param>
        /// <returns>
        /// </returns>
        // ********************************************************************************************
        public List<DirectoryEntry> ReadDirectory(string _folder)
        {
            _folder = FixPath(_folder);

            if(_folder.Length>0 && _folder[0] != PATH_SEPERATOR)
            {
                _folder = CurrentDirectory + PATH_SEPERATOR + _folder;
            }
            if(_folder.Length > 0 && _folder[0] == PATH_SEPERATOR)
            {
                _folder = _folder.Substring(1);
            }

            // root folder?
            if ( _folder=="" || _folder== ""+PATH_SEPERATOR){
                return ReadDirectory(BootSector.RootDriveTableCluster);
            }

            DirectoryEntry d = FindItem(_folder);
            if (d == null) return null;

            List<DirectoryEntry> list= ReadDirectory(d.Cluster);
            return list;
        }


        // ********************************************************************************************
        /// <summary>
        ///     Get the directory path of a filena,e
        /// </summary>
        /// <param name="_filename">path+filename to get directory from</param>
        /// <returns>
        ///     filename portion of a full path+file
        /// </returns>
        // ********************************************************************************************
        public string GetDirectory(string _filename)
        {
            _filename = FixPath(_filename);
            int i = _filename.LastIndexOf(PATH_SEPERATOR);
            if( i>0)
            {
                _filename = _filename.Substring(0, i);
                return _filename;
            }
            return "";
        }

        // ********************************************************************************************
        /// <summary>
        ///     Get the filename and extension from a full path
        /// </summary>
        /// <param name="_filename">path+filename  to get filename from</param>
        /// <returns>
        ///     filename portion of a full path+file
        /// </returns>
        // ********************************************************************************************
        public string GetFilename(string _filename)
        {
            _filename = FixPath(_filename);
            string[] s = _filename.Split(PATH_SEPERATOR);
            return s[s.Length - 1];
        }


        // ********************************************************************************************
        /// <summary>
        ///     Load a file from the disk image into memory
        /// </summary>
        /// <param name="_file"></param>
        /// <returns>
        ///     byte[] holding the file, or null for error
        /// </returns>
        // ********************************************************************************************
        public byte[] LoadFile(string _file)
        {
            if (_file.Length > 0 && _file[0] != PATH_SEPERATOR)
            {
                _file = CurrentDirectory + PATH_SEPERATOR + _file;
            }
            _file = FixPath(_file);
            if (_file.Length > 0 && _file[0] == PATH_SEPERATOR)
            {
                _file = _file.Substring(1);
            }

            DirectoryEntry d = FindItem(_file);
            if (d == null) return null;

            long total = d.FileSize;
            long index = 0;
            byte[] buffer = new byte[total];
            int c = d.Cluster;
            Cluster cluster = ClusterManager.ReadCluster(c);
            while (index<total)
            {
                long size = cluster.Length;
                if ((index+size) > total) size = total - index;
                for(int i = 0; i < size; i++)
                {
                    buffer[index++] = cluster[i];
                }
                cluster = GetNextCluster(cluster);
            }
            return buffer;
        }

        // ********************************************************************************************
        /// <summary>
        ///     Load a file from the disk image into memory
        /// </summary>
        /// <param name="_file">file to copy</param>
        /// <param name="_dest">Destination path</param>
        /// <returns>
        ///     true for copied, false for error
        /// </returns>
        // ********************************************************************************************
        public bool ExportFile(string _file, string _dest)
        {
            if (_file.Length > 0 && _file[0] != PATH_SEPERATOR)
            {
                _file = CurrentDirectory + PATH_SEPERATOR + _file;
            }
            _file = FixPath(_file);
            if (_file.Length > 0 && _file[0] == PATH_SEPERATOR)
            {
                _file = _file.Substring(1);
            }


            DirectoryEntry d = FindItem(_file);
            if (d == null) return false;

            string p = Path.GetDirectoryName(_dest);
            try
            {
                Directory.CreateDirectory(p);
            }
            catch
            {
                return false;
            }
            FileStream dest_file = File.Open(_dest, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite);

            long total = d.FileSize;
            long index = 0;
            int c = d.Cluster;
            Cluster cluster = ClusterManager.ReadCluster(c);
            while (index < total)
            {
                long size = cluster.Length;
                if ((index + size) > total) size = total - index;
                byte[] buff = new byte[size];
                for (int i = 0; i < size; i++){
                    buff[i] = cluster[i];
                }
                dest_file.Write(buff, 0, (int)size);

                cluster = GetNextCluster(cluster);
                index += size;
            }
            dest_file.Flush();
            dest_file.Close();
            return true;
        }

        // ********************************************************************************************
        /// <summary>
        ///     Close the SD card - flushing any modifications while we're at it.
        /// </summary>
        // ********************************************************************************************
        public void Close()
        {
            SectorManager.Free();
            SDFile.Flush();
            SDFile.Close();
        }

        // ********************************************************************************************************************
        /// <summary>
        ///     Allocate a cluster
        /// </summary>
        /// <returns>
        ///     Free cluster number
        /// </returns>
        // ********************************************************************************************************************
        private int AllocatateCluster()
        {
            return AllocatateCluster(-1);
        }

        // ********************************************************************************************************************
        /// <summary>
        ///     Allocate a cluster
        /// </summary>
        /// <param name="_previous">Cluster number to link "from"</param>
        /// <returns>
        /// </returns>
        // ********************************************************************************************************************
        private int AllocatateCluster(int _previous)
        {
            int MaxClusters = BootSector.FAT32_MaxClusters;
            int MaxFatSectors = BootSector.SectorsPerFAT;
            int sect = BootSector.FATSector;
            bool first = true;
            int found_cluster = 0;
            for (int i = 0; i < MaxFatSectors; i++)
            {
                Sector sector = SectorManager.ReadSector(sect);
                if (BootSector.FatType == eFATType.FAT16)
                {
                    int clusters_persector = BootSector.BytesPerSector/2;
                    int cnt = BootSector.BytesPerSector;
                    int start = 0;
                    if (first) start = 2;
                    for (int cl = start; cl < cnt; cl+=2)
                    {
                        int fat_entry = sector.Read16(cl);
                        // if entry is 0, then it's
                        if (fat_entry == 0)
                        {
                            found_cluster = i * 1;
                        }
                    }
                }else
                {
                    int cnt = BootSector.BytesPerSector / 4;
                    int start = 0;
                    if (first) start = 2;
                    for (int cl = start; cl < cnt; cl++)
                    {

                    }
                }
            }
            return 0;
        }

        public  void MakeDir(string _name)
        {

        }


        // ********************************************************************************************
        /// <summary>
        ///     
        /// </summary>
        /// <param name="_filename"></param>
        // ********************************************************************************************
        private SDCard( string _filename )
        {

            Filename = _filename;
            try
            {
                SDFile = File.Open(_filename, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite);
                CardSize = SDFile.Length;
                TotalNumberSectors = CardSize / Sector.DEFAULT_SECTOR_SIZE;
            }
            catch
            {
                SDFile = null;
                Filename = "";
                return;
            }

            SectorManager = new SectorManager(SECTOR_CACHE_DEFAULT, SDFile);
            ClusterCacheCount = CLUSTER_CACHE_DEFAULT;
            CurrentDirectory = "";

            ReadMBR();
            ReadBootSector(CurrentPartition.LBABegin);
            ClusterManager = new ClusterManager(CLUSTER_CACHE_DEFAULT, BootSector, SectorManager, SDFile);

            ReadDirectory(BootSector.RootDriveTableCluster);

        }






        #region Static access
        // ********************************************************************************************
        /// <summary>
        ///     Create a new SD card handler
        /// </summary>
        /// <param name="_filename">Name+Path of SD Card</param>
        // ********************************************************************************************
        public static SDCard Open(string _filename)
        {
            if (!File.Exists(_filename)) return null;
            SDCard card = new SDCard(_filename);

            // if the card failed to open, return null
            if (card.SDFile == null) return null;

            return card;
        }
        #endregion
    }
}
