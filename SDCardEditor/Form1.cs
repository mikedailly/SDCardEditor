using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

            SDCardPath.Text = @"E:\ZXNext_Images\cspect-next-8gb.img";
            // setup column types
            listView1.Columns.Add("Name", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Date", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Attribute", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Cluster", -2, HorizontalAlignment.Left);
            AdjustColumnSize();
        }

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

        public void SetDirectory(string _directory)
        {
            if (global.Card == null) return;
            List<DirectoryEntry> dir = global.Card.ReadDirectory(_directory);
            listView1.Items.Clear();

            foreach(DirectoryEntry entry in dir)
            {
                string[] item = new string[] { entry.Filename, entry.FileSize.ToString(), "1/1/1", entry.Attribute.ToString(), entry.Cluster.ToString() };
                listView1.Items.Add( new ListViewItem(item));
            }

        }

        private void OpenSDCard_Click(object sender, EventArgs e)
        {
            string file = SDCardPath.Text;
            global.Card = SDCard.Open(file);
            global.CurrentDirectory = "";
            SetDirectory(global.CurrentDirectory);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dir = listView1.SelectedItems[0].SubItems[0].Text;

            global.CurrentDirectory += "\\" + dir;
            global.CurrentDirectory = global.Card.GetFullPath(global.CurrentDirectory);
            SetDirectory(global.CurrentDirectory);

        }
    }
}
