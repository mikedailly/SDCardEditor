using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDCardAccess;

namespace SDCardEditor
{
    public class global
    {
        /// <summary>The </summary>
        public SDCard Card;
        /// <summary>The </summary>
        public string CurrentDirectory;



        public global()
        {
            Card = null;
            CurrentDirectory = "";
        }
    }
}
