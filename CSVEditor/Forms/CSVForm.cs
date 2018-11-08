using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;

public partial class CSVForm : Form
{
	/// <summary>
	/// 源文件 完整路径 如: C://a.csv
	/// </summary>
	public string SourcePath;

	public CSVEditManager EditManager;

	public bool Initialized = false;

	public bool DataChanged = false;

	private DataTable m_DataTable;
	private DataTable m_CopyDataTable;

	private CSVLayout m_Layout;

	/// <summary>
	/// 源文件 文件名 如: a.csv
	/// </summary>
	private string m_SourceFile;

	/// <summary>
	/// 源文件的副本 完整路径
	/// </summary>
	private string m_SourceCopyPath;

	public CSVForm(string path)
	{
		InitializeComponent();

		SourcePath = path;

		EditManager = new CSVEditManager(this);
	}

	public DataGridView GetDataGridView()
	{
		return m_DataGridView;
	}

	public DataTable GetDataTable()
	{
		return m_DataTable;
	}

	public void BeforeChangeCellValue()
	{
		m_DataGridView.CellValueChanged -= OnDataGridView_CellValueChanged;
	}

	public void AfterChangeCellValue()
	{
		DataChanged = true;
		UpdateFormText();

		m_CopyDataTable = m_DataTable.Copy();
		m_DataGridView.CellValueChanged += OnDataGridView_CellValueChanged;
	}

	/// <summary>
	/// 移除所有制表符并转换所有换行符
	/// </summary>
	public void RemoveAllTabAndConvertAllCRLFToLFs()
	{
		BeforeChangeCellValue();

		List<CSVEditManager.CellValueChangeItem> changeList = new List<CSVEditManager.CellValueChangeItem>();
		List<DataGridViewConsoleForm.Message> messageList = new List<DataGridViewConsoleForm.Message>();

		for (int rowIdx = 0; rowIdx < m_DataGridView.Rows.Count; rowIdx++)
		{
			DataGridViewRow iterRow = m_DataGridView.Rows[rowIdx];
			for (int colIdx = 0; colIdx < iterRow.Cells.Count; colIdx++)
			{
				DataGridViewCell iterCell = iterRow.Cells[colIdx];
				string iterValue = (string)iterCell.Value;
				if (!string.IsNullOrEmpty(iterValue))
				{
					string newValue = iterValue.Replace("\t", "");
					newValue = newValue.Replace("\r\n", "\n");

					if (iterValue != newValue)
					{
						CSVEditManager.CellValueChangeItem change = new CSVEditManager.CellValueChangeItem();
						change.Row = rowIdx;
						change.Column = colIdx;
						change.OldValue = iterValue;
						change.NewValue = newValue;
						changeList.Add(change);

						DataGridViewConsoleForm.Message message = new DataGridViewConsoleForm.Message();
						message.Level = DataGridViewConsoleForm.Level.Info;
						//int indexOfTab = iterValue.IndexOf('\t');
						//if (indexOfTab > 0 && indexOfTab < iterValue.Length - 1)
						//{
						//	message.Level = DataGridViewConsoleForm.Level.Warning;
						//}
						message.Row = rowIdx;
						message.Column = colIdx;
						message.Caption = "移除制表符并转换换行符";
						message.Text = string.Format("源：\n({0})\n处理后：\n({1})", iterValue, newValue);
						messageList.Add(message);

						iterCell.Value = newValue;
					}
				}
			}
		}
		EditManager.DidCellsValueChange(changeList);

		AfterChangeCellValue();

		DataGridViewConsoleForm.ShowForm(messageList, m_DataGridView, "移除所有制表符并转换所有换行符");
		MessageBox.Show(string.Format("移除所有制表符并转换所有换行符完成, 转换了{0}个单元格", messageList.Count), "提示");
	}

	/// <summary>
	/// 更新窗口Text
	/// 文件没有保存到源文件就在开头加 ?
	/// 文件有改动就在结尾加 *
	/// </summary>
	public void UpdateFormText()
	{
		string newFormText = string.Format("{0}{1}  -检验规则({2})",
			m_SourceFile,
			(DataChanged ? "*" : ""),
			VerifierUtility.GetVerifierInfo(m_Layout.Verifier).Display);

		if (Text != newFormText)
		{
			Text = newFormText;
			MainForm.Instance.UpdateFormText();
		}
	}

