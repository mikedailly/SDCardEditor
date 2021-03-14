namespace SDCardEditor
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SDCardPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.OpenSDCard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SDCardPath
            // 
            this.SDCardPath.Location = new System.Drawing.Point(97, 12);
            this.SDCardPath.Name = "SDCardPath";
            this.SDCardPath.Size = new System.Drawing.Size(539, 20);
            this.SDCardPath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "SD Card Image";
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(12, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(705, 114);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Disk Info";
            // 
            // listView1
            // 
            this.listView1.AllowColumnReorder = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(15, 159);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(702, 403);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // OpenSDCard
            // 
            this.OpenSDCard.Location = new System.Drawing.Point(642, 10);
            this.OpenSDCard.Name = "OpenSDCard";
            this.OpenSDCard.Size = new System.Drawing.Size(75, 23);
            this.OpenSDCard.TabIndex = 4;
            this.OpenSDCard.Text = "Open";
            this.OpenSDCard.UseVisualStyleBackColor = true;
            this.OpenSDCard.Click += new System.EventHandler(this.OpenSDCard_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 603);
            this.Controls.Add(this.OpenSDCard);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SDCardPath);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button OpenSDCard;
        public System.Windows.Forms.TextBox SDCardPath;
    }
}

