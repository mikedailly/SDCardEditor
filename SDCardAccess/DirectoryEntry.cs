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
        /// <summary>Date/Time of the file/directory creation</summary>
        public DateTime CreationTime;
        /// <summary>Date/Time of the file/directory Write time</summary>
        public DateTime LastWriteTime;
        /// <summary>Date the last time the entry was accessed</summary>
        public DateTime LastAccessDate;
        /// <summary>Millisecond stamp (actual 100th of a second)</summary>
        public int CreationTimeMS;


        /// <summary>MSDOS date time value</summary>
        UInt32 MSDOS_CreateDateTime;
        /// <summary>MSDOS date time value</summary>
        UInt32 MSDOS_LastWriteDateTime;
        /// <summary>MSDOS date value</summary>
        //UInt32 MSDOS_LastAccessDate;

        // *********************************************************************************************************
        /// <summary>
        ///     Is the Create Date/Time valid
        /// </summary>
        // *********************************************************************************************************
        public bool IsCreateValid
        {
            get
            {
                if (MSDOS_CreateDateTime == 0) return false; else return true;
            }
        }


        // *********************************************************************************************************
        /// <summary>
        ///     Is the LastWrite Date/Time valid
        /// </summary>
        // *********************************************************************************************************
        public bool IsLastWriteValid
        {
            get
            {
                if (MSDOS_LastWriteDateTime == 0) return false; else return true;
            }
        }


        // *********************************************************************************************************
        /// <summary>
        ///     Check that the day is valid
        /// </summary>
        /// <param name="_year">Year we're checking</param>
        /// <param name="_month">Month to validate</param>
        /// <param name="_day">Day to check</param>
        /// <returns>
        ///     Clamped day
        /// </returns>
        // *********************************************************************************************************
        int CheckDay(int _year, int _month, int _day)
        {
            if (_day < 0) return 0;

            int[] Days = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            if (_month == 2)
            {
                float f = (float)_year / 4.0f;
                if ((f - Math.Floor(f)) != 0)
                {
                    if (_day > 29) return 29;
                    return _day;
                }
            }
            _month--;
            if (_day > Days[_month]) _day = Days[_month];
            return _day;
        }

        // *********************************************************************************************************
        /// <summary>
        ///     Set actual time on a DateTime
        /// </summary>
        /// <remarks>
        ///       15--------------11 10------------5   4------------0
        ///          Hours (0-23)      Minutes 0-59    Seconds (0-29)
        /// </remarks>
        /// <param name="_time">time to set (in MSDOS DISK format)</param>
        // *********************************************************************************************************
        private DateTime SetTime(DateTime _datetime, int _time)
        {
            int seconds = (_time & 0x1f) << 1;
            int minutes = ((_time&0x7e0) >> 5);
            int hours = ((_time&0xf800) >> 11);

            int day = _datetime.Day;
            int month = _datetime.Month;
            int year = _datetime.Year;

            /*
            if (month > 12) month = 12;
            if (month < 0) month = 0;
            if (hours > 23) hours = 23;
            if (hours < 0) hours = 0;
            if (minutes > 59) minutes = 59;
            if (minutes < 0) minutes = 0;
            if (seconds > 59) seconds = 59;
            if (seconds < 0) seconds = 0;
            day = CheckDay(year, month, day);
            */
            _datetime = new DateTime(year, month, day, hours, minutes, seconds, 0);
            return _datetime;
        }

        // *********************************************************************************************************
        /// <summary>
        ///     Set the creation write time
        /// </summary>
        /// <remarks>
        ///       15--------------11 10------------5   4------------0
        ///          Hours (0-23)      Minutes 0-59    Seconds (0-29)
        /// </remarks>
        /// <param name="_time">time to set (in MSDOS DISK format)</param>
        // *********************************************************************************************************
        public void SetCreationTime(int _time)
        {
            MSDOS_CreateDateTime = ((MSDOS_CreateDateTime & 0xffff0000) | ((UInt32)_time & 0xffff));
            CreationTime = SetTime(CreationTime, _time);
        }

        // *********************************************************************************************************
        /// <summary>
        ///     Set the last write time
        /// </summary>
        /// <remarks>
        ///       15--------------11 10------------5   4------------0
        ///          Hours (0-23)      Minutes 0-59    Seconds (0-29)
        /// </remarks>
        /// <param name="_time">time to set (in MSDOS DISK format)</param>
        // *********************************************************************************************************
        public void SetLastWriteTime(int _time)
        {
            MSDOS_LastWriteDateTime = ((MSDOS_LastWriteDateTime & 0xffff0000) | ((UInt32)_time & 0xffff));
            LastWriteTime = SetTime(LastWriteTime, _time);
        }


        // *********************************************************************************************************
        /// <summary>
        ///     Set actual date on a DateTime
        /// </summary>
        /// <param name="_date">Date to set (in MSDOS DISK format)</param>
        // *********************************************************************************************************
        private DateTime SetDate(DateTime _datetime, int _date)
        {
            int day = (_date & 0x1f);
            if (day == 0) day = 1;
            int month = ((_date >> 5) & 0xf);
            if (month == 0) month = 1;
            int year = 1980+((_date >> 9) & 0x2f);

            int sec = _datetime.Second;
            int min = _datetime.Minute;
            int hour = _datetime.Hour;

            if (month > 12) month = 12;
            if (month < 0) month = 0;
            if (hour > 23) hour = 23;
            if (hour < 0) hour = 0;
            if (min > 59) min = 59;
            if (min < 0) min = 0;
            if (sec > 59) sec = 59;
            if (sec < 0) sec = 0;
            day = CheckDay(year, month, day);

            _datetime = new DateTime(year, month, day, hour, min, sec, 0);
            return _datetime;
        }

        // *********************************************************************************************************
        /// <summary>
        ///     Set the last write time
        /// </summary>
        /// <remarks>
        /// 15-------------------------9 8----------5 4----------0
        ///        year 1980-2107         Month 1-12   Day (1-31)
        /// </remarks>
        /// <param name="_date">Date to set (in MSDOS DISK format)</param>
        // *********************************************************************************************************
        public void SetCreationDate(int _date)
        {
            MSDOS_CreateDateTime = ((MSDOS_CreateDateTime & 0xffff) | (((UInt32)_date & 0xffff) << 16));
            CreationTime = SetDate(CreationTime, _date);
        }

        // *********************************************************************************************************
        /// <summary>
        ///     Set the last write time
        /// </summary>
        /// <remarks>
        /// 15-------------------------9 8----------5 4----------0
        ///        year 1980-2107         Month 1-12   Day (1-31)
        /// </remarks>
        /// <param name="_date">Date to set (in MSDOS DISK format)</param>
        // *********************************************************************************************************
        public void SetLastWriteDate(int _date)
        {
            MSDOS_LastWriteDateTime = ((MSDOS_LastWriteDateTime & 0xffff) | (((UInt32)_date & 0xffff)<<16) );
            LastWriteTime = SetDate(LastWriteTime, _date);
        }


        // *********************************************************************************************************
        /// <summary>
        ///     Create a new Directory entry
        /// </summary>
        // *********************************************************************************************************
        public DirectoryEntry()
        {
            CreationTime = DateTime.Now;
            LastWriteTime = DateTime.Now;
            LastAccessDate = DateTime.Now;

            MSDOS_CreateDateTime = 0;
            MSDOS_LastWriteDateTime = 0;
            //MSDOS_LastAccessDate = 0;

            Filename = "";
            ShortName = "";
            FileSize = 0;
            Cluster = -1;

        }

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
