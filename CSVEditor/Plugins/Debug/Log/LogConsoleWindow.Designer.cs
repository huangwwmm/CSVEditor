namespace Debug
{
	partial class LogConsoleWindow
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
			System.Windows.Forms.Label label1;
			PresentationControls.CheckBoxProperties checkBoxProperties1 = new PresentationControls.CheckBoxProperties();
			this.m_TopPanel = new System.Windows.Forms.Panel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_LevelComboBox = new PresentationControls.CheckBoxComboBox();
			this.m_EnsureCheckBox = new System.Windows.Forms.CheckBox();
			this.m_StackTraceCheckBox = new System.Windows.Forms.CheckBox();
			this.m_FilterButton = new System.Windows.Forms.Button();
			this.m_RefreshButton = new System.Windows.Forms.Button();
			this.m_ClearButton = new System.Windows.Forms.Button();
			this.m_OpenButton = new System.Windows.Forms.Button();
			this.m_ListView = new System.Windows.Forms.ListView();
			this.m_LevelColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.m_DTColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.m_TagColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.m_TextColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.m_OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.m_LogRichTextBox = new System.Windows.Forms.RichTextBox();
			label1 = new System.Windows.Forms.Label();
			this.m_TopPanel.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(3, 4);
			label1.Name = "label1";
			label1.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			label1.Size = new System.Drawing.Size(41, 20);
			label1.TabIndex = 4;
			label1.Text = "Level:";
			// 
			// m_TopPanel
			// 
			this.m_TopPanel.Controls.Add(this.flowLayoutPanel1);
			this.m_TopPanel.Controls.Add(this.m_OpenButton);
			this.m_TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_TopPanel.Location = new System.Drawing.Point(0, 0);
			this.m_TopPanel.Name = "m_TopPanel";
			this.m_TopPanel.Size = new System.Drawing.Size(1035, 37);
			this.m_TopPanel.TabIndex = 0;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Controls.Add(label1);
			this.flowLayoutPanel1.Controls.Add(this.m_LevelComboBox);
			this.flowLayoutPanel1.Controls.Add(this.m_EnsureCheckBox);
			this.flowLayoutPanel1.Controls.Add(this.m_StackTraceCheckBox);
			this.flowLayoutPanel1.Controls.Add(this.m_FilterButton);
			this.flowLayoutPanel1.Controls.Add(this.m_RefreshButton);
			this.flowLayoutPanel1.Controls.Add(this.m_ClearButton);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(509, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
			this.flowLayoutPanel1.Size = new System.Drawing.Size(518, 33);
			this.flowLayoutPanel1.TabIndex = 3;
			// 
			// m_LevelComboBox
			// 
			checkBoxProperties1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.m_LevelComboBox.CheckBoxProperties = checkBoxProperties1;
			this.m_LevelComboBox.DisplayMemberSingleItem = "";
			this.m_LevelComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.m_LevelComboBox.Location = new System.Drawing.Point(50, 7);
			this.m_LevelComboBox.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
			this.m_LevelComboBox.Name = "m_LevelComboBox";
			this.m_LevelComboBox.Size = new System.Drawing.Size(108, 20);
			this.m_LevelComboBox.TabIndex = 3;
			// 
			// m_EnsureCheckBox
			// 
			this.m_EnsureCheckBox.AutoSize = true;
			this.m_EnsureCheckBox.Checked = true;
			this.m_EnsureCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_EnsureCheckBox.Location = new System.Drawing.Point(185, 7);
			this.m_EnsureCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
			this.m_EnsureCheckBox.Name = "m_EnsureCheckBox";
			this.m_EnsureCheckBox.Size = new System.Drawing.Size(60, 16);
			this.m_EnsureCheckBox.TabIndex = 6;
			this.m_EnsureCheckBox.Text = "Ensure";
			this.m_EnsureCheckBox.UseVisualStyleBackColor = true;
			// 
			// m_StackTraceCheckBox
			// 
			this.m_StackTraceCheckBox.AutoSize = true;
			this.m_StackTraceCheckBox.Location = new System.Drawing.Point(272, 7);
			this.m_StackTraceCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
			this.m_StackTraceCheckBox.Name = "m_StackTraceCheckBox";
			this.m_StackTraceCheckBox.Size = new System.Drawing.Size(84, 16);
			this.m_StackTraceCheckBox.TabIndex = 4;
			this.m_StackTraceCheckBox.Text = "StackTrace";
			this.m_StackTraceCheckBox.UseVisualStyleBackColor = true;
			this.m_StackTraceCheckBox.CheckedChanged += new System.EventHandler(this.OnStackTraceCheckBox_CheckedChanged);
			// 
			// m_FilterButton
			// 
			this.m_FilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_FilterButton.Location = new System.Drawing.Point(383, 7);
			this.m_FilterButton.Name = "m_FilterButton";
			this.m_FilterButton.Size = new System.Drawing.Size(40, 23);
			this.m_FilterButton.TabIndex = 2;
			this.m_FilterButton.Text = "筛选";
			this.m_FilterButton.UseVisualStyleBackColor = true;
			this.m_FilterButton.Click += new System.EventHandler(this.OnFilterButton_Click);
			// 
			// m_RefreshButton
			// 
			this.m_RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_RefreshButton.Location = new System.Drawing.Point(429, 7);
			this.m_RefreshButton.Name = "m_RefreshButton";
			this.m_RefreshButton.Size = new System.Drawing.Size(40, 23);
			this.m_RefreshButton.TabIndex = 1;
			this.m_RefreshButton.Text = "刷新";
			this.m_RefreshButton.UseVisualStyleBackColor = true;
			this.m_RefreshButton.Click += new System.EventHandler(this.OnRefreshButton_Click);
			// 
			// m_ClearButton
			// 
			this.m_ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ClearButton.Location = new System.Drawing.Point(475, 7);
			this.m_ClearButton.Name = "m_ClearButton";
			this.m_ClearButton.Size = new System.Drawing.Size(40, 23);
			this.m_ClearButton.TabIndex = 5;
			this.m_ClearButton.Text = "清空";
			this.m_ClearButton.UseVisualStyleBackColor = true;
			this.m_ClearButton.Click += new System.EventHandler(this.OnClearButton_Click);
			// 
			// m_OpenButton
			// 
			this.m_OpenButton.Location = new System.Drawing.Point(12, 8);
			this.m_OpenButton.Name = "m_OpenButton";
			this.m_OpenButton.Size = new System.Drawing.Size(90, 23);
			this.m_OpenButton.TabIndex = 0;
			this.m_OpenButton.Text = "打开日志文件";
			this.m_OpenButton.UseVisualStyleBackColor = true;
			this.m_OpenButton.Click += new System.EventHandler(this.OnOpenButton_Click);
			// 
			// m_ListView
			// 
			this.m_ListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ListView.BackColor = System.Drawing.Color.Gainsboro;
			this.m_ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_LevelColumnHeader,
            this.m_DTColumnHeader,
            this.m_TagColumnHeader,
            this.m_TextColumnHeader});
			this.m_ListView.Font = new System.Drawing.Font("Microsoft YaHei", 11F);
			this.m_ListView.FullRowSelect = true;
			this.m_ListView.GridLines = true;
			this.m_ListView.HideSelection = false;
			this.m_ListView.Location = new System.Drawing.Point(0, 37);
			this.m_ListView.MultiSelect = false;
			this.m_ListView.Name = "m_ListView";
			this.m_ListView.Size = new System.Drawing.Size(1035, 490);
			this.m_ListView.TabIndex = 1;
			this.m_ListView.UseCompatibleStateImageBehavior = false;
			this.m_ListView.View = System.Windows.Forms.View.Details;
			this.m_ListView.SelectedIndexChanged += new System.EventHandler(this.OnListView_SelectedIndexChanged);
			// 
			// m_LevelColumnHeader
			// 
			this.m_LevelColumnHeader.Text = "";
			this.m_LevelColumnHeader.Width = 38;
			// 
			// m_DTColumnHeader
			// 
			this.m_DTColumnHeader.Text = "Time";
			this.m_DTColumnHeader.Width = 208;
			// 
			// m_TagColumnHeader
			// 
			this.m_TagColumnHeader.Text = "Tag";
			this.m_TagColumnHeader.Width = 152;
			// 
			// m_TextColumnHeader
			// 
			this.m_TextColumnHeader.Text = "Text";
			this.m_TextColumnHeader.Width = 620;
			// 
			// m_OpenFileDialog
			// 
			this.m_OpenFileDialog.Filter = "*.txt|*.log";
			// 
			// m_LogRichTextBox
			// 
			this.m_LogRichTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.m_LogRichTextBox.Font = new System.Drawing.Font("SimSun", 10F);
			this.m_LogRichTextBox.Location = new System.Drawing.Point(0, 533);
			this.m_LogRichTextBox.Name = "m_LogRichTextBox";
			this.m_LogRichTextBox.ReadOnly = true;
			this.m_LogRichTextBox.Size = new System.Drawing.Size(1035, 264);
			this.m_LogRichTextBox.TabIndex = 2;
			this.m_LogRichTextBox.Text = "";
			// 
			// LogConsoleWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1035, 797);
			this.Controls.Add(this.m_LogRichTextBox);
			this.Controls.Add(this.m_ListView);
			this.Controls.Add(this.m_TopPanel);
			this.Name = "LogConsoleWindow";
			this.Text = "Console";
			this.Shown += new System.EventHandler(this.OnConsoleWindow_Shown);
			this.m_TopPanel.ResumeLayout(false);
			this.m_TopPanel.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel m_TopPanel;
		private System.Windows.Forms.Button m_RefreshButton;
		private System.Windows.Forms.Button m_OpenButton;
		private System.Windows.Forms.ListView m_ListView;
		private System.Windows.Forms.ColumnHeader m_LevelColumnHeader;
		private System.Windows.Forms.ColumnHeader m_DTColumnHeader;
		private System.Windows.Forms.ColumnHeader m_TagColumnHeader;
		private System.Windows.Forms.ColumnHeader m_TextColumnHeader;
		private System.Windows.Forms.OpenFileDialog m_OpenFileDialog;
		private System.Windows.Forms.RichTextBox m_LogRichTextBox;
		private System.Windows.Forms.Button m_FilterButton;
		private PresentationControls.CheckBoxComboBox m_LevelComboBox;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button m_ClearButton;
		private System.Windows.Forms.CheckBox m_StackTraceCheckBox;
		private System.Windows.Forms.CheckBox m_EnsureCheckBox;
	}
}