	/// <summary>
	/// 更新标题
	/// 行:数字1~x
	/// 列:字母A~Z
	/// </summary>
	public void UpdateGridHeader()
	{
		for (int colIdx = 0; colIdx < m_DataGridView.Columns.Count; colIdx++)
		{
			// 列数不能超过26
			m_DataGridView.Columns[colIdx].HeaderText = ConvertUtility.NumberToLetter(colIdx + 1);
			m_DataGridView.Columns[colIdx].SortMode = DataGridViewColumnSortMode.Programmatic;
		}
		// 更新行号
		for (int rowIdx = 0; rowIdx < m_DataGridView.RowCount; rowIdx++)
		{
			m_DataGridView.Rows[rowIdx].HeaderCell.Value = (rowIdx + 1).ToString();
		}
	}

	#region Layout
	public CSVLayout GetLayout()
	{
		return m_Layout;
	}

	public void SaveLayout()
	{
		SaveCellSize();
		SaveFrozen();
		CSVLayoutManager.Instance.Save();
	}

	public void LoadLayout()
	{
		m_Layout = CSVLayoutManager.Instance.LoadOrCreate(SourcePath);

		LoadCellSize();
		LoadFrozen();

		UpdateFormText();
	}
	#endregion // End Layout

	#region File
	/// <summary>
	/// 保存文件到源文件
	/// </summary>
	/// <returns>保存是否成功</returns>
	public bool SaveFile()
	{
		bool saveSuccess = SaveFile(SourcePath);
		if (saveSuccess)
		{
			DataChanged = false;
		}
		UpdateFormText();
		return saveSuccess;
	}

