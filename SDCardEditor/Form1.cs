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

        public Form1()
        {
            global = new global();

            InitializeComponent();

            // default (test) SD card path
            SDCardPath.Text = @"E:\ZXNext_Images\cspect-next-8gb.img";
            CurrentDirLabel.Text = "";


            // setup column types
            listView1.Columns.Add("Name", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Date", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Attribute", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Cluster", -2, HorizontalAlignment.Left);
            AdjustColumnSize();
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
            List<DirectoryEntry> dir = global.Card.ReadDirectory(_directory);
            CurrentDirLabel.Text = _directory;
            CurrentDirLabel.ForeColor = Color.Black;

            listView1.Items.Clear();
            if (dir == null || dir.Count == 0)
            {
                CurrentDirLabel.ForeColor = Color.Red;
                return;
            }

            // Now fill in directory
            foreach (DirectoryEntry entry in dir)
            {
                string[] item = new string[] { entry.Filename, entry.FileSize.ToString(), "1/1/1", entry.Attribute.ToString(), entry.Cluster.ToString() };
                listView1.Items.Add( new ListViewItem(item));
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
            string dir = listView1.SelectedItems[0].SubItems[0].Text;

            global.CurrentDirectory += "\\" + dir;
            global.CurrentDirectory = global.Card.GetFullPath(global.CurrentDirectory);
            SetDirectory(global.CurrentDirectory);

        }


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
    }
}
