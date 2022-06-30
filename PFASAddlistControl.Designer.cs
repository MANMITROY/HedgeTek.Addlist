namespace HedgeTek.Addlist
{
    partial class PFASAddListControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PFASAddListControl));
            this.lblCaption0 = new System.Windows.Forms.Label();
            this.lblCaption1 = new System.Windows.Forms.Label();
            this.lstSource = new System.Windows.Forms.ListBox();
            this.lstDestination = new System.Windows.Forms.ListBox();
            this.cmdMovefields0 = new System.Windows.Forms.Button();
            this.cmdMovefields1 = new System.Windows.Forms.Button();
            this.cmdMovefields2 = new System.Windows.Forms.Button();
            this.cmdMovefields3 = new System.Windows.Forms.Button();
            this.cmdUpDown1 = new System.Windows.Forms.Button();
            this.cmdUpDown0 = new System.Windows.Forms.Button();
            this.imglst = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // lblCaption0
            // 
            this.lblCaption0.AutoSize = true;
            this.lblCaption0.Location = new System.Drawing.Point(16, 8);
            this.lblCaption0.Name = "lblCaption0";
            this.lblCaption0.Size = new System.Drawing.Size(50, 13);
            this.lblCaption0.TabIndex = 8;
            this.lblCaption0.Text = "Available";
            // 
            // lblCaption1
            // 
            this.lblCaption1.AutoSize = true;
            this.lblCaption1.Location = new System.Drawing.Point(168, 8);
            this.lblCaption1.Name = "lblCaption1";
            this.lblCaption1.Size = new System.Drawing.Size(49, 13);
            this.lblCaption1.TabIndex = 9;
            this.lblCaption1.Text = "Selected";
            // 
            // lstSource
            // 
            this.lstSource.AllowDrop = true;
            this.lstSource.FormattingEnabled = true;
            this.lstSource.Location = new System.Drawing.Point(8, 32);
            this.lstSource.Name = "lstSource";
            this.lstSource.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstSource.Size = new System.Drawing.Size(113, 134);
            this.lstSource.TabIndex = 5;
            // 
            // lstDestination
            // 
            this.lstDestination.AllowDrop = true;
            this.lstDestination.FormattingEnabled = true;
            this.lstDestination.Location = new System.Drawing.Point(168, 32);
            this.lstDestination.Name = "lstDestination";
            this.lstDestination.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstDestination.Size = new System.Drawing.Size(113, 134);
            this.lstDestination.TabIndex = 4;
            this.lstDestination.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.lstDestination_KeyPress);
            // 
            // cmdMovefields0
            // 
            this.cmdMovefields0.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.cmdMovefields0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdMovefields0.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdMovefields0.Location = new System.Drawing.Point(128, 42);
            this.cmdMovefields0.Margin = new System.Windows.Forms.Padding(0);
            this.cmdMovefields0.Name = "cmdMovefields0";
            this.cmdMovefields0.Size = new System.Drawing.Size(33, 23);
            this.cmdMovefields0.TabIndex = 3;
            this.cmdMovefields0.Text = ">";
            this.cmdMovefields0.UseVisualStyleBackColor = true;
            // 
            // cmdMovefields1
            // 
            this.cmdMovefields1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdMovefields1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdMovefields1.Location = new System.Drawing.Point(128, 72);
            this.cmdMovefields1.Margin = new System.Windows.Forms.Padding(0);
            this.cmdMovefields1.Name = "cmdMovefields1";
            this.cmdMovefields1.Size = new System.Drawing.Size(33, 23);
            this.cmdMovefields1.TabIndex = 2;
            this.cmdMovefields1.Text = ">>";
            this.cmdMovefields1.UseVisualStyleBackColor = true;
            // 
            // cmdMovefields2
            // 
            this.cmdMovefields2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdMovefields2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdMovefields2.Location = new System.Drawing.Point(128, 104);
            this.cmdMovefields2.Margin = new System.Windows.Forms.Padding(0);
            this.cmdMovefields2.Name = "cmdMovefields2";
            this.cmdMovefields2.Size = new System.Drawing.Size(33, 23);
            this.cmdMovefields2.TabIndex = 1;
            this.cmdMovefields2.Text = "<";
            this.cmdMovefields2.UseVisualStyleBackColor = true;
            // 
            // cmdMovefields3
            // 
            this.cmdMovefields3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdMovefields3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdMovefields3.Location = new System.Drawing.Point(128, 136);
            this.cmdMovefields3.Name = "cmdMovefields3";
            this.cmdMovefields3.Size = new System.Drawing.Size(33, 23);
            this.cmdMovefields3.TabIndex = 0;
            this.cmdMovefields3.Text = "<<";
            this.cmdMovefields3.UseVisualStyleBackColor = true;
            // 
            // cmdUpDown1
            // 
            this.cmdUpDown1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdUpDown1.Image = global::HedgeTek.Addlist.Properties.Resources.ARW06DN;
            this.cmdUpDown1.Location = new System.Drawing.Point(288, 104);
            this.cmdUpDown1.Name = "cmdUpDown1";
            this.cmdUpDown1.Size = new System.Drawing.Size(25, 41);
            this.cmdUpDown1.TabIndex = 7;
            this.cmdUpDown1.UseVisualStyleBackColor = true;
            // 
            // cmdUpDown0
            // 
            this.cmdUpDown0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdUpDown0.Image = global::HedgeTek.Addlist.Properties.Resources.ARW06UP;
            this.cmdUpDown0.Location = new System.Drawing.Point(288, 56);
            this.cmdUpDown0.Name = "cmdUpDown0";
            this.cmdUpDown0.Size = new System.Drawing.Size(25, 41);
            this.cmdUpDown0.TabIndex = 6;
            this.cmdUpDown0.UseVisualStyleBackColor = true;
            // 
            // imglst
            // 
            this.imglst.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglst.ImageStream")));
            this.imglst.TransparentColor = System.Drawing.Color.Transparent;
            this.imglst.Images.SetKeyName(0, "DRAG1aPG.ICO");
            this.imglst.Images.SetKeyName(1, "DRAG1PG.ICO");
            this.imglst.Images.SetKeyName(2, "DRAG2aPG.ICO");
            this.imglst.Images.SetKeyName(3, "DRAG2PG.ICO");
            // 
            // PFASAddListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdUpDown1);
            this.Controls.Add(this.cmdUpDown0);
            this.Controls.Add(this.cmdMovefields3);
            this.Controls.Add(this.cmdMovefields2);
            this.Controls.Add(this.cmdMovefields1);
            this.Controls.Add(this.cmdMovefields0);
            this.Controls.Add(this.lstDestination);
            this.Controls.Add(this.lstSource);
            this.Controls.Add(this.lblCaption1);
            this.Controls.Add(this.lblCaption0);
            this.Name = "PFASAddListControl";
            this.Size = new System.Drawing.Size(317, 177);
            this.Load += new System.EventHandler(this.PFASAddListControl_Load);
            this.Resize += new System.EventHandler(this.PFASAddListControl_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCaption0;
        private System.Windows.Forms.Label lblCaption1;
        private System.Windows.Forms.ListBox lstSource;
        private System.Windows.Forms.ListBox lstDestination;
        private System.Windows.Forms.Button cmdMovefields0;
        private System.Windows.Forms.Button cmdMovefields1;
        private System.Windows.Forms.Button cmdMovefields2;
        private System.Windows.Forms.Button cmdMovefields3;
        private System.Windows.Forms.Button cmdUpDown0;
        private System.Windows.Forms.Button cmdUpDown1;
        private System.Windows.Forms.ImageList imglst;
    }
}