	/// <summary>
	/// 保存文件到path，可能是源文件也可能是其他文件
	/// </summary>
	/// <param name="path"></param>
	/// <returns保存是否成功></returns>
	public bool SaveFile(string path)
	{
		SaveLayout();

		DataGridViewConsoleForm.Level verifyLevel = VerifySelfAndShowConsole("保存文件");

		bool canSave = false;
		if (verifyLevel == DataGridViewConsoleForm.Level.Info)
		{
			canSave = true;
		}
		else if (verifyLevel == DataGridViewConsoleForm.Level.Warning)
		{
			if (MessageBox.Show("您现在有Warning，确定存储吗?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				canSave = true;
			}
		}

		if (canSave)
		{
			// 保存文件
			CSVExport myExport = new CSVExport(",", false);
			try
			{
				for (int rowIdx = 0; rowIdx < m_DataTable.Rows.Count; rowIdx++)
				{
					myExport.AddRow();
					DataRow dataRow = m_DataTable.Rows[rowIdx];
					for (int colIdx = 0; colIdx < m_DataTable.Columns.Count; colIdx++)
					{
						string value = (string)dataRow[colIdx];
						myExport[colIdx.ToString()] = value;
					}
				}

				myExport.ExportToFile(path);
			}
			catch (Exception ex)
			{
				DebugUtility.ShowExceptionMessageBox(string.Format("保存文件({0})失败", path), ex);
				return false;
			}
			return true;
		}
		else
		{
			MessageBox.Show(string.Format("保存文件({0})失败", path), "提示");
			return false;
		}
	}
	#endregion // End File

	#region Verify
	public DataGridViewConsoleForm.Level VerifySelf(out List<DataGridViewConsoleForm.Message> messageList)
	{
		return VerifierUtility.VerifyWithVerifier(m_Layout.Verifier, m_DataGridView, out messageList);
	}

	public DataGridViewConsoleForm.Level VerifySelfAndShowConsole(string consoleTitle)
	{
		DataGridViewConsoleForm.Level verifyLevel = VerifySelf(out List<DataGridViewConsoleForm.Message> messageList);
		DataGridViewConsoleForm.ShowForm(messageList, m_DataGridView, consoleTitle);
		return verifyLevel;
	}
	#endregion

	/// <summary>
	/// 监听Alt+回车键  
	/// 如果单元格正在被编辑, 就在光标处添加换行(\r\n)
	/// </summary>
	protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
	{
		if (keyData == (Keys.Enter | Keys.Alt)
			&& m_DataGridView.IsCurrentCellInEditMode)
		{
			try
			{
				TextBox textBox = m_DataGridView.EditingControl as TextBox;
				int nStart = textBox.SelectionStart;
				string oldValue = textBox.Text;
				string newValue = oldValue.Insert(nStart, "\r\n");
				textBox.Text = newValue;
				EditManager.DidCellValueChange(m_DataGridView.CurrentCell.ColumnIndex, m_DataGridView.CurrentCell.RowIndex, oldValue, newValue);
			}
			catch (Exception ex)
			{
				DebugUtility.ShowExceptionMessageBox("插入换行符失败", ex);
			}
			return true;
		}
		else
		{
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}

	/// <summary>
	/// 拷贝源文件文件副本
	/// </summary>
	/// <returns>副本文件名</returns>
	private void CopySourceFile()
	{
		m_SourceCopyPath = string.Format("{0}{1}.{2}", Path.GetTempPath(), m_SourceFile, Guid.NewGuid());
		File.Copy(SourcePath, m_SourceCopyPath);
	}

	private bool LoadFileToDataTable(string path)
	{
		string[][] csvTable = FileUtility.LoadFileToCSV(path);
		if (csvTable == null)
		{
			return false;
		}

		// csv->DataTable
		try
		{
			m_DataTable = ConvertUtility.CSVToDataTable(csvTable);
			m_CopyDataTable = m_DataTable.Copy();

			m_DataGridView.DataSource = m_DataTable;

			UpdateGridHeader();
			LoadLayout();
			MainForm.Instance.UpdateCellEdit();

			m_DataGridView.CellValueChanged += OnDataGridView_CellValueChanged;
			EditManager.DoSomething += OnRedoUndo_DoSomethingChange;
		}
		catch (Exception ex)
		{
			DebugUtility.ShowExceptionMessageBox("csv转DataTable失败:" + path, ex);
			return false;
		}
		return true;
	}

	#region Layout
	private void LoadCellSize()
	{
		// Column
		if (m_Layout.ColumnWidths != null)
		{
			for (int colIdx = 0; colIdx < m_Layout.ColumnWidths.Length; colIdx++)
			{
				if (colIdx == m_DataGridView.Columns.Count)
				{
					break;
				}
				m_DataGridView.Columns[colIdx].Width = m_Layout.ColumnWidths[colIdx];
			}
		}
	}

	private void SaveCellSize()
	{
		// Column
		m_Layout.ColumnWidths = new int[m_DataGridView.Columns.Count];
		for (int colIdx = 0; colIdx < m_DataGridView.Columns.Count; colIdx++)
		{
			m_Layout.ColumnWidths[colIdx] = m_DataGridView.Columns[colIdx].Width;
		}
	}

	private void LoadFrozen()
	{
		if (m_Layout.FrozenRow >= 0)
		{
			m_DataGridView.Rows[m_Layout.FrozenRow >= m_DataGridView.RowCount ? m_DataGridView.RowCount - 1 : m_Layout.FrozenRow].Frozen = true;
		}
		if (m_Layout.FrozenColumn >= 0)
		{
			m_DataGridView.Columns[m_Layout.FrozenColumn >= m_DataGridView.ColumnCount ? m_DataGridView.ColumnCount - 1 : m_Layout.FrozenColumn].Frozen = true;
		}
	}

	private void SaveFrozen()
	{
		m_Layout.FrozenRow = -1;
		for (int rowIdx = 0; rowIdx < m_DataGridView.RowCount; rowIdx++)
		{
			if (!m_DataGridView.Rows[rowIdx].Frozen)
			{
				break;
			}
			m_Layout.FrozenRow = rowIdx;
		}

		m_Layout.FrozenColumn = -1;
		for (int colIdx = 0; colIdx < m_DataGridView.ColumnCount; colIdx++)
		{
			if (!m_DataGridView.Columns[colIdx].Frozen)
			{
				break;
			}
			m_Layout.FrozenColumn = colIdx;
		}
	}
	#endregion // End Layout

	private bool DataGridViewCellPaintingNeedNote(DataGridViewCell cell)
	{
		return !string.IsNullOrEmpty(cell.ToolTipText);
	}

	private bool DataGridViewCellPaintingNeedMultiline(DataGridViewCell cell)
	{
		return cell.Value.ToString().Contains("\n");
	}

	#region UIEvent
	/// <summary>
	/// 窗口加载时读取csv文件
	/// </summary>
	private void OnForm_Load(object sender, EventArgs e)
	{
		// 初始化文件，创建文件副本
		try
		{
			m_SourceFile = Path.GetFileName(SourcePath);
			MainForm.Instance.Text = Text;
			CopySourceFile();
		}
		catch (Exception ex)
		{
			DebugUtility.ShowExceptionMessageBox("初始化文件失败:" + SourcePath, ex);
			return;
		}

		Initialized = LoadFileToDataTable(SourcePath);
		if (Initialized)
		{
			UpdateFormText();
		}
	}

	private void OnForm_Shown(object sender, EventArgs e)
	{
		if (!Initialized)
		{
			Close();
		}
	}

	/// <summary>
	/// RedoUndo触发  
	/// 因为(Re\Un)Do时要取消DataGridView的监听, 所以在这里进行 数据改变 & 更新标题
	/// </summary>
	private void OnRedoUndo_DoSomethingChange(object sender, CSVEditManager.DoSomethingEventArgs e)
	{
		if (e.MyDoType == CSVEditManager.DoType.CellsValueChange)
		{
			OnDataGridViewData_Change();
		}
		else if (e.MyDoType == CSVEditManager.DoType.AddRow)
		{
			OnDataGridViewData_Change();
			UpdateGridHeader();
		}
		else if (e.MyDoType == CSVEditManager.DoType.ManyThings)
		{
			OnDataGridViewData_Change();
			UpdateGridHeader();
		}
	}

	/// <summary>
	/// 数据改变时更新标题
	/// </summary>
	private void OnDataGridViewData_Change()
	{
		DataChanged = true;

		UpdateFormText();
	}

	/// <summary>
	/// 点击DataGridViewCell时
	/// 切换SelectionMode 
	/// 改变DataGridView选择的行列 
	/// 更新菜单项状态
	/// </summary>
	private void OnDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
	{
		if (e.ColumnIndex < 0 && e.RowIndex < 0)
		{
		}
		else if (e.ColumnIndex < 0)
		{
			m_DataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
		}
		else if (e.RowIndex < 0)
		{
			m_DataGridView.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
		}
		// 在右键菜单弹出前触发, 在这里初始化右键菜单
		if (e.Button == MouseButtons.Right)
		{
			m_InsertNewRowToDownToolStripMenuItem.Enabled = false;
			m_InsertNewRowToUpToolStripMenuItem.Enabled = false;
			m_FrozenToolStripMenuItem.Enabled = false;
			m_UnFrozenToolStripMenuItem.Enabled = false;
			m_AddColWidthToolStripMenuItem.Enabled = false;
			m_AddRowHeightToolStripMenuItem.Enabled = false;
			m_EditNoteToolStripMenuItem.Enabled = false;
			m_InsertToUpFromClipboardToolStripMenuItem.Enabled = false;
			m_InsertToDownFromClipboardToolStripMenuItem.Enabled = false;

			// 右键菜单里的不支持多行、多列操作
			if (e.ColumnIndex < 0 && e.RowIndex < 0)
			{
				return;
			}
			// 点击行标题
			else if (e.ColumnIndex < 0)
			{
				m_InsertNewRowToDownToolStripMenuItem.Enabled = true;
				m_InsertNewRowToUpToolStripMenuItem.Enabled = true;

				m_DataGridView.ClearSelection();
				m_DataGridView.Rows[e.RowIndex].Selected = true;

				if (m_DataGridView.Rows[e.RowIndex].Frozen)
				{
					m_UnFrozenToolStripMenuItem.Enabled = true;
				}
				else
				{
					m_FrozenToolStripMenuItem.Enabled = true;
				}

				m_AddRowHeightToolStripMenuItem.Enabled = true;

				if (EditManager.CanInsertFromClipboard())
				{
					m_InsertToUpFromClipboardToolStripMenuItem.Enabled = true;
					m_InsertToDownFromClipboardToolStripMenuItem.Enabled = true;
				}
			}
			// 点击列标题
			else if (e.RowIndex < 0)
			{
				m_DataGridView.ClearSelection();
				m_DataGridView.Columns[e.ColumnIndex].Selected = true;

				if (m_DataGridView.Columns[e.ColumnIndex].Frozen)
				{
					m_UnFrozenToolStripMenuItem.Enabled = true;
				}
				else
				{
					m_FrozenToolStripMenuItem.Enabled = true;
				}

				m_AddColWidthToolStripMenuItem.Enabled = true;
			}
			// 点击单元格
			else
			{
				m_DataGridView.ClearSelection();
				m_DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
				m_EditNoteToolStripMenuItem.Enabled = true;

				m_AddRowHeightToolStripMenuItem.Enabled = true;
				m_AddColWidthToolStripMenuItem.Enabled = true;
			}
		}
	}

	/// <summary>
	/// 插入行并添加Do事件到RedoUndo里
	/// </summary>
	private void OnInsertNewRowToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
	{
		if (!Initialized)
		{
			return;
		}

		int offset = 0;
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (item == m_InsertNewRowToDownToolStripMenuItem)
		{
			offset = 1;
		}
		else if (item == m_InsertNewRowToUpToolStripMenuItem)
		{
			offset = 0;
		}
		else
		{
			return;
		}
		if (m_DataGridView.SelectedRows.Count < 1)
		{
			return;
		}

		BeforeChangeCellValue();
		int index = m_DataGridView.SelectedRows[0].Index + offset;
		DataGridViewUtility.InsertNewRow(m_DataGridView, m_DataTable, index);

		m_DataGridView.ClearSelection();
		m_DataGridView.Rows[index].Selected = true;
		EditManager.DidAddRow(index);

		UpdateGridHeader();
		AfterChangeCellValue();
	}

	private void OnInsertFromClipboardToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
	{
		if (!Initialized)
		{
			return;
		}

		int offset = 0;
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (item == m_InsertToDownFromClipboardToolStripMenuItem)
		{
			offset = 1;
		}
		else if (item == m_InsertToUpFromClipboardToolStripMenuItem)
		{
			offset = 0;
		}
		else
		{
			return;
		}

		int insertRowIndex = m_DataGridView.SelectedRows[0].Index + offset;
		EditManager.InsertFromClipboard(m_DataGridView, m_DataTable, insertRowIndex);
	}

	/// <summary>
	/// 单元格值改变时添加Did事件
	/// </summary>
	private void OnDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || e.ColumnIndex < 0)
		{
			return;
		}
		string oldValue = m_CopyDataTable.Rows[e.RowIndex][e.ColumnIndex].ToString();
		string newValue = m_DataTable.Rows[e.RowIndex][e.ColumnIndex].ToString();
		EditManager.DidCellValueChange(e.ColumnIndex, e.RowIndex, oldValue, newValue);

		m_CopyDataTable.Rows[e.RowIndex][e.ColumnIndex] = newValue;
		OnDataGridViewData_Change();
	}

