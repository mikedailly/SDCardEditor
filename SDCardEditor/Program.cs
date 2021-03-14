using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDCardAccess;

namespace SDCardEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //string name = @"C:\source\ZXSpectrum\_Demos\2gbimage\cspect-next-2gb.img";
            //SDCard card = SDCard.Open(name);
            //string s = card.GetFullPath("\\demos\\WidescreenImageDemo\\test\\..\\help\\more\\..\\.\\..\\test.txt");
            //List<DirectoryEntry> d = card.ReadDirectory("demos\\WidescreenImageDemo");
            //byte[] f = card.LoadFile("demos\\WidescreenImageDemo\\readme.txt");

            /*
            string s = "";
            for (int i = 0; i < f.Length; i++)
            {
                s = s + (char)f[i];
            }*/

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
