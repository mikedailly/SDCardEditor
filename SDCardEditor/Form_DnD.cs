using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDCardAccess;

namespace SDCardEditor
{
    public partial class Form1 : Form
    {
        public const string TEMP_FOLDER = "%localappdata%\\SDCardEditor";

        #region Drag and Drop
        public void Init_DnD()
        {
            AllowDrop = true;
            this.DragDrop += Form1_DragDrop;  //new DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += Form1_DragEnter; //new DragEventHandler(this.Form1_DragEnter);
            listView1.AllowDrop = true;
            listView1.ItemDrag += ListView1_ItemDrag;
            
        }

        private void ListView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            List<string> selection = new List<string>();

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                selection.Add(item.Text);
                //int imgIndex = item.ImageIndex;
                //selection.Add(filenames[imgIndex]);
            }

            DataObject data = new DataObject(DataFormats.FileDrop, selection.ToArray());
            DoDragDrop(data, DragDropEffects.Copy);
        }

        // **************************************************************************************************************
        /// <summary>
        ///     Dragging into the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // **************************************************************************************************************
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            // If the data is a file or a bitmap, display the copy cursor.
            //if (e.Data.GetDataPresent(DataFormats.Bitmap) ||
            if( e.Data.GetDataPresent(DataFormats.FileDrop) )
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        // **************************************************************************************************************
        /// <summary>
        ///     Drop ONTO the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // **************************************************************************************************************
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) )
            {
                // Assign the file names to a string array, in case the user has selected multiple files.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // we can now copy these files onto the SD card
                foreach(string file in files)
                { 
                }
            }
        }

        /// <summary>
        /// Write files to clipboard (from
        /// http://blogs.wdevs.com/idecember/
        ///      archive/2005/10/27/10979.aspx)
        /// </summary>
        /// <param name="cut">True if cut, false if copy</param>
        void CopyToClipboard(bool cut)
        {
            List<string> selection = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                selection.Add(item.Text);
            }
            selection = CopyFiles(selection);
            //selection.Add("C:\\Users\\Mike\\Downloads\\test_image.bmp");
            //selection.Add("C:\\Users\\Mike\\Downloads\\TBBLUE.30110.2D.tbu");
            //selection.Add("C:\\Users\\Mike\\Downloads\\TestMe");

            string[] files = selection.ToArray();
            IDataObject data = new DataObject(DataFormats.FileDrop, files);
            MemoryStream memo = new MemoryStream(4);
            byte[] bytes = new byte[] { (byte)(cut ? 2 : 5), 0, 0, 0 };
            memo.Write(bytes, 0, bytes.Length);
            data.SetData("Preferred DropEffect", memo);
            Clipboard.SetDataObject(data);
        }



        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyToClipboard(false);
        }


        private void pasteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            IDataObject data = Clipboard.GetDataObject();
            if (!data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])
              data.GetData(DataFormats.FileDrop);
            MemoryStream stream = (MemoryStream)
              data.GetData("Preferred DropEffect", true);
            int flag = stream.ReadByte();
            if (flag != 2 && flag != 5)
                return;
            bool cut = (flag == 2);
            foreach (string file in files)
            {
                string dest = "\\" +
                              Path.GetFileName(file);
                try
                {
                    if (cut)
                        File.Move(file, dest);
                    else
                        File.Copy(file, dest, false);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(this, "Failed to perform the" +
                        " specified operation:\n\n" + ex.Message,
                        "File operation failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }

        }
        #endregion

        private void CopyRecursive(List<string> _files)
        {

        }

        public List<string> CopyFiles(List<string> _files)
        {
            string temp_path = Environment.ExpandEnvironmentVariables(TEMP_FOLDER);
            List<string> copied_files = new List<string>();

            List<DirectoryEntry> dir = global.Card.ReadDirectory(global.CurrentDirectory);

            // copy each file
            foreach(string file in _files)
            {                
                string f = global.CurrentDirectory + "\\" + file;
                string dest = temp_path + "\\" + file;
                if (global.Card.ExportFile(f, dest))
                {
                    copied_files.Add(dest);
                }
            }


            return copied_files;
        }
    }
}
