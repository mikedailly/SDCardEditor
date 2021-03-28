using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardAccess
{
    // ***********************************************************************************************************************
    /// <summary>
    ///     Sector class, holds and access sector data
    /// </summary>
        // ***********************************************************************************************************************
    public class Sector : IDisposable
    {
        /// <summary>Size of an SD card sector</summary>
        public const int DEFAULT_SECTOR_SIZE = 512;
        /// <summary>Size of an SD card sector</summary>
        public int SectorSize = DEFAULT_SECTOR_SIZE;


        FileStream file;
        private bool isDisposed;

        /// <summary>Sector number</summary>
        long mNumber;

        /// <summary>Sector number</summary>
        public long Number {
            get { return mNumber; }
        }

        /// <summary>Actual sector data</summary>
        public byte[] buffer;

        /// <summary>Has this sector been modified</summary>
        public bool dirty;

        // ***********************************************************************************************************************
        /// <summary>
        ///     Read/Write sector data
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
        ///     Read a 16bit value from the buffer
        /// </summary>
        /// <param name="_index">index into sector</param>
        /// <returns>
        ///     16bit value
        /// </returns>
        // ***********************************************************************************************************************
        public int Read16(int _index)
        {
            if (_index < 0 || _index >= (buffer.Length - 1)) return 0;
            int a = buffer[_index];
            a |= ((int)buffer[_index+1])<<8;
            return a;
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     Write a 16bit value from the buffer
        /// </summary>
        /// <param name="_index">index into sector</param>
        /// <param name="_value">value to write</param>
        // ***********************************************************************************************************************
        public void Write16(int _index,int _value)
        {
            if (_index < 0 || _index >= (buffer.Length - 1)) return;

            dirty = true;
            buffer[_index] = (byte) (_value & 0xff);
            buffer[_index+1] = (byte) ((_value>>8) & 0xff);
        }


        // ***********************************************************************************************************************
        /// <summary>
        ///     Read a 32bit value from the buffer
        /// </summary>
        /// <param name="_index">index into sector</param>
        /// <returns>
        ///     32 bit value
        /// </returns>
        // ***********************************************************************************************************************
        public int Read32(int _index)
        {
            if (_index < 0 || _index >= (buffer.Length - 3)) return 0;
            int a = buffer[_index];
            a |= ((int)buffer[_index + 1]) << 8;
            a |= ((int)buffer[_index + 2]) << 16;
            a |= ((int)buffer[_index + 3]) << 24;
            return a;
        }


        // ***********************************************************************************************************************
        /// <summary>
        ///     Write a 16bit value from the buffer
        /// </summary>
        /// <param name="_index">index into sector</param>
        /// <param name="_value">value to write</param>
        // ***********************************************************************************************************************
        public void Write32(int _index, int _value)
        {
            if (_index < 0 || _index >= (buffer.Length - 3)) return;

            dirty = true;
            buffer[_index] = (byte)(_value & 0xff);
            buffer[_index + 1] = (byte)((_value >> 8) & 0xff);
            buffer[_index + 2] = (byte)((_value >> 16) & 0xff);
            buffer[_index + 3] = (byte)((_value >> 24) & 0xff);
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     If dirty... flush
        /// </summary>
        // ***********************************************************************************************************************
        public void Flush()
        {
            if (dirty)
            {
                file.Seek(mNumber * SectorSize, SeekOrigin.Begin);
                file.Write(buffer, 0, SectorSize);
                dirty = false;
            }
        }


        #region IDispose
        // ***********************************************************************************************************************
        /// <summary>
        ///     Dispose of sector
        /// </summary>
        // ***********************************************************************************************************************
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ***********************************************************************************************************************
        /// <summary>
        ///     The bulk of the clean-up code is implemented in Dispose(bool)
        /// </summary>
        /// <param name="disposing">Are we disposing, or garbage collecting?</param>
        // ***********************************************************************************************************************
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                // free managed resources
                Flush();
            }

            // free native resources if there are any.

            isDisposed = true;
        }
        #endregion 

        // ***********************************************************************************************************************
        /// <summary>
        ///     Create a new sector
        /// </summary>
        /// <param name="_number">Sectore number</param>
        // ***********************************************************************************************************************
        public Sector(long  _number, FileStream _file)
        {
            isDisposed = false;
            file = _file;
            mNumber = _number;
            buffer = new byte[SectorSize];
            dirty = false;
        }
    }
}
