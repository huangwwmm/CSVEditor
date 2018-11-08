using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;

public partial class MergeLocalizationForm : Form
{
	private const int MERGECOLTYPECOMBOBOX_SELECTEDINDEX_ALLCOL = 0;
	private const int MERGECOLTYPECOMBOBOX_SELECTEDINDEX_SELECTEDCOL = 1;

	/// <summary>
	/// 下面几个List在void Merge中初始化
	/// </summary>
	private List<CSVEditManager.IUndoRedo> m_DoManyThingList;
	private List<CSVEditManager.CellValueChangeItem> m_CellChangeList;
	private List<DataGridViewConsoleForm.Message> m_MessageList;
	/// <summary>
	/// 要合并的列，为null时合并所有列
	/// </summary>
	private List<int> m_MergeCols;

	public MergeLocalizationForm()
	{
		InitializeComponent();

		UpdateOkButtonEnbale();
	}

	/// <summary>
	/// 检测能否Merge，如果不能Merge弹出对话框提示原因。可以Merge
	/// </summary>
	private void DoMerge()
	{
		m_MergeCols = null;
		if (m_MergeColTypeComboBox.SelectedIndex == MERGECOLTYPECOMBOBOX_SELECTEDINDEX_SELECTEDCOL)
		{
			m_MergeCols = new List<int>();
			for (int iItem = 1; iItem < m_MergeColCheckBoxComboBox.CheckBoxItems.Count; iItem++)
			{
				if (m_MergeColCheckBoxComboBox.CheckBoxItems[iItem].Checked)
				{
					// 魔法数字：UI控件的，第一个Item的Index是1，所以要减去1.本地化Col是从第2列开始的，所以要加上1(String ID)
					m_MergeCols.Add(iItem - 1 + 1);
				}
			}
			if (m_MergeCols.Count == 0)
			{
				MessageBox.Show(CSVEditor.Properties.Resources.MergeLocalizationToolNotSelectMergeColHint
				  , "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}

		string amPath = m_AMCSVPathTextBox.Text;
		// 未选择AM文件
		if (string.IsNullOrEmpty(amPath))
		{
			MessageBox.Show(CSVEditor.Properties.Resources.MergeLocalizationToolNotChooseAMCSVHint
				, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		// AM文件不存在
		if (!File.Exists(amPath))
		{
			MessageBox.Show(CSVEditor.Properties.Resources.MergeLocalizationToolNotExistsAMCSVHint
				, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		string[][] amCSV = FileUtility.LoadFileToCSV(amPath);
		// 读取文件失败
		if (amCSV == null)
		{
			MessageBox.Show(CSVEditor.Properties.Resources.MergeLocalizationToolNotLocalizationCSVHint
				, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		// AM不是本地化表
		if (amCSV.Length < 2 || amCSV[0].Length < 2 || amCSV[0][0].Trim().ToLower() != "String ID".ToLower())
		{
			MessageBox.Show(CSVEditor.Properties.Resources.MergeLocalizationToolNotLocalizationCSVHint
				, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		DataTable originalCSV = MainForm.Instance.GetCSVForm().GetDataTable();
		// AM文件和源文件列数不一致
		if (amCSV[0].Length != originalCSV.Columns.Count)
		{
			MessageBox.Show(string.Format(CSVEditor.Properties.Resources.MergeLocalizationToolColumCountInconformityHint
				, originalCSV.Columns.Count
				, amCSV[0].Length)
				, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		// 检测AM文件和源文件列头是否一致
		for (int iCol = 0; iCol < amCSV[0].Length; iCol++)
		{
			string iterAMHead = amCSV[0][iCol];
			string iterOriginalHead = ((string)originalCSV.Rows[0][iCol]);
			// HACK Chinese_Uncensor​列的列头有注释，忽略注释，只比较"Chinese_Uncensor"。所以只取第一行
			iterAMHead = TrimAndSplitLanguage(iterAMHead);
			iterOriginalHead = TrimAndSplitLanguage(iterOriginalHead);
			if (iterAMHead.ToLower() != iterOriginalHead.ToLower())
			{
				MessageBox.Show(string.Format(CSVEditor.Properties.Resources.MergeLocalizationToolHeadNotSame
					, ConvertUtility.NumberToLetter(iCol)
					, iterAMHead
					, iterOriginalHead)
					, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}

		MergeCsv(originalCSV, amCSV);
		MessageBox.Show(CSVEditor.Properties.Resources.MergeLocalizationToolMergeCompleteHint
			, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	/// <summary>
	/// 合并文件
	/// </summary>
	private void MergeCsv(DataTable originalCSV, string[][] amCSV)
	{
		CSVForm csvForm = MainForm.Instance.GetCSVForm();
		csvForm.BeforeChangeCellValue();

		// 做的操作(用于撤销)
		m_DoManyThingList = new List<CSVEditManager.IUndoRedo>();
		m_CellChangeList = new List<CSVEditManager.CellValueChangeItem>();
		m_MessageList = new List<DataGridViewConsoleForm.Message>();

		// 遍历AM文件
		for (int iRowInAMCSV = 0; iRowInAMCSV < amCSV.Length; iRowInAMCSV++)
		{
			string[] iterAMRow = amCSV[iRowInAMCSV];
			// 这里Trim是为了匹配到源文件中的Key(Key头尾肯定不能用空白字符)
			string iterAMKey = iterAMRow[0].Trim();

			// 跳过空key，Key为空代表这行为空行
			if (string.IsNullOrWhiteSpace(iterAMKey))
			{
				continue;
			}
			// 如果key是String ID，是表头，直接无视
			if (iterAMKey.ToLower() == "String ID".ToLower())
			{
				continue;
			}
			// key中不能包含空格，跳过
			if (iterAMKey.Contains(" "))
			{
				DataGridViewConsoleForm.Message message = new DataGridViewConsoleForm.Message
				{
					Level = DataGridViewConsoleForm.Level.Warning,
					Column = 0,
					Row = iRowInAMCSV,
					Caption = CSVEditor.Properties.Resources.MergeLocalizationToolAMCSVKeyContainsSpaceMessageCaption,
					Text = string.Format(CSVEditor.Properties.Resources.MergeLocalizationToolAMCSVKeyContainsSpaceMessageText, iterAMKey)
				};
				m_MessageList.Add(message);
				continue;
			}

			// 源文件中是否存在AM文件中的Key
			// 存在的话直接merge，不存在的话，把这一行添加到源文件结尾
			bool amKeyExistedInOriginal = false;
			for (int iRowInOriginalCSV = 0; iRowInOriginalCSV < originalCSV.Rows.Count; iRowInOriginalCSV++)
			{
				DataRow iterOriginalRow = originalCSV.Rows[iRowInOriginalCSV];
				string iterOriginalKey = (string)iterOriginalRow[0];
				if (iterOriginalKey.Trim() == iterAMKey)
				{
					amKeyExistedInOriginal = true;
					MergeRow(iRowInOriginalCSV, iterAMRow, iterOriginalRow);
					break;
				}
			}

			if (!amKeyExistedInOriginal)
			{
				// 添加新行
				DataRow newRow = originalCSV.NewRow();

				CSVEditManager.DoAddRowEvent doAddRowEvent = new CSVEditManager.DoAddRowEvent
				{
					Row = originalCSV.Rows.Count
				};
				m_DoManyThingList.Add(doAddRowEvent);

				// Key
				newRow[0] = iterAMKey;
				CSVEditManager.CellValueChangeItem changeItem = new CSVEditManager.CellValueChangeItem
				{
					Row = originalCSV.Rows.Count,
					Column = 0,
					OldValue = "",
					NewValue = iterAMKey
				};
				m_CellChangeList.Add(changeItem);

				// 初始化值
				for (int iCell = 1; iCell < iterAMRow.Length; iCell++)
				{
					newRow[iCell] = "";
				}

				MergeRow(originalCSV.Rows.Count, iterAMRow, newRow, true);
				originalCSV.Rows.Add(newRow);
			}
		}

		CSVEditManager.DoCellsValueChangeEvent doCellsValueChangeEvent = new CSVEditManager.DoCellsValueChangeEvent
		{
			ChangeList = m_CellChangeList
		};
		m_DoManyThingList.Add(doCellsValueChangeEvent);

		csvForm.EditManager.DidManyThings(m_DoManyThingList);
		csvForm.AfterChangeCellValue();
		csvForm.UpdateGridHeader();

		DataGridViewConsoleForm.ShowForm(m_MessageList, csvForm.GetDataGridView(), "本地化合并");
		Close();
	}

	/// <summary>
	/// 把amRow Merge到 originalRow
	/// </summary>
	/// <param name="rowIndexInOriginal">行index，仅用于输出Log</param>
	/// <param name="amRow">AM行</param>
	/// <param name="originalRow">源行</param>
	/// <param name="isNewRow">是不是新增的行</param>
	private void MergeRow(int rowIndexInOriginal, string[] amRow, DataRow originalRow, bool isNewRow = false)
	{
		string originalRowStr = RowToString(originalRow);
		int changedCellCount = 0;

		// 魔法数字1：key不需要Merge
		for (int iCell = 1; iCell < amRow.Length; iCell++)
		{
			// 不是要合并的列，跳过
			if (m_MergeCols != null && !m_MergeCols.Contains(iCell))
			{
				continue;
			}

			string iterAMCell = amRow[iCell];
			string iterOriginalCell = (string)originalRow[iCell];

			// AM文件中为空，跳过
			if (string.IsNullOrWhiteSpace(iterAMCell))
			{
				// 魔法数字1：Text列可以为空
				if (iCell != 1)
				{
					DataGridViewConsoleForm.Message message = new DataGridViewConsoleForm.Message
					{
						Level = DataGridViewConsoleForm.Level.Warning,
						Column = iCell,
						Row = rowIndexInOriginal,
						Caption = "AM文件中，该单元格为空",
						Text = string.Format("使用源文件的值：\n({0})", iterOriginalCell)
					};
					m_MessageList.Add(message);
				}
			}
			else if (iterAMCell != iterOriginalCell)
			{
				changedCellCount++;
				CSVEditManager.CellValueChangeItem changeItem = new CSVEditManager.CellValueChangeItem
				{
					Row = rowIndexInOriginal,
					Column = iCell,
					OldValue = iterOriginalCell,
					NewValue = iterAMCell
				};
				m_CellChangeList.Add(changeItem);

				originalRow[iCell] = iterAMCell;
			}
		}

		// 添加修改记录，用于显示在控制台
		if (changedCellCount > 0)
		{
			string changedRowStr = RowToString(originalRow);

			if (isNewRow)
			{
				DataGridViewConsoleForm.Message message = new DataGridViewConsoleForm.Message
				{
					Level = DataGridViewConsoleForm.Level.Info,
					Column = -1,
					Row = rowIndexInOriginal,
					Caption = "合并：新增行",
					Text = string.Format("值：\n({0})", changedRowStr)
				};
				m_MessageList.Add(message);
			}
			else
			{
				DataGridViewConsoleForm.Message message = new DataGridViewConsoleForm.Message
				{
					Level = DataGridViewConsoleForm.Level.Info,
					Column = -1,
					Row = rowIndexInOriginal,
					Caption = string.Format("合并：修改了({0})个单元格", changedCellCount),
					Text = string.Format("源值：\n({0})\n合并后：\n({1})", originalRowStr, changedRowStr)
				};
				m_MessageList.Add(message);
			}
		}
	}

	private string RowToString(DataRow row)
	{
		System.Text.StringBuilder rowStr = new System.Text.StringBuilder();
		for (int iCell = 0; iCell < row.ItemArray.Length; iCell++)
		{
			string iterCell = (string)row[iCell];
			rowStr.Append(string.Format("\"{0}\"", iterCell));
			if (iCell != row.ItemArray.Length - 1)
			{
				rowStr.Append(",");
			}
		}
		return rowStr.ToString();
	}

	/// <summary>
	/// 对text进行Trim操作，并截取第一行
	/// </summary>
	private string TrimAndSplitLanguage(string text)
	{
		text = text.Trim();
		if (text.Contains(" "))
		{
			string[] texts = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (texts.Length > 0)
			{
				text = texts[0].Trim();
			}
		}
		return text;
	}

	private void UpdateOkButtonEnbale()
	{
		string path = m_AMCSVPathTextBox.Text;
		if (string.IsNullOrEmpty(path))
		{
			m_OkButton.Enabled = false;
		}
		else
		{
			m_OkButton.Enabled = File.Exists(path);
		}
	}

	#region UIEvent
	private void OnOkButton_Click(object sender, EventArgs e)
	{
		DoMerge();
	}

	private void OnCancelButton_Click(object sender, EventArgs e)
	{
		Close();
		Dispose();
	}

	private void OnOpenCSVFileDialogButton_Click(object sender, EventArgs e)
	{
		if (m_OpenCSVFileDialog.ShowDialog() == DialogResult.OK)
		{
			m_AMCSVPathTextBox.Text = m_OpenCSVFileDialog.FileName;
		}
	}

	private void OnAMCSVPathTextBox_TextChanged(object sender, EventArgs e)
	{
		UpdateOkButtonEnbale();
	}

	private void OnMergeLocalizationForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
	{
		MessageBox.Show(CSVEditor.Properties.Resources.MergeLocalizationToolHelpText,
			"帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);

		e.Cancel = true;
	}

	private void OnMergeLocalizationForm_Load(object sender, EventArgs e)
	{
		GlobalData.DefaultTooltip.SetToolTip(m_AMFilePathLabel, CSVEditor.Properties.Resources.AMFilePathHint);
	}

	private void OnMergeLocalizationForm_Shown(object sender, EventArgs e)
	{
		m_MergeColTypeComboBox.SelectedIndex = MERGECOLTYPECOMBOBOX_SELECTEDINDEX_ALLCOL;

		DataTable originalCSV = MainForm.Instance.GetCSVForm().GetDataTable();

		#region 初始化m_MergeColCheckBoxComboBox.Items
		m_MergeColCheckBoxComboBox.Items.Clear();
		// 魔法数字：跳过String ID列
		for (int iCol = 1; iCol < originalCSV.Columns.Count; iCol++)
		{
			m_MergeColCheckBoxComboBox.Items.Add(TrimAndSplitLanguage((string)originalCSV.Rows[0][iCol]));
		}
		#endregion
	}

	private void OnMergeColTypeComboBox_SelectedValueChanged(object sender, EventArgs e)
	{
		if (m_MergeColTypeComboBox.SelectedIndex == MERGECOLTYPECOMBOBOX_SELECTEDINDEX_ALLCOL)
		{
			m_MergeColLabel.Visible = false;
			m_MergeColCheckBoxComboBox.Visible = false;
		}
		else if (m_MergeColTypeComboBox.SelectedIndex == MERGECOLTYPECOMBOBOX_SELECTEDINDEX_SELECTEDCOL)
		{
			m_MergeColLabel.Visible = true;
			m_MergeColCheckBoxComboBox.Visible = true;
		}
	}

	private void OnAMCSVPathTextBox_DragDrop(object sender, DragEventArgs e)
	{
		object data = e.Data.GetData(DataFormats.FileDrop);
		if (data != null && data is string[] files)
		{
			if (files.Length == 1)
			{
				m_AMCSVPathTextBox.Text = files[0];
			}
			else
			{
				MessageBox.Show("不支持拖拽多个文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}

	private void OnAMCSVPathTextBox_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			e.Effect = DragDropEffects.Copy;
		}
	}
	#endregion // END UIEvent
}