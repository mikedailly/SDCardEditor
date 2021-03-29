using SDCardAccess;
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
    public partial class FATViewer : Form
    {
        global global;
        SDCard mCard;
        int mSector = 0;
        int mMaxSector = 0;
        int mMaxCluster = 0;
        Font mFont;
        Sector mCurrentSector;

        void UpdateMessage()
        {
            int scale = 2;
            if (mCard.FatType == eFATType.FAT32) scale = 4;
            int cluster_start = mSector * 512 / scale;
            int cluster_end = ((mSector+1) * 512 / scale)-1;

            InfoLabel.Text = "Fat: "+ mSector.ToString()+"   Cluster: "+cluster_start.ToString()+"/"+cluster_end.ToString();
        } 

        public FATViewer(global _global)
        {
            global = _global;
            mCard = global.Card;
            mMaxSector = mCard.SectorsPerFat;
            if (mCard.FatType == eFATType.FAT16) {
                mMaxCluster = mMaxSector >> 1;
            }else{
                mMaxCluster = mMaxSector >> 2;
            }

            mCurrentSector = mCard.ReadFATBlock(mSector);
            InitializeComponent();
            UpdateMessage();
            vScrollBar1.Minimum = 0;
            vScrollBar1.Maximum = mMaxSector;
            vScrollBar1.SmallChange = 1;
            vScrollBar1.LargeChange = 10;

            mFont = new Font("Arial Font", 8.0f, FontStyle.Regular);
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            mSector = vScrollBar1.Value;
            mCurrentSector = mCard.ReadFATBlock(mSector);
            UpdateMessage();
            panel1.Invalidate();
        }

        void PaintFat(Graphics _g)
        {
            if (mCard.FatType == eFATType.FAT16)
            {
                SizeF s = _g.MeasureString("FFFF", mFont);
                int ww = (int)s.Width + 8;
                int hh = (int)s.Height + 4;
                int index = 0;
                for (int y = 0; y < 16; y++)
                {
                    int yp = y * hh;
                    for (int x = 0; x < 16; x++)
                    {
                        int xp = x * ww;
                        _g.DrawRectangle(Pens.Red, xp, yp, ww, hh);
                        int cluster = mCurrentSector[index]+(mCurrentSector[index+1]<<8);
                        index += 2;
                        string h = cluster.ToString("X4");
                        _g.DrawString(h, mFont, Brushes.Black, xp + 4, yp+ 4);
                    }
                }
            } else
            {

            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //g.FillRectangle(Brushes.Red, new Rectangle(0, 0, 256, 256));

            //g.DrawString("FFFF", mFont, Brushes.Black, 10, 10);
            PaintFat(g);
        }
    }
}
