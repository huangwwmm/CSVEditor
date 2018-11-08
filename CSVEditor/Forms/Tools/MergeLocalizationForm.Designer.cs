partial class MergeLocalizationForm
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
			this.m_AMFilePathLabel = new System.Windows.Forms.Label();
			this.m_OpenCSVFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.m_AMCSVPathTextBox = new System.Windows.Forms.TextBox();
			this.m_OpenCSVFileDialogButton = new System.Windows.Forms.Button();
			this.m_CancelButton = new System.Windows.Forms.Button();
			this.m_OkButton = new System.Windows.Forms.Button();
			this.m_MergeColTypeComboBox = new System.Windows.Forms.ComboBox();
			this.m_MergeColLabel = new System.Windows.Forms.Label();
			this.m_MergeColCheckBoxComboBox = new PresentationControls.CheckBoxComboBox();
			label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(13, 63);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(77, 12);
			label1.TabIndex = 5;
			label1.Text = "要合并的列：";
			// 
			// m_AMFilePathLabel
			// 
			this.m_AMFilePathLabel.AutoSize = true;
			this.m_AMFilePathLabel.Location = new System.Drawing.Point(13, 12);
			this.m_AMFilePathLabel.Name = "m_AMFilePathLabel";
			this.m_AMFilePathLabel.Size = new System.Drawing.Size(95, 12);
			this.m_AMFilePathLabel.TabIndex = 0;
			this.m_AMFilePathLabel.Text = "AM文件路径(?)：";
			// 
			// m_OpenCSVFileDialog
			// 
			this.m_OpenCSVFileDialog.FileName = "openFileDialog1";
			this.m_OpenCSVFileDialog.Filter = "*.csv|*.csv";
			// 
			// m_AMCSVPathTextBox
			// 
			this.m_AMCSVPathTextBox.AllowDrop = true;
			this.m_AMCSVPathTextBox.Location = new System.Drawing.Point(13, 29);
			this.m_AMCSVPathTextBox.Name = "m_AMCSVPathTextBox";
			this.m_AMCSVPathTextBox.Size = new System.Drawing.Size(438, 21);
			this.m_AMCSVPathTextBox.TabIndex = 1;
			this.m_AMCSVPathTextBox.TextChanged += new System.EventHandler(this.OnAMCSVPathTextBox_TextChanged);
			this.m_AMCSVPathTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnAMCSVPathTextBox_DragDrop);
			this.m_AMCSVPathTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnAMCSVPathTextBox_DragEnter);
			// 
			// m_OpenCSVFileDialogButton
			// 
			this.m_OpenCSVFileDialogButton.Location = new System.Drawing.Point(457, 25);
			this.m_OpenCSVFileDialogButton.Name = "m_OpenCSVFileDialogButton";
			this.m_OpenCSVFileDialogButton.Size = new System.Drawing.Size(42, 26);
			this.m_OpenCSVFileDialogButton.TabIndex = 2;
			this.m_OpenCSVFileDialogButton.Text = "...";
			this.m_OpenCSVFileDialogButton.UseVisualStyleBackColor = true;
			this.m_OpenCSVFileDialogButton.Click += new System.EventHandler(this.OnOpenCSVFileDialogButton_Click);
			// 
			// m_CancelButton
			// 
			this.m_CancelButton.Location = new System.Drawing.Point(424, 94);
			this.m_CancelButton.Name = "m_CancelButton";
			this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
			this.m_CancelButton.TabIndex = 3;
			this.m_CancelButton.Text = "取消";
			this.m_CancelButton.UseVisualStyleBackColor = true;
			this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButton_Click);
			// 
			// m_OkButton
			// 
			this.m_OkButton.Location = new System.Drawing.Point(336, 94);
			this.m_OkButton.Name = "m_OkButton";
			this.m_OkButton.Size = new System.Drawing.Size(75, 23);
			this.m_OkButton.TabIndex = 4;
			this.m_OkButton.Text = "运行";
			this.m_OkButton.UseVisualStyleBackColor = true;
			this.m_OkButton.Click += new System.EventHandler(this.OnOkButton_Click);
			// 
			// m_MergeColTypeComboBox
			// 
			this.m_MergeColTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_MergeColTypeComboBox.Items.AddRange(new object[] {
            "所有列",
            "指定列"});
			this.m_MergeColTypeComboBox.Location = new System.Drawing.Point(97, 63);
			this.m_MergeColTypeComboBox.Name = "m_MergeColTypeComboBox";
			this.m_MergeColTypeComboBox.Size = new System.Drawing.Size(78, 20);
			this.m_MergeColTypeComboBox.TabIndex = 6;
			this.m_MergeColTypeComboBox.SelectedValueChanged += new System.EventHandler(this.OnMergeColTypeComboBox_SelectedValueChanged);
			// 
			// m_MergeColLabel
			// 
			this.m_MergeColLabel.AutoSize = true;
			this.m_MergeColLabel.Location = new System.Drawing.Point(193, 63);
			this.m_MergeColLabel.Name = "m_MergeColLabel";
			this.m_MergeColLabel.Size = new System.Drawing.Size(53, 12);
			this.m_MergeColLabel.TabIndex = 7;
			this.m_MergeColLabel.Text = "指定列：";
			// 
			// m_MergeColCheckBoxComboBox
			// 
			checkBoxProperties1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.m_MergeColCheckBoxComboBox.CheckBoxProperties = checkBoxProperties1;
			this.m_MergeColCheckBoxComboBox.DisplayMemberSingleItem = "";
			this.m_MergeColCheckBoxComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_MergeColCheckBoxComboBox.FormattingEnabled = true;
			this.m_MergeColCheckBoxComboBox.Location = new System.Drawing.Point(252, 63);
			this.m_MergeColCheckBoxComboBox.Name = "m_MergeColCheckBoxComboBox";
			this.m_MergeColCheckBoxComboBox.Size = new System.Drawing.Size(247, 20);
			this.m_MergeColCheckBoxComboBox.TabIndex = 8;
			// 
			// MergeLocalizationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(511, 124);
			this.Controls.Add(this.m_MergeColCheckBoxComboBox);
			this.Controls.Add(this.m_MergeColLabel);
			this.Controls.Add(this.m_MergeColTypeComboBox);
			this.Controls.Add(label1);
			this.Controls.Add(this.m_OkButton);
			this.Controls.Add(this.m_CancelButton);
			this.Controls.Add(this.m_OpenCSVFileDialogButton);
			this.Controls.Add(this.m_AMCSVPathTextBox);
			this.Controls.Add(this.m_AMFilePathLabel);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MergeLocalizationForm";
			this.Text = "本地化合并";
			this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.OnMergeLocalizationForm_HelpButtonClicked);
			this.Load += new System.EventHandler(this.OnMergeLocalizationForm_Load);
			this.Shown += new System.EventHandler(this.OnMergeLocalizationForm_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.OpenFileDialog m_OpenCSVFileDialog;
	private System.Windows.Forms.TextBox m_AMCSVPathTextBox;
	private System.Windows.Forms.Button m_OpenCSVFileDialogButton;
	private System.Windows.Forms.Button m_CancelButton;
	private System.Windows.Forms.Button m_OkButton;
	private System.Windows.Forms.Label m_AMFilePathLabel;
	private System.Windows.Forms.ComboBox m_MergeColTypeComboBox;
	private System.Windows.Forms.Label m_MergeColLabel;
	private PresentationControls.CheckBoxComboBox m_MergeColCheckBoxComboBox;
}