using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDCardEditor
{
    public partial class TextViewer : Form
    {
        // ************************************************************************************************************
        /// <summary>
        ///     Create and open a simple text viewer
        /// </summary>
        /// <param name="_text"></param>
        // ************************************************************************************************************
        public TextViewer(string _text)
        {
            InitializeComponent();

            textBox1.Text = _text;
            textBox1.Font = new Font(textBox1.Font.FontFamily, 14.0f);
            textBox1.Select(0, 0);
        }

        // ************************************************************************************************************
        /// <summary>
        ///     "close" button pressed - so close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // ************************************************************************************************************
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ************************************************************************************************************
        /// <summary>
        ///     Show a text window
        /// </summary>
        /// <param name="_file">file in bytes</param>
        // ************************************************************************************************************
        public static void View(byte[] _file)
        {
            StringBuilder sb = new StringBuilder(_file.Length + 1);
            byte prev = 255;
            for (int i = 0; i < _file.Length; i++)
            {
                byte b1 = _file[i];
                #region Newline check/fix
                // if a 0x0a, then check for a "pair", if not add a newline replacement
                if (b1 == 0x0a)
                {
                    // part of an 0x0a,0x0d pair? if so carry on....
                    if (prev == 0xd)
                    {
                    }
                    else if (i < (_file.Length - 1) && _file[i + 1] == 0x0d)
                    {
                    }
                    else
                    {
                        // if ONLY 0x0a, then feed both
                        sb.Append("\r\n");
                    }
                }
                if (b1 == 0x0d)
                {
                    // part of an 0x0a,0x0d pair? if so carry on....
                    if (prev == 0xa)
                    {
                    }
                    else if (i < (_file.Length - 1) && _file[i + 1] == 0x0a)
                    {
                    }
                    else
                    {
                        // if ONLY 0x0d, then feed both
                        sb.Append("\r\n");
                    }
                }
                #endregion
                else
                {
                    sb.Append((char)b1);
                }
                prev = b1;
            }
            TextViewer viewer = new TextViewer(sb.ToString());
            viewer.Show();
        }
    }
}
