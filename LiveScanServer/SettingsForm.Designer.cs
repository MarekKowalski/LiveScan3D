namespace KinectServer
{
    partial class SettingsForm
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
            this.lbMerge = new System.Windows.Forms.Label();
            this.chMerge = new System.Windows.Forms.CheckBox();
            this.lbICPIters = new System.Windows.Forms.Label();
            this.txtICPIters = new System.Windows.Forms.TextBox();
            this.grClient = new System.Windows.Forms.GroupBox();
            this.grBody = new System.Windows.Forms.GroupBox();
            this.chSkeletons = new System.Windows.Forms.CheckBox();
            this.chBodyData = new System.Windows.Forms.CheckBox();
            this.grMarkers = new System.Windows.Forms.GroupBox();
            this.lbX2 = new System.Windows.Forms.Label();
            this.btRemove = new System.Windows.Forms.Button();
            this.txtOrientationZ = new System.Windows.Forms.TextBox();
            this.txtId = new System.Windows.Forms.TextBox();
            this.lbY2 = new System.Windows.Forms.Label();
            this.txtOrientationY = new System.Windows.Forms.TextBox();
            this.lbId = new System.Windows.Forms.Label();
            this.lbZ2 = new System.Windows.Forms.Label();
            this.txtOrientationX = new System.Windows.Forms.TextBox();
            this.btAdd = new System.Windows.Forms.Button();
            this.lbTranslation = new System.Windows.Forms.Label();
            this.lbOrientation = new System.Windows.Forms.Label();
            this.txtTranslationZ = new System.Windows.Forms.TextBox();
            this.txtTranslationX = new System.Windows.Forms.TextBox();
            this.lisMarkers = new System.Windows.Forms.ListBox();
            this.txtTranslationY = new System.Windows.Forms.TextBox();
            this.grBounding = new System.Windows.Forms.GroupBox();
            this.lbMin = new System.Windows.Forms.Label();
            this.txtMaxZ = new System.Windows.Forms.TextBox();
            this.txtMaxY = new System.Windows.Forms.TextBox();
            this.txtMinX = new System.Windows.Forms.TextBox();
            this.txtMaxX = new System.Windows.Forms.TextBox();
            this.txtMinY = new System.Windows.Forms.TextBox();
            this.lbMax = new System.Windows.Forms.Label();
            this.txtMinZ = new System.Windows.Forms.TextBox();
            this.lbZ = new System.Windows.Forms.Label();
            this.lbY = new System.Windows.Forms.Label();
            this.lbX = new System.Windows.Forms.Label();
            this.grFiltering = new System.Windows.Forms.GroupBox();
            this.txtFilterNeighbors = new System.Windows.Forms.TextBox();
            this.chFilter = new System.Windows.Forms.CheckBox();
            this.lbFilterNeighbors = new System.Windows.Forms.Label();
            this.lbFilterDistance = new System.Windows.Forms.Label();
            this.txtFilterDistance = new System.Windows.Forms.TextBox();
            this.grServer = new System.Windows.Forms.GroupBox();
            this.rBinaryPly = new System.Windows.Forms.RadioButton();
            this.lbFormat = new System.Windows.Forms.Label();
            this.rAsciiPly = new System.Windows.Forms.RadioButton();
            this.txtRefinIters = new System.Windows.Forms.TextBox();
            this.lbOuterIters = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbCompressionLevel = new System.Windows.Forms.ComboBox();
            this.grClient.SuspendLayout();
            this.grBody.SuspendLayout();
            this.grMarkers.SuspendLayout();
            this.grBounding.SuspendLayout();
            this.grFiltering.SuspendLayout();
            this.grServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbMerge
            // 
            this.lbMerge.AutoSize = true;
            this.lbMerge.Location = new System.Drawing.Point(7, 69);
            this.lbMerge.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbMerge.Name = "lbMerge";
            this.lbMerge.Size = new System.Drawing.Size(99, 20);
            this.lbMerge.TabIndex = 22;
            this.lbMerge.Text = "Scan saving:";
            // 
            // chMerge
            // 
            this.chMerge.AutoSize = true;
            this.chMerge.Location = new System.Drawing.Point(156, 70);
            this.chMerge.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chMerge.Name = "chMerge";
            this.chMerge.Size = new System.Drawing.Size(126, 24);
            this.chMerge.TabIndex = 23;
            this.chMerge.Text = "merge scans";
            this.chMerge.UseVisualStyleBackColor = true;
            this.chMerge.CheckedChanged += new System.EventHandler(this.chMerge_CheckedChanged);
            // 
            // lbICPIters
            // 
            this.lbICPIters.AutoSize = true;
            this.lbICPIters.Location = new System.Drawing.Point(7, 34);
            this.lbICPIters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbICPIters.Name = "lbICPIters";
            this.lbICPIters.Size = new System.Drawing.Size(128, 20);
            this.lbICPIters.TabIndex = 24;
            this.lbICPIters.Text = "Num of ICP iters:";
            // 
            // txtICPIters
            // 
            this.txtICPIters.Location = new System.Drawing.Point(156, 31);
            this.txtICPIters.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtICPIters.Name = "txtICPIters";
            this.txtICPIters.Size = new System.Drawing.Size(55, 26);
            this.txtICPIters.TabIndex = 25;
            this.txtICPIters.TextChanged += new System.EventHandler(this.txtICPIters_TextChanged);
            // 
            // grClient
            // 
            this.grClient.Controls.Add(this.grBody);
            this.grClient.Controls.Add(this.grMarkers);
            this.grClient.Controls.Add(this.grBounding);
            this.grClient.Controls.Add(this.grFiltering);
            this.grClient.Location = new System.Drawing.Point(18, 18);
            this.grClient.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grClient.Name = "grClient";
            this.grClient.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grClient.Size = new System.Drawing.Size(948, 447);
            this.grClient.TabIndex = 43;
            this.grClient.TabStop = false;
            this.grClient.Text = "KinectClient settings";
            // 
            // grBody
            // 
            this.grBody.Controls.Add(this.chSkeletons);
            this.grBody.Controls.Add(this.chBodyData);
            this.grBody.Location = new System.Drawing.Point(14, 325);
            this.grBody.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grBody.Name = "grBody";
            this.grBody.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grBody.Size = new System.Drawing.Size(374, 101);
            this.grBody.TabIndex = 47;
            this.grBody.TabStop = false;
            this.grBody.Text = "Body data";
            // 
            // chSkeletons
            // 
            this.chSkeletons.AutoSize = true;
            this.chSkeletons.Location = new System.Drawing.Point(16, 63);
            this.chSkeletons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chSkeletons.Name = "chSkeletons";
            this.chSkeletons.Size = new System.Drawing.Size(220, 24);
            this.chSkeletons.TabIndex = 21;
            this.chSkeletons.Text = "show skeletons in live view";
            this.chSkeletons.UseVisualStyleBackColor = true;
            this.chSkeletons.CheckedChanged += new System.EventHandler(this.chSkeletons_CheckedChanged);
            // 
            // chBodyData
            // 
            this.chBodyData.AutoSize = true;
            this.chBodyData.Location = new System.Drawing.Point(16, 29);
            this.chBodyData.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chBodyData.Name = "chBodyData";
            this.chBodyData.Size = new System.Drawing.Size(167, 24);
            this.chBodyData.TabIndex = 19;
            this.chBodyData.Text = "stream only bodies";
            this.chBodyData.UseVisualStyleBackColor = true;
            this.chBodyData.CheckedChanged += new System.EventHandler(this.chBodyData_CheckedChanged);
            // 
            // grMarkers
            // 
            this.grMarkers.Controls.Add(this.lbX2);
            this.grMarkers.Controls.Add(this.btRemove);
            this.grMarkers.Controls.Add(this.txtOrientationZ);
            this.grMarkers.Controls.Add(this.txtId);
            this.grMarkers.Controls.Add(this.lbY2);
            this.grMarkers.Controls.Add(this.txtOrientationY);
            this.grMarkers.Controls.Add(this.lbId);
            this.grMarkers.Controls.Add(this.lbZ2);
            this.grMarkers.Controls.Add(this.txtOrientationX);
            this.grMarkers.Controls.Add(this.btAdd);
            this.grMarkers.Controls.Add(this.lbTranslation);
            this.grMarkers.Controls.Add(this.lbOrientation);
            this.grMarkers.Controls.Add(this.txtTranslationZ);
            this.grMarkers.Controls.Add(this.txtTranslationX);
            this.grMarkers.Controls.Add(this.lisMarkers);
            this.grMarkers.Controls.Add(this.txtTranslationY);
            this.grMarkers.Location = new System.Drawing.Point(406, 29);
            this.grMarkers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grMarkers.Name = "grMarkers";
            this.grMarkers.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grMarkers.Size = new System.Drawing.Size(530, 285);
            this.grMarkers.TabIndex = 45;
            this.grMarkers.TabStop = false;
            this.grMarkers.Text = "Calibration markers";
            // 
            // lbX2
            // 
            this.lbX2.AutoSize = true;
            this.lbX2.Location = new System.Drawing.Point(345, 29);
            this.lbX2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbX2.Name = "lbX2";
            this.lbX2.Size = new System.Drawing.Size(20, 20);
            this.lbX2.TabIndex = 49;
            this.lbX2.Text = "X";
            // 
            // btRemove
            // 
            this.btRemove.Location = new System.Drawing.Point(350, 195);
            this.btRemove.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btRemove.Name = "btRemove";
            this.btRemove.Size = new System.Drawing.Size(138, 35);
            this.btRemove.TabIndex = 59;
            this.btRemove.Text = "Remove marker";
            this.btRemove.UseVisualStyleBackColor = true;
            this.btRemove.Click += new System.EventHandler(this.btRemove_Click);
            // 
            // txtOrientationZ
            // 
            this.txtOrientationZ.Location = new System.Drawing.Point(458, 58);
            this.txtOrientationZ.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtOrientationZ.Name = "txtOrientationZ";
            this.txtOrientationZ.Size = new System.Drawing.Size(55, 26);
            this.txtOrientationZ.TabIndex = 48;
            this.txtOrientationZ.TextChanged += new System.EventHandler(this.txtOrientationZ_TextChanged);
            // 
            // txtId
            // 
            this.txtId.Location = new System.Drawing.Point(326, 143);
            this.txtId.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtId.Name = "txtId";
            this.txtId.Size = new System.Drawing.Size(55, 26);
            this.txtId.TabIndex = 58;
            this.txtId.TextChanged += new System.EventHandler(this.txtId_TextChanged);
            // 
            // lbY2
            // 
            this.lbY2.AutoSize = true;
            this.lbY2.Location = new System.Drawing.Point(411, 29);
            this.lbY2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbY2.Name = "lbY2";
            this.lbY2.Size = new System.Drawing.Size(20, 20);
            this.lbY2.TabIndex = 50;
            this.lbY2.Text = "Y";
            // 
            // txtOrientationY
            // 
            this.txtOrientationY.Location = new System.Drawing.Point(392, 58);
            this.txtOrientationY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtOrientationY.Name = "txtOrientationY";
            this.txtOrientationY.Size = new System.Drawing.Size(55, 26);
            this.txtOrientationY.TabIndex = 47;
            this.txtOrientationY.TextChanged += new System.EventHandler(this.txtOrientationY_TextChanged);
            // 
            // lbId
            // 
            this.lbId.AutoSize = true;
            this.lbId.Location = new System.Drawing.Point(198, 148);
            this.lbId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbId.Name = "lbId";
            this.lbId.Size = new System.Drawing.Size(78, 20);
            this.lbId.TabIndex = 57;
            this.lbId.Text = "Marker id:";
            // 
            // lbZ2
            // 
            this.lbZ2.AutoSize = true;
            this.lbZ2.Location = new System.Drawing.Point(476, 29);
            this.lbZ2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbZ2.Name = "lbZ2";
            this.lbZ2.Size = new System.Drawing.Size(19, 20);
            this.lbZ2.TabIndex = 51;
            this.lbZ2.Text = "Z";
            // 
            // txtOrientationX
            // 
            this.txtOrientationX.Location = new System.Drawing.Point(326, 58);
            this.txtOrientationX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtOrientationX.Name = "txtOrientationX";
            this.txtOrientationX.Size = new System.Drawing.Size(55, 26);
            this.txtOrientationX.TabIndex = 46;
            this.txtOrientationX.TextChanged += new System.EventHandler(this.txtOrientationX_TextChanged);
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(202, 195);
            this.btAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(138, 35);
            this.btAdd.TabIndex = 56;
            this.btAdd.Text = "Add marker";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // lbTranslation
            // 
            this.lbTranslation.AutoSize = true;
            this.lbTranslation.Location = new System.Drawing.Point(198, 105);
            this.lbTranslation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbTranslation.Name = "lbTranslation";
            this.lbTranslation.Size = new System.Drawing.Size(91, 20);
            this.lbTranslation.TabIndex = 52;
            this.lbTranslation.Text = "Translation:";
            // 
            // lbOrientation
            // 
            this.lbOrientation.AutoSize = true;
            this.lbOrientation.Location = new System.Drawing.Point(198, 63);
            this.lbOrientation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbOrientation.Name = "lbOrientation";
            this.lbOrientation.Size = new System.Drawing.Size(91, 20);
            this.lbOrientation.TabIndex = 45;
            this.lbOrientation.Text = "Orientation:";
            // 
            // txtTranslationZ
            // 
            this.txtTranslationZ.Location = new System.Drawing.Point(458, 100);
            this.txtTranslationZ.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTranslationZ.Name = "txtTranslationZ";
            this.txtTranslationZ.Size = new System.Drawing.Size(55, 26);
            this.txtTranslationZ.TabIndex = 55;
            this.txtTranslationZ.TextChanged += new System.EventHandler(this.txtTranslationZ_TextChanged);
            // 
            // txtTranslationX
            // 
            this.txtTranslationX.Location = new System.Drawing.Point(326, 100);
            this.txtTranslationX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTranslationX.Name = "txtTranslationX";
            this.txtTranslationX.Size = new System.Drawing.Size(55, 26);
            this.txtTranslationX.TabIndex = 53;
            this.txtTranslationX.TextChanged += new System.EventHandler(this.txtTranslationX_TextChanged);
            // 
            // lisMarkers
            // 
            this.lisMarkers.FormattingEnabled = true;
            this.lisMarkers.ItemHeight = 20;
            this.lisMarkers.Location = new System.Drawing.Point(9, 63);
            this.lisMarkers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lisMarkers.Name = "lisMarkers";
            this.lisMarkers.Size = new System.Drawing.Size(178, 164);
            this.lisMarkers.TabIndex = 43;
            this.lisMarkers.SelectedIndexChanged += new System.EventHandler(this.lisMarkers_SelectedIndexChanged);
            // 
            // txtTranslationY
            // 
            this.txtTranslationY.Location = new System.Drawing.Point(392, 100);
            this.txtTranslationY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTranslationY.Name = "txtTranslationY";
            this.txtTranslationY.Size = new System.Drawing.Size(55, 26);
            this.txtTranslationY.TabIndex = 54;
            this.txtTranslationY.TextChanged += new System.EventHandler(this.txtTranslationY_TextChanged);
            // 
            // grBounding
            // 
            this.grBounding.Controls.Add(this.lbMin);
            this.grBounding.Controls.Add(this.txtMaxZ);
            this.grBounding.Controls.Add(this.txtMaxY);
            this.grBounding.Controls.Add(this.txtMinX);
            this.grBounding.Controls.Add(this.txtMaxX);
            this.grBounding.Controls.Add(this.txtMinY);
            this.grBounding.Controls.Add(this.lbMax);
            this.grBounding.Controls.Add(this.txtMinZ);
            this.grBounding.Controls.Add(this.lbZ);
            this.grBounding.Controls.Add(this.lbY);
            this.grBounding.Controls.Add(this.lbX);
            this.grBounding.Location = new System.Drawing.Point(14, 29);
            this.grBounding.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grBounding.Name = "grBounding";
            this.grBounding.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grBounding.Size = new System.Drawing.Size(374, 140);
            this.grBounding.TabIndex = 46;
            this.grBounding.TabStop = false;
            this.grBounding.Text = "Bounding box";
            // 
            // lbMin
            // 
            this.lbMin.AutoSize = true;
            this.lbMin.Location = new System.Drawing.Point(12, 58);
            this.lbMin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbMin.Name = "lbMin";
            this.lbMin.Size = new System.Drawing.Size(95, 20);
            this.lbMin.TabIndex = 13;
            this.lbMin.Text = "Min bounds:";
            // 
            // txtMaxZ
            // 
            this.txtMaxZ.Location = new System.Drawing.Point(272, 95);
            this.txtMaxZ.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxZ.Name = "txtMaxZ";
            this.txtMaxZ.Size = new System.Drawing.Size(55, 26);
            this.txtMaxZ.TabIndex = 23;
            this.txtMaxZ.TextChanged += new System.EventHandler(this.txtMaxZ_TextChanged);
            // 
            // txtMaxY
            // 
            this.txtMaxY.Location = new System.Drawing.Point(206, 95);
            this.txtMaxY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxY.Name = "txtMaxY";
            this.txtMaxY.Size = new System.Drawing.Size(55, 26);
            this.txtMaxY.TabIndex = 22;
            this.txtMaxY.TextChanged += new System.EventHandler(this.txtMaxY_TextChanged);
            // 
            // txtMinX
            // 
            this.txtMinX.Location = new System.Drawing.Point(140, 54);
            this.txtMinX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMinX.Name = "txtMinX";
            this.txtMinX.Size = new System.Drawing.Size(55, 26);
            this.txtMinX.TabIndex = 14;
            this.txtMinX.TextChanged += new System.EventHandler(this.txtMinX_TextChanged);
            // 
            // txtMaxX
            // 
            this.txtMaxX.Location = new System.Drawing.Point(140, 95);
            this.txtMaxX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMaxX.Name = "txtMaxX";
            this.txtMaxX.Size = new System.Drawing.Size(55, 26);
            this.txtMaxX.TabIndex = 21;
            this.txtMaxX.TextChanged += new System.EventHandler(this.txtMaxX_TextChanged);
            // 
            // txtMinY
            // 
            this.txtMinY.Location = new System.Drawing.Point(206, 54);
            this.txtMinY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMinY.Name = "txtMinY";
            this.txtMinY.Size = new System.Drawing.Size(55, 26);
            this.txtMinY.TabIndex = 15;
            this.txtMinY.TextChanged += new System.EventHandler(this.txtMinY_TextChanged);
            // 
            // lbMax
            // 
            this.lbMax.AutoSize = true;
            this.lbMax.Location = new System.Drawing.Point(12, 100);
            this.lbMax.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbMax.Name = "lbMax";
            this.lbMax.Size = new System.Drawing.Size(99, 20);
            this.lbMax.TabIndex = 20;
            this.lbMax.Text = "Max bounds:";
            // 
            // txtMinZ
            // 
            this.txtMinZ.Location = new System.Drawing.Point(272, 54);
            this.txtMinZ.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMinZ.Name = "txtMinZ";
            this.txtMinZ.Size = new System.Drawing.Size(55, 26);
            this.txtMinZ.TabIndex = 16;
            this.txtMinZ.TextChanged += new System.EventHandler(this.txtMinZ_TextChanged);
            // 
            // lbZ
            // 
            this.lbZ.AutoSize = true;
            this.lbZ.Location = new System.Drawing.Point(290, 25);
            this.lbZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbZ.Name = "lbZ";
            this.lbZ.Size = new System.Drawing.Size(19, 20);
            this.lbZ.TabIndex = 19;
            this.lbZ.Text = "Z";
            // 
            // lbY
            // 
            this.lbY.AutoSize = true;
            this.lbY.Location = new System.Drawing.Point(225, 25);
            this.lbY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbY.Name = "lbY";
            this.lbY.Size = new System.Drawing.Size(20, 20);
            this.lbY.TabIndex = 18;
            this.lbY.Text = "Y";
            // 
            // lbX
            // 
            this.lbX.AutoSize = true;
            this.lbX.Location = new System.Drawing.Point(159, 25);
            this.lbX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbX.Name = "lbX";
            this.lbX.Size = new System.Drawing.Size(20, 20);
            this.lbX.TabIndex = 17;
            this.lbX.Text = "X";
            // 
            // grFiltering
            // 
            this.grFiltering.Controls.Add(this.txtFilterNeighbors);
            this.grFiltering.Controls.Add(this.chFilter);
            this.grFiltering.Controls.Add(this.lbFilterNeighbors);
            this.grFiltering.Controls.Add(this.lbFilterDistance);
            this.grFiltering.Controls.Add(this.txtFilterDistance);
            this.grFiltering.Location = new System.Drawing.Point(14, 178);
            this.grFiltering.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grFiltering.Name = "grFiltering";
            this.grFiltering.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grFiltering.Size = new System.Drawing.Size(374, 135);
            this.grFiltering.TabIndex = 45;
            this.grFiltering.TabStop = false;
            this.grFiltering.Text = "Filtering";
            // 
            // txtFilterNeighbors
            // 
            this.txtFilterNeighbors.Location = new System.Drawing.Point(140, 60);
            this.txtFilterNeighbors.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFilterNeighbors.Name = "txtFilterNeighbors";
            this.txtFilterNeighbors.Size = new System.Drawing.Size(55, 26);
            this.txtFilterNeighbors.TabIndex = 20;
            this.txtFilterNeighbors.TextChanged += new System.EventHandler(this.txtFilterNeighbors_TextChanged);
            // 
            // chFilter
            // 
            this.chFilter.AutoSize = true;
            this.chFilter.Location = new System.Drawing.Point(140, 25);
            this.chFilter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chFilter.Name = "chFilter";
            this.chFilter.Size = new System.Drawing.Size(147, 24);
            this.chFilter.TabIndex = 18;
            this.chFilter.Text = "filtering enabled";
            this.chFilter.UseVisualStyleBackColor = true;
            this.chFilter.CheckedChanged += new System.EventHandler(this.chFilter_CheckedChanged);
            // 
            // lbFilterNeighbors
            // 
            this.lbFilterNeighbors.AutoSize = true;
            this.lbFilterNeighbors.Location = new System.Drawing.Point(12, 65);
            this.lbFilterNeighbors.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbFilterNeighbors.Name = "lbFilterNeighbors";
            this.lbFilterNeighbors.Size = new System.Drawing.Size(98, 20);
            this.lbFilterNeighbors.TabIndex = 19;
            this.lbFilterNeighbors.Text = "N neighbors:";
            // 
            // lbFilterDistance
            // 
            this.lbFilterDistance.AutoSize = true;
            this.lbFilterDistance.Location = new System.Drawing.Point(12, 105);
            this.lbFilterDistance.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbFilterDistance.Name = "lbFilterDistance";
            this.lbFilterDistance.Size = new System.Drawing.Size(106, 20);
            this.lbFilterDistance.TabIndex = 21;
            this.lbFilterDistance.Text = "Max distance:";
            // 
            // txtFilterDistance
            // 
            this.txtFilterDistance.Location = new System.Drawing.Point(140, 100);
            this.txtFilterDistance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFilterDistance.Name = "txtFilterDistance";
            this.txtFilterDistance.Size = new System.Drawing.Size(55, 26);
            this.txtFilterDistance.TabIndex = 22;
            this.txtFilterDistance.TextChanged += new System.EventHandler(this.txtFilterDistance_TextChanged);
            // 
            // grServer
            // 
            this.grServer.Controls.Add(this.cbCompressionLevel);
            this.grServer.Controls.Add(this.label2);
            this.grServer.Controls.Add(this.rBinaryPly);
            this.grServer.Controls.Add(this.lbFormat);
            this.grServer.Controls.Add(this.rAsciiPly);
            this.grServer.Controls.Add(this.txtRefinIters);
            this.grServer.Controls.Add(this.lbOuterIters);
            this.grServer.Controls.Add(this.lbMerge);
            this.grServer.Controls.Add(this.txtICPIters);
            this.grServer.Controls.Add(this.chMerge);
            this.grServer.Controls.Add(this.lbICPIters);
            this.grServer.Location = new System.Drawing.Point(18, 478);
            this.grServer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grServer.Name = "grServer";
            this.grServer.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grServer.Size = new System.Drawing.Size(948, 175);
            this.grServer.TabIndex = 44;
            this.grServer.TabStop = false;
            this.grServer.Text = "KinectServer settings";
            // 
            // rBinaryPly
            // 
            this.rBinaryPly.AutoSize = true;
            this.rBinaryPly.Location = new System.Drawing.Point(277, 105);
            this.rBinaryPly.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rBinaryPly.Name = "rBinaryPly";
            this.rBinaryPly.Size = new System.Drawing.Size(110, 24);
            this.rBinaryPly.TabIndex = 30;
            this.rBinaryPly.TabStop = true;
            this.rBinaryPly.Text = "binary PLY";
            this.rBinaryPly.UseVisualStyleBackColor = true;
            this.rBinaryPly.CheckedChanged += new System.EventHandler(this.PlyFormat_CheckedChanged);
            // 
            // lbFormat
            // 
            this.lbFormat.AutoSize = true;
            this.lbFormat.Location = new System.Drawing.Point(7, 106);
            this.lbFormat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbFormat.Name = "lbFormat";
            this.lbFormat.Size = new System.Drawing.Size(88, 20);
            this.lbFormat.TabIndex = 29;
            this.lbFormat.Text = "File format:";
            // 
            // rAsciiPly
            // 
            this.rAsciiPly.AutoSize = true;
            this.rAsciiPly.Location = new System.Drawing.Point(156, 105);
            this.rAsciiPly.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rAsciiPly.Name = "rAsciiPly";
            this.rAsciiPly.Size = new System.Drawing.Size(111, 24);
            this.rAsciiPly.TabIndex = 28;
            this.rAsciiPly.TabStop = true;
            this.rAsciiPly.Text = "ASCII PLY";
            this.rAsciiPly.UseVisualStyleBackColor = true;
            this.rAsciiPly.CheckedChanged += new System.EventHandler(this.PlyFormat_CheckedChanged);
            // 
            // txtRefinIters
            // 
            this.txtRefinIters.Location = new System.Drawing.Point(408, 31);
            this.txtRefinIters.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtRefinIters.Name = "txtRefinIters";
            this.txtRefinIters.Size = new System.Drawing.Size(55, 26);
            this.txtRefinIters.TabIndex = 27;
            this.txtRefinIters.TextChanged += new System.EventHandler(this.txtRefinIters_TextChanged);
            // 
            // lbOuterIters
            // 
            this.lbOuterIters.AutoSize = true;
            this.lbOuterIters.Location = new System.Drawing.Point(222, 36);
            this.lbOuterIters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbOuterIters.Name = "lbOuterIters";
            this.lbOuterIters.Size = new System.Drawing.Size(178, 20);
            this.lbOuterIters.TabIndex = 26;
            this.lbOuterIters.Text = "Num of refinement iters:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 20);
            this.label2.TabIndex = 31;
            this.label2.Text = "Compression level:";
            // 
            // cbCompressionLevel
            // 
            this.cbCompressionLevel.FormattingEnabled = true;
            this.cbCompressionLevel.Items.AddRange(new object[] {
            "0 (no compression)",
            "1",
            "2 (recommended)",
            "3",
            "5",
            "7",
            "9",
            "11",
            "13",
            "15",
            "17",
            "19"});
            this.cbCompressionLevel.Location = new System.Drawing.Point(154, 136);
            this.cbCompressionLevel.Name = "cbCompressionLevel";
            this.cbCompressionLevel.Size = new System.Drawing.Size(169, 28);
            this.cbCompressionLevel.TabIndex = 32;
            this.cbCompressionLevel.SelectedIndexChanged += new System.EventHandler(this.cbCompressionLevel_SelectedIndexChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 663);
            this.Controls.Add(this.grServer);
            this.Controls.Add(this.grClient);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.grClient.ResumeLayout(false);
            this.grBody.ResumeLayout(false);
            this.grBody.PerformLayout();
            this.grMarkers.ResumeLayout(false);
            this.grMarkers.PerformLayout();
            this.grBounding.ResumeLayout(false);
            this.grBounding.PerformLayout();
            this.grFiltering.ResumeLayout(false);
            this.grFiltering.PerformLayout();
            this.grServer.ResumeLayout(false);
            this.grServer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbMerge;
        private System.Windows.Forms.CheckBox chMerge;
        private System.Windows.Forms.Label lbICPIters;
        private System.Windows.Forms.TextBox txtICPIters;
        private System.Windows.Forms.GroupBox grClient;
        private System.Windows.Forms.GroupBox grServer;
        private System.Windows.Forms.TextBox txtRefinIters;
        private System.Windows.Forms.Label lbOuterIters;
        private System.Windows.Forms.GroupBox grFiltering;
        private System.Windows.Forms.TextBox txtFilterNeighbors;
        private System.Windows.Forms.CheckBox chFilter;
        private System.Windows.Forms.Label lbFilterNeighbors;
        private System.Windows.Forms.Label lbFilterDistance;
        private System.Windows.Forms.TextBox txtFilterDistance;
        private System.Windows.Forms.GroupBox grMarkers;
        private System.Windows.Forms.Label lbX2;
        private System.Windows.Forms.Button btRemove;
        private System.Windows.Forms.TextBox txtOrientationZ;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Label lbY2;
        private System.Windows.Forms.TextBox txtOrientationY;
        private System.Windows.Forms.Label lbId;
        private System.Windows.Forms.Label lbZ2;
        private System.Windows.Forms.TextBox txtOrientationX;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Label lbTranslation;
        private System.Windows.Forms.Label lbOrientation;
        private System.Windows.Forms.TextBox txtTranslationZ;
        private System.Windows.Forms.TextBox txtTranslationX;
        private System.Windows.Forms.ListBox lisMarkers;
        private System.Windows.Forms.TextBox txtTranslationY;
        private System.Windows.Forms.GroupBox grBounding;
        private System.Windows.Forms.Label lbMin;
        private System.Windows.Forms.TextBox txtMaxZ;
        private System.Windows.Forms.TextBox txtMaxY;
        private System.Windows.Forms.TextBox txtMinX;
        private System.Windows.Forms.TextBox txtMaxX;
        private System.Windows.Forms.TextBox txtMinY;
        private System.Windows.Forms.Label lbMax;
        private System.Windows.Forms.TextBox txtMinZ;
        private System.Windows.Forms.Label lbZ;
        private System.Windows.Forms.Label lbY;
        private System.Windows.Forms.Label lbX;
        private System.Windows.Forms.GroupBox grBody;
        private System.Windows.Forms.CheckBox chBodyData;
        private System.Windows.Forms.RadioButton rBinaryPly;
        private System.Windows.Forms.Label lbFormat;
        private System.Windows.Forms.RadioButton rAsciiPly;
        private System.Windows.Forms.CheckBox chSkeletons;
        private System.Windows.Forms.ComboBox cbCompressionLevel;
        private System.Windows.Forms.Label label2;

    }
}