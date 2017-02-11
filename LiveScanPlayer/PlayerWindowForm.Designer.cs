namespace LiveScanPlayer
{
    partial class PlayerWindowForm
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
            this.btSelect = new System.Windows.Forms.Button();
            this.btStart = new System.Windows.Forms.Button();
            this.btShow = new System.Windows.Forms.Button();
            this.updateWorker = new System.ComponentModel.BackgroundWorker();
            this.OpenGLWorker = new System.ComponentModel.BackgroundWorker();
            this.btRewind = new System.Windows.Forms.Button();
            this.btRemove = new System.Windows.Forms.Button();
            this.lFrameFilesListView = new System.Windows.Forms.ListView();
            this.chSaveFrames = new System.Windows.Forms.CheckBox();
            this.btnSelectPly = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btSelect
            // 
            this.btSelect.Location = new System.Drawing.Point(12, 41);
            this.btSelect.Name = "btSelect";
            this.btSelect.Size = new System.Drawing.Size(89, 23);
            this.btSelect.TabIndex = 0;
            this.btSelect.Text = "Select bin files";
            this.btSelect.UseVisualStyleBackColor = true;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(12, 12);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(89, 23);
            this.btStart.TabIndex = 1;
            this.btStart.Text = "Start player";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // btShow
            // 
            this.btShow.Location = new System.Drawing.Point(12, 157);
            this.btShow.Name = "btShow";
            this.btShow.Size = new System.Drawing.Size(89, 23);
            this.btShow.TabIndex = 2;
            this.btShow.Text = "Show live";
            this.btShow.UseVisualStyleBackColor = true;
            this.btShow.Click += new System.EventHandler(this.btShow_Click);
            // 
            // updateWorker
            // 
            this.updateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.updateWorker_DoWork);
            // 
            // OpenGLWorker
            // 
            this.OpenGLWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OpenGLWorker_DoWork);
            // 
            // btRewind
            // 
            this.btRewind.Location = new System.Drawing.Point(12, 128);
            this.btRewind.Name = "btRewind";
            this.btRewind.Size = new System.Drawing.Size(89, 23);
            this.btRewind.TabIndex = 5;
            this.btRewind.Text = "Rewind";
            this.btRewind.UseVisualStyleBackColor = true;
            this.btRewind.Click += new System.EventHandler(this.btRewind_Click);
            // 
            // btRemove
            // 
            this.btRemove.Location = new System.Drawing.Point(12, 99);
            this.btRemove.Name = "btRemove";
            this.btRemove.Size = new System.Drawing.Size(89, 23);
            this.btRemove.TabIndex = 6;
            this.btRemove.Text = "Remove file";
            this.btRemove.UseVisualStyleBackColor = true;
            this.btRemove.Click += new System.EventHandler(this.btRemove_Click);
            // 
            // lFrameFilesListView
            // 
            this.lFrameFilesListView.LabelEdit = true;
            this.lFrameFilesListView.Location = new System.Drawing.Point(107, 12);
            this.lFrameFilesListView.MultiSelect = false;
            this.lFrameFilesListView.Name = "lFrameFilesListView";
            this.lFrameFilesListView.Size = new System.Drawing.Size(408, 139);
            this.lFrameFilesListView.TabIndex = 7;
            this.lFrameFilesListView.UseCompatibleStateImageBehavior = false;
            this.lFrameFilesListView.View = System.Windows.Forms.View.Details;
            this.lFrameFilesListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lFrameFilesListView_AfterLabelEdit);
            this.lFrameFilesListView.DoubleClick += new System.EventHandler(this.lFrameFilesListView_DoubleClick);
            // 
            // chSaveFrames
            // 
            this.chSaveFrames.AutoSize = true;
            this.chSaveFrames.Location = new System.Drawing.Point(432, 157);
            this.chSaveFrames.Name = "chSaveFrames";
            this.chSaveFrames.Size = new System.Drawing.Size(83, 17);
            this.chSaveFrames.TabIndex = 8;
            this.chSaveFrames.Text = "save frames";
            this.chSaveFrames.UseVisualStyleBackColor = true;
            // 
            // btnSelectPly
            // 
            this.btnSelectPly.Location = new System.Drawing.Point(12, 70);
            this.btnSelectPly.Name = "btnSelectPly";
            this.btnSelectPly.Size = new System.Drawing.Size(89, 23);
            this.btnSelectPly.TabIndex = 9;
            this.btnSelectPly.Text = "Select ply files";
            this.btnSelectPly.UseVisualStyleBackColor = true;
            this.btnSelectPly.Click += new System.EventHandler(this.btnSelectPly_Click);
            // 
            // PlayerWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 190);
            this.Controls.Add(this.btnSelectPly);
            this.Controls.Add(this.chSaveFrames);
            this.Controls.Add(this.lFrameFilesListView);
            this.Controls.Add(this.btRemove);
            this.Controls.Add(this.btRewind);
            this.Controls.Add(this.btShow);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.btSelect);
            this.Name = "PlayerWindowForm";
            this.Text = "LiveScanPlayer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlayerWindowForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btSelect;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.Button btShow;
        private System.ComponentModel.BackgroundWorker updateWorker;
        private System.ComponentModel.BackgroundWorker OpenGLWorker;
        private System.Windows.Forms.Button btRewind;
        private System.Windows.Forms.Button btRemove;
        private System.Windows.Forms.ListView lFrameFilesListView;
        private System.Windows.Forms.CheckBox chSaveFrames;
        private System.Windows.Forms.Button btnSelectPly;
    }
}

