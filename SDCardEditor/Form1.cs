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
using SDCardAccess;

namespace SDCardEditor
{
    public partial class Form1 : Form
    {
        public global global;

        /// <summary>The current directory listing</summary>
        public List<DirectoryEntry> DirectoryListing;

        public Form1()
        {
            global = new global();

            InitializeComponent();

            // default (test) SD card path
            //SDCardPath.Text = @"E:\ZXNext_Images\cspect-next-2gb.img";
            SDCardPath.Text = @"C:\source\ZXSpectrum\_Demos\_NEXTROM\card.img";
            //SDCardPath.Text = @"C:\source\ZXSpectrum\_Demos\320_L2_TURRICAN.nex";
            CurrentDirLabel.Text = "";


            // setup column types
            listView1.Columns.Add("Name", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Date Created", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Date Modified", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Attribute", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Cluster", -2, HorizontalAlignment.Left);
            listView1.MultiSelect = true;
            AdjustColumnSize();

            Init_DnD();
        }


        // *********************************************************************************************************
        /// <summary>
        ///     Spread thje columns over the area, but make the "filename" part double size
        /// </summary>
        // *********************************************************************************************************
        public void AdjustColumnSize()
        {
            int w = listView1.Width / (listView1.Columns.Count+1);
            for(int i=0;i< listView1.Columns.Count;i++)
            {
                ColumnHeader header = listView1.Columns[i];
                if (i == 0)
                {
                    header.Width = w*2;
                }else
                {
                    header.Width = w;
                }
            }

        }

        // *********************************************************************************************************
        /// <summary>
        ///     Get the directory from the SD card, and fill the view with the entries
        /// </summary>
        /// <param name="_directory">Directory to get</param>
        // *********************************************************************************************************
        public void SetDirectory(string _directory)
        {
            if (global.Card == null) return;
            DirectoryListing = global.Card.ReadDirectory(_directory);
            CurrentDirLabel.Text = _directory;
            CurrentDirLabel.ForeColor = Color.Black;

            listView1.Items.Clear();
            if (DirectoryListing == null || DirectoryListing.Count == 0)
            {
                CurrentDirLabel.ForeColor = Color.Red;
                return;
            }

            // Now fill in directory
            foreach (DirectoryEntry entry in DirectoryListing)
            {
                string created = "";
                string lastwrite = "";
                if (entry.IsCreateValid) created = entry.CreationTime.ToString();
                if (entry.IsLastWriteValid) lastwrite = entry.LastWriteTime.ToString();

                string[] item = new string[] { entry.Filename, entry.FileSize.ToString(), created, lastwrite, entry.Attribute.ToString(), entry.Cluster.ToString() };
                listView1.Items.Add( new ListViewItem(item) );
            }

        }


        // *********************************************************************************************************
        /// <summary>
        ///     Open a new SD card image
        /// </summary>
        /// <param name="_filename">Path+File name to open</param>
        // *********************************************************************************************************
        public void OpenSDCard(string _filename)
        {
            if (!File.Exists(_filename)) return;

            if (global.Card != null) global.Card.Close();

            global.Card = SDCard.Open(_filename);
            global.CurrentDirectory = "";
            SetDirectory(global.CurrentDirectory);
        }


        // *********************************************************************************************************
        /// <summary>
        ///     Button to open SD card press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // *********************************************************************************************************
        private void OpenSDCard_Click(object sender, EventArgs e)
        {
            string file = SDCardPath.Text;
            OpenSDCard(file);
        }


        // *********************************************************************************************************
        /// <summary>
        ///     Double click on a file in the folder view. If it's a folder, select it and go in...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // *********************************************************************************************************
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string file = listView1.SelectedItems[0].SubItems[0].Text;

            foreach (DirectoryEntry entry in DirectoryListing)
            {
                if (entry.Filename == file)
                {
                    if ((entry.Attribute & eFileAttribute.Directory) != 0)
                    {
                        // change Directory
                        global.CurrentDirectory += "\\" + file;
                        global.CurrentDirectory = global.Card.GetFullPath(global.CurrentDirectory);
                        SetDirectory(global.CurrentDirectory);
                    }
                    else if (Path.GetExtension(file).ToLower() == ".txt" || Path.GetExtension(file).ToLower() == ".md")
                    {
                        // Load and view text files
                        file = global.CurrentDirectory + "\\" + file;
                        byte[] data = global.Card.LoadFile(file);
                        TextViewer.View(data);
                    }
                    else if (Path.GetExtension(file).ToLower() == ".bmp" || Path.GetExtension(file).ToLower() == ".png" || Path.GetExtension(file).ToLower() == ".jpg")
                    {
                        // simple image viewer
                        file = global.CurrentDirectory + "\\" + file;
                        byte[] data = global.Card.LoadFile(file);

                        ImageViewer viewer = new ImageViewer(data, Path.GetExtension(file).ToLower());
                        viewer.Show();
                    }
                    /*
                    else if (Path.GetExtension(file).ToLower() == ".bas")
                    {
                        // simple image viewer
                        file = global.CurrentDirectory + "\\" + file;
                        byte[] basic_file = global.Card.LoadFile(file);
                        Basic.Basic2Text(basic_file);

                        //TextViewer.View(data);
                    }*/
                    break;                        
                }
            }
        }

        #region MENU items
        // *********************************************************************************************************
        /// <summary>
        ///     Open File menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // *********************************************************************************************************
        private void openSDCardImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog.Filter = "img files (*.img)|*.img|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                if ( File.Exists(filename))
                {
                    SDCardPath.Text = filename;
                    OpenSDCard(filename);
                }
            }
        }

        #endregion

        private void fATViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (global.Card == null) return;
            FATViewer viewer = new FATViewer(global);
            viewer.Show();
        }
    }
}
