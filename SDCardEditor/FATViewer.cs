﻿using SDCardAccess;
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
        UInt32 ClusterSector = 0xffffffff;

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
            int SectorOffset = (int)mCurrentSector.Number - mCard.BootSector.FATSector;
            if (mCard.FatType == eFATType.FAT16)
            {
                UInt32 ClusterNum = (UInt32) SectorOffset * 256;
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
                        Brush ink = Brushes.Black;
                        if (ClusterSector == ClusterNum){
                            _g.FillRectangle(Brushes.Blue, xp, yp, ww, hh);
                            ink = Brushes.White;
                        }else{
                            _g.DrawRectangle(Pens.Red, xp, yp, ww, hh);
                        }
                        int cluster = mCurrentSector[index] + (mCurrentSector[index + 1] << 8);
                        index += 2;
                        string h = cluster.ToString("X4");
                        _g.DrawString(h, mFont, ink, xp + 4, yp + 4);
                        ClusterNum++;
                    }
                }
            }
            else if (mCard.FatType == eFATType.FAT32)
            {
                UInt32 ClusterNum = (UInt32)SectorOffset * 128;
                SizeF s = _g.MeasureString("FFFFFFFF", mFont);
                int ww = (int)s.Width + 8;
                int hh = (int)s.Height + 4;
                int index = 0;
                for (int y = 0; y < 16; y++)
                {
                    int yp = y * hh;
                    for (int x = 0; x < 8; x++)
                    {
                        int xp = x * ww;
                        Brush ink = Brushes.Black;
                        if (ClusterSector == ClusterNum){
                            _g.FillRectangle(Brushes.Blue, xp, yp, ww, hh);
                            ink = Brushes.White;
                        }else{
                            _g.DrawRectangle(Pens.Red, xp, yp, ww, hh);
                        }
                        UInt32 cluster = (UInt32)mCurrentSector[index] + ((UInt32)mCurrentSector[index + 1] << 8) + ((UInt32)mCurrentSector[index + 2] << 16) + ((UInt32)mCurrentSector[index + 3] << 24);
                        index += 4;
                        string h = cluster.ToString("X8");
                        _g.DrawString(h, mFont, ink, xp + 4, yp + 4);
                        ClusterNum++;
                    }
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            PaintFat(g);
        }

        // *********************************************************************************************************
        /// <summary>
        ///     Goto a cluster....
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // *********************************************************************************************************
        private void gotoClusterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NumberDialog d = new NumberDialog();
            d.ShowDialog();

            int r = d.Result;
            ClusterSector = (uint) r;

            int scale = 2;
            if (mCard.FatType == eFATType.FAT32) scale = 4;
            int clusters_per_sector = 512/ scale;
            int sect = r / clusters_per_sector;
            mSector = sect;
            mCurrentSector = mCard.ReadFATBlock(mSector);
            vScrollBar1.Value = mSector;

            UpdateMessage();
            vScrollBar1.Invalidate();
            panel1.Invalidate();
        }
    }
}
