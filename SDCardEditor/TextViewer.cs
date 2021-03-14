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
    }
}
