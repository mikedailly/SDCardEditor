using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDCardAccess;

namespace SDCardEditor
{
    // *********************************************************************************************************
    /// <summary>
    ///     SD Card "instance" global store - just to make things a little easier
    /// </summary>
        // *********************************************************************************************************
    public class global
    {
        /// <summary>The </summary>
        public SDCard Card;
        /// <summary>The </summary>
        public string CurrentDirectory;


        // *********************************************************************************************************
        /// <summary>
        ///     Create a new global store
        /// </summary>
        // *********************************************************************************************************
        public global()
        {
            Card = null;
            CurrentDirectory = "";
        }
    }
}