	/// <summary>
	/// 绘制单元格时
	/// </summary>
	private void OnDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
	{
		if (e.ColumnIndex < 0 || e.RowIndex < 0)
		{
			return;
		}

		DataGridViewCell cell = m_DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
		if (cell.Selected)
		{
			return;
		}
		bool needNote = DataGridViewCellPaintingNeedNote(cell);
		bool needMultiline = DataGridViewCellPaintingNeedMultiline(cell);
		if (needNote || needMultiline)
		{
			e.Paint(e.ClipBounds, e.PaintParts);
			e.Handled = true;
		}

		if (needNote)
		{
			OnDataGridView_CellPainting_Note(cell, e);
		}
		if (needMultiline)
		{
			OnDataGridView_CellPainting_Multiline(cell, e);
		}
	}

	/// <summary>
	/// 绘制单元格时, 显示批注标记
	/// </summary>
	private void OnDataGridView_CellPainting_Note(DataGridViewCell cell, DataGridViewCellPaintingEventArgs e)
	{
		using (Brush gridBrush = new SolidBrush(Color.Red))
		{
			Point[] polygonPoints = new Point[3];
			polygonPoints[0] = new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
			polygonPoints[1] = new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1 - GlobalData.CSV_NOTE_POLYGON_SIZE);
			polygonPoints[2] = new Point(e.CellBounds.Right - 1 - GlobalData.CSV_NOTE_POLYGON_SIZE, e.CellBounds.Bottom - 1);
			e.Graphics.FillPolygon(gridBrush, polygonPoints);
		}
	}

	/// <summary>
	/// 绘制单元格时, 单元格内容为多行时添加标记
	/// </summary>
	private void OnDataGridView_CellPainting_Multiline(DataGridViewCell cell, DataGridViewCellPaintingEventArgs e)
	{
		using (Brush gridBrush = new SolidBrush(Color.Red))
		{
			Point[] polygonPoints = new Point[3];
			polygonPoints[0] = new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
			polygonPoints[1] = new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1 - GlobalData.CSV_MULTILINE_POLYGON_SIZE);
			polygonPoints[2] = new Point(e.CellBounds.Right - 1 - GlobalData.CSV_MULTILINE_POLYGON_SIZE, e.CellBounds.Bottom - 1);
			e.Graphics.FillPolygon(gridBrush, polygonPoints);
		}
	}

	/// <summary>
	/// 锁定行或列
	/// </summary>
	private void OnFrozenToolStripMenuItem_Click(object sender, EventArgs e)
	{
		bool frozen = true;
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (item == m_FrozenToolStripMenuItem)
		{
			frozen = true;
		}
		else if (item == m_UnFrozenToolStripMenuItem)
		{
			frozen = false;
		}
		for (int rowIdx = 0; rowIdx < m_DataGridView.SelectedRows.Count; rowIdx++)
		{
			m_DataGridView.SelectedRows[rowIdx].Frozen = frozen;
		}
		for (int colIdx = 0; colIdx < m_DataGridView.SelectedColumns.Count; colIdx++)
		{
			m_DataGridView.SelectedColumns[colIdx].Frozen = frozen;
		}
		UpdateGridHeader();
	}

	/// <summary>
	/// 增加列宽 || 行高
	/// </summary>
	private void OnAddCellSizeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		// 增加列宽
		if (item == m_AddColWidthToolStripMenuItem)
		{
			for (int colIdx = 0; colIdx < m_DataGridView.SelectedColumns.Count; colIdx++)
			{
				// 魔法数字：列宽增加
				m_DataGridView.SelectedColumns[colIdx].Width += 120;
			}
		}

		// 增加行高
		if (item == m_AddRowHeightToolStripMenuItem)
		{
			for (int rowIdx = 0; rowIdx < m_DataGridView.SelectedRows.Count; rowIdx++)
			{
				// 魔法数字：行高增加
				m_DataGridView.SelectedRows[rowIdx].Height += 40;
			}
		}
	}

	private void OnForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (Initialized)
		{
			if (DataChanged)
			{
				DialogResult dialogResult = MessageBox.Show(string.Format("是否保存对\"{0}\"的更改?", m_SourceFile), "提示", MessageBoxButtons.YesNoCancel);
				switch (dialogResult)
				{
					case DialogResult.Yes:
						e.Cancel = !SaveFile();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}
		}
	}

	private void OnForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (Initialized)
		{
			SaveLayout();
			Dispose();
			MainForm.Instance.OnCSVForm_FormClosed();
		}
	}

	/// <summary>
	/// 编辑批注
	/// </summary>
	private void OnEditNoteToolStripMenuItem_Click(object sender, EventArgs e)
	{

	}

	private void OnDataGridView_SelectionChanged(object sender, EventArgs e)
	{
		MainForm.Instance.UpdateCellEdit();
	}
	#endregion UIEvent	
}