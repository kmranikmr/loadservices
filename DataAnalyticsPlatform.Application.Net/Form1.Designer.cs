namespace ApplicationNet
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pnlXmlView = new System.Windows.Forms.Panel();
            this.pnlSubSchema = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.treeViewModels = new System.Windows.Forms.TreeView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnNewModel = new System.Windows.Forms.Button();
            this.pnlMainSchema = new System.Windows.Forms.Panel();
            this.treeViewMainClass = new System.Windows.Forms.TreeView();
            this.lstMainObject = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtWriteConnectionString = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSourceFile = new System.Windows.Forms.TextBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblSourcePath = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnImportData = new System.Windows.Forms.Button();
            this.btnCreateReadCfg = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnUpdateModel = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.treeViewResult = new System.Windows.Forms.TreeView();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.pnlXmlView.SuspendLayout();
            this.pnlSubSchema.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnlMainSchema.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tabControl1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(906, 520);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 16);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(900, 501);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pnlXmlView);
            this.tabPage1.Controls.Add(this.pnlSubSchema);
            this.tabPage1.Controls.Add(this.pnlMainSchema);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(892, 475);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Read Configuration";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pnlXmlView
            // 
            this.pnlXmlView.Controls.Add(this.treeViewResult);
            this.pnlXmlView.Controls.Add(this.panel4);
            this.pnlXmlView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlXmlView.Location = new System.Drawing.Point(403, 3);
            this.pnlXmlView.Name = "pnlXmlView";
            this.pnlXmlView.Size = new System.Drawing.Size(486, 469);
            this.pnlXmlView.TabIndex = 5;
            // 
            // pnlSubSchema
            // 
            this.pnlSubSchema.Controls.Add(this.panel1);
            this.pnlSubSchema.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSubSchema.Location = new System.Drawing.Point(203, 3);
            this.pnlSubSchema.Name = "pnlSubSchema";
            this.pnlSubSchema.Size = new System.Drawing.Size(200, 469);
            this.pnlSubSchema.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.treeViewModels);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 469);
            this.panel1.TabIndex = 3;
            // 
            // treeViewModels
            // 
            this.treeViewModels.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewModels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewModels.Location = new System.Drawing.Point(0, 0);
            this.treeViewModels.Name = "treeViewModels";
            this.treeViewModels.Size = new System.Drawing.Size(200, 447);
            this.treeViewModels.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnNewModel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 447);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 22);
            this.panel2.TabIndex = 2;
            // 
            // btnNewModel
            // 
            this.btnNewModel.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnNewModel.Location = new System.Drawing.Point(0, 0);
            this.btnNewModel.Name = "btnNewModel";
            this.btnNewModel.Size = new System.Drawing.Size(109, 22);
            this.btnNewModel.TabIndex = 1;
            this.btnNewModel.Text = "New Model";
            this.btnNewModel.UseVisualStyleBackColor = true;
            this.btnNewModel.Click += new System.EventHandler(this.btnNewModel_Click);
            // 
            // pnlMainSchema
            // 
            this.pnlMainSchema.Controls.Add(this.treeViewMainClass);
            this.pnlMainSchema.Controls.Add(this.lstMainObject);
            this.pnlMainSchema.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlMainSchema.Location = new System.Drawing.Point(3, 3);
            this.pnlMainSchema.Name = "pnlMainSchema";
            this.pnlMainSchema.Size = new System.Drawing.Size(200, 469);
            this.pnlMainSchema.TabIndex = 3;
            // 
            // treeViewMainClass
            // 
            this.treeViewMainClass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewMainClass.Location = new System.Drawing.Point(0, 15);
            this.treeViewMainClass.Name = "treeViewMainClass";
            this.treeViewMainClass.Size = new System.Drawing.Size(200, 454);
            this.treeViewMainClass.TabIndex = 1;
            // 
            // lstMainObject
            // 
            this.lstMainObject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstMainObject.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstMainObject.FormattingEnabled = true;
            this.lstMainObject.Location = new System.Drawing.Point(0, 0);
            this.lstMainObject.Name = "lstMainObject";
            this.lstMainObject.Size = new System.Drawing.Size(200, 15);
            this.lstMainObject.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtWriteConnectionString);
            this.tabPage2.Controls.Add(this.comboBox1);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(892, 475);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Write Configuration";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtWriteConnectionString
            // 
            this.txtWriteConnectionString.Location = new System.Drawing.Point(22, 92);
            this.txtWriteConnectionString.Name = "txtWriteConnectionString";
            this.txtWriteConnectionString.Size = new System.Drawing.Size(711, 20);
            this.txtWriteConnectionString.TabIndex = 2;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Mongo",
            "Elastic",
            "PostgreSql"});
            this.comboBox1.Location = new System.Drawing.Point(22, 40);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(163, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "ConnectionString";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Database";
            // 
            // txtSourceFile
            // 
            this.txtSourceFile.Location = new System.Drawing.Point(13, 82);
            this.txtSourceFile.Name = "txtSourceFile";
            this.txtSourceFile.Size = new System.Drawing.Size(828, 20);
            this.txtSourceFile.TabIndex = 1;
            this.txtSourceFile.Text = "C:\\Users\\bibek\\Source\\Repos\\DataAnalyticsPlatform\\Data\\FL_insurance_sample.csv";
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(766, 108);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(75, 23);
            this.btnPreview.TabIndex = 2;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(847, 79);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(25, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblSourcePath
            // 
            this.lblSourcePath.AutoSize = true;
            this.lblSourcePath.Location = new System.Drawing.Point(13, 63);
            this.lblSourcePath.Name = "lblSourcePath";
            this.lblSourcePath.Size = new System.Drawing.Size(66, 13);
            this.lblSourcePath.TabIndex = 4;
            this.lblSourcePath.Text = "Source Path";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(906, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(13, 30);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(859, 20);
            this.txtUrl.TabIndex = 1;
            this.txtUrl.Text = "http://localhost:50926/api/Preview/1/generatemodel ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Url";
            // 
            // btnImportData
            // 
            this.btnImportData.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnImportData.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnImportData.Location = new System.Drawing.Point(827, 0);
            this.btnImportData.Name = "btnImportData";
            this.btnImportData.Size = new System.Drawing.Size(75, 26);
            this.btnImportData.TabIndex = 7;
            this.btnImportData.Text = "Import Data";
            this.btnImportData.UseVisualStyleBackColor = true;
            this.btnImportData.Click += new System.EventHandler(this.btnImportData_Click);
            // 
            // btnCreateReadCfg
            // 
            this.btnCreateReadCfg.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCreateReadCfg.Location = new System.Drawing.Point(736, 0);
            this.btnCreateReadCfg.Name = "btnCreateReadCfg";
            this.btnCreateReadCfg.Size = new System.Drawing.Size(91, 26);
            this.btnCreateReadCfg.TabIndex = 7;
            this.btnCreateReadCfg.Text = "GetReadConfig";
            this.btnCreateReadCfg.UseVisualStyleBackColor = true;
            this.btnCreateReadCfg.Click += new System.EventHandler(this.btnCreateReadCfg_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.txtSourceFile);
            this.splitContainer1.Panel1.Controls.Add(this.btnPreview);
            this.splitContainer1.Panel1.Controls.Add(this.txtUrl);
            this.splitContainer1.Panel1.Controls.Add(this.btnBrowse);
            this.splitContainer1.Panel1.Controls.Add(this.lblSourcePath);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(906, 668);
            this.splitContainer1.SplitterDistance = 144;
            this.splitContainer1.TabIndex = 8;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.btnCreateReadCfg);
            this.panel3.Controls.Add(this.btnImportData);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 692);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(906, 30);
            this.panel3.TabIndex = 9;
            // 
            // btnUpdateModel
            // 
            this.btnUpdateModel.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnUpdateModel.Location = new System.Drawing.Point(0, 0);
            this.btnUpdateModel.Name = "btnUpdateModel";
            this.btnUpdateModel.Size = new System.Drawing.Size(91, 22);
            this.btnUpdateModel.TabIndex = 7;
            this.btnUpdateModel.Text = "Update Model";
            this.btnUpdateModel.UseVisualStyleBackColor = true;
            this.btnUpdateModel.Click += new System.EventHandler(this.btnUpdateModel_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnUpdateModel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 447);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(486, 22);
            this.panel4.TabIndex = 0;
            // 
            // treeViewResult
            // 
            this.treeViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewResult.Location = new System.Drawing.Point(0, 0);
            this.treeViewResult.Name = "treeViewResult";
            this.treeViewResult.Size = new System.Drawing.Size(486, 447);
            this.treeViewResult.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 722);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.panel3);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "CSV/Json Import";
            this.groupBox1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.pnlXmlView.ResumeLayout(false);
            this.pnlSubSchema.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.pnlMainSchema.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtSourceFile;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSourcePath;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox txtWriteConnectionString;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnImportData;
        private System.Windows.Forms.TreeView treeViewModels;
        private System.Windows.Forms.ListBox lstMainObject;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnNewModel;
        private System.Windows.Forms.Button btnCreateReadCfg;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel pnlXmlView;
        private System.Windows.Forms.Panel pnlSubSchema;
        private System.Windows.Forms.Panel pnlMainSchema;
        private System.Windows.Forms.TreeView treeViewMainClass;
        private System.Windows.Forms.Button btnUpdateModel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TreeView treeViewResult;
    }
}

