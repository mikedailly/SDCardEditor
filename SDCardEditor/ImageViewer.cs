using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDCardEditor
{
    public partial class ImageViewer : Form
    {
        // ************************************************************************************************************
        /// <summary>
        ///     Simple image viewer
        /// </summary>
        /// <param name="_image">Image bitmap to view</param>
        /// <param name="_extension">extension type (".bmp" etc)</param>
        // ************************************************************************************************************
        public ImageViewer(byte[] _image, string _extension)
        {
            InitializeComponent();

            Size s = pictureBox1.Size;

            string temp_file = Environment.ExpandEnvironmentVariables("%localappdata%");
            temp_file  = Path.Combine(temp_file, "SDCardEditor");
            if(!Directory.Exists(temp_file)) {
                try
                {
                    Directory.CreateDirectory(temp_file);
                }catch{
                    return;
                }
            }
            temp_file = Path.Combine(temp_file, "tmp" + _extension);
            System.IO.File.WriteAllBytes(temp_file, _image);

            pictureBox1.ImageLocation = temp_file;
            pictureBox1.Size = s;

        }

        // ************************************************************************************************************
        /// <summary>
        ///     Close button pressed - so close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // ************************************************************************************************************
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
