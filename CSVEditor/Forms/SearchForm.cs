using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static CSVEditManager;

/// <summary>
/// 搜索窗口
/// TODO 选定范围的查找替换实现难度高，优先级低，先不做
/// </summary>
public partial class SearchForm : Form
{
    public bool Initialized = false;

    private static bool ms_IsWildcard = false;
    private static bool ms_IsCase = false;
    private static string ms_SearchText = "";
    private static string ms_ReplaceText = "";

    public SearchForm()
    {
        InitializeComponent();

        m_WildcardCheckBox.Checked = ms_IsWildcard;
        m_CaseCheckBox.Checked = ms_IsCase;
        m_SearchTextBox.Text = ms_SearchText;
        m_ReplaceTextBox.Text = ms_ReplaceText;

        // TEMP 未实现不区分大小写的替换
        m_ReplaceButton.Enabled = ms_IsCase;
        m_ReplaceAllButton.Enabled = ms_IsCase;

		ClearHintLabelText();
		Initialized = true;
	}

	/// <summary>
	/// 从起始位置的下一个位置开始搜索，搜索到文件结尾后从文件开始位置搜索到起始位置
	/// startRow,startCol 必须是dataGridView里存在的Cell，否则会死循环
	/// </summary>
	/// <param name="startRow">起始行</param>
	/// <param name="startCol">起始列</param>
	/// <returns>搜索到的Cell null是没搜索到</returns>
	public DataGridViewCell Searching(DataGridView dataGridView, int startRow, int startCol)
    {
        int currentRow = startRow;
        int currentCol = startCol;
        while (true)
        {
            currentCol++;
            if (currentCol >= dataGridView.ColumnCount)
            {
                currentCol = 0;
                currentRow++;
                if (currentRow >= dataGridView.RowCount)
                {
                    currentCol = 0;
                    currentRow = 0;
                }
            }
            // 没有找到
            if (currentRow == startRow && currentCol == startCol)
            {
                return null;
            }
            DataGridViewCell cell = dataGridView.Rows[currentRow].Cells[currentCol];
            if (Matching((string)cell.Value))
            {
                return cell;
            }
        }
    }

    /// <summary>
    /// 用当前的选项匹配
    /// </summary>
    /// <param name="target">被匹配的字符串</param>
    /// <returns>是否匹配</returns>
    public bool Matching(string target)
    {
        if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(ms_SearchText))
        {
            return false;
        }

		string searchText = ms_SearchText;

		// 不匹配大小写
		if (!ms_IsCase)
        {
            target = target.ToUpper();
			searchText = ms_SearchText.ToUpper();
        }

        if (ms_IsWildcard)
        {
            return ConvertUtility.WildcardToRegex(searchText).IsMatch(target);
        }
        else
        {
            return target.Contains(searchText);
        }
    }

    /// <summary>
    /// 替换
    /// </summary>
    /// <param name="target">目标字符串</param>
    /// <returns>替换结果</returns>
    public string Replacing(string target)
    {
        if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(ms_SearchText) || ms_ReplaceText == null)
        {
            return target;
        }
        if (ms_IsWildcard)
        {
            return ConvertUtility.WildcardToRegex(ms_SearchText).Replace(target, ms_ReplaceText);
        }
        else
        {
            return target.Replace(ms_SearchText, ms_ReplaceText);
        }
    }

	private void ClearHintLabelText()
	{
		m_HintRichTextBox.Text = "";
	}

	/// <summary>
	/// 没有搜索到searchText时的提示
	/// </summary>
	private void SetHintLabelText_NotSearched(string searchText)
	{
		m_HintRichTextBox.Text = "";

		string formatSearchText = searchText;
		// 限制长度
		if ( formatSearchText.Length > GlobalData.SEARCHFORM_HINTLABEL_NOTSEARCHED_LIMIT_LENGHT)
		{
			formatSearchText = formatSearchText.Substring(0, GlobalData.SEARCHFORM_HINTLABEL_NOTSEARCHED_LIMIT_LENGHT);
			formatSearchText += "...";
		}

		// 转义特殊符号
		formatSearchText = formatSearchText.Replace("\r", "\\r");
		formatSearchText = formatSearchText.Replace("\n", "\\n");
		formatSearchText = formatSearchText.Replace("\t", "\\t");

		m_HintRichTextBox.SelectionColor = System.Drawing.Color.Black;
		m_HintRichTextBox.AppendText("未找到指定文本\"");
		m_HintRichTextBox.SelectionColor = System.Drawing.Color.Red;
		m_HintRichTextBox.AppendText(formatSearchText);
		m_HintRichTextBox.SelectionColor = System.Drawing.Color.Black;
		m_HintRichTextBox.AppendText("\"");
	}

	#region UIEvent
	private void OnSearchNextButton_Click(object sender, EventArgs e)
    {
		ClearHintLabelText();

		if (!MainForm.Instance.SelCSVFormInitialized())
		{
			MessageBox.Show("当前没有打开CSV文件", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataGridView dataGridView = MainForm.Instance.GetCSVForm().GetDataGridView();

        int startRow = 0;
        int startCol = 0;
        if (dataGridView.CurrentCell != null)
        {
            startRow = dataGridView.CurrentCell.RowIndex;
            startCol = dataGridView.CurrentCell.ColumnIndex;
        }

        DataGridViewCell cell = Searching(dataGridView, startRow, startCol);
        if (cell == null)
		{
			SetHintLabelText_NotSearched(ms_SearchText);
		}
		else
        {
            dataGridView.ClearSelection();
            dataGridView.CurrentCell = cell;
        }
    }

    private void OnReplaceButton_Click(object sender, EventArgs e)
    {
		ClearHintLabelText();

		if (!MainForm.Instance.SelCSVFormInitialized())
        {
            MessageBox.Show("当前没有打开CSV文件", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataGridView dataGridView = MainForm.Instance.GetCSVForm().GetDataGridView();

        // 当前单元格
        if (dataGridView.CurrentCell != null && Matching((string)dataGridView.CurrentCell.Value))
        {
            string oldValue = (string)dataGridView.CurrentCell.Value;
            string newValue = Replacing(oldValue);
            MainForm.Instance.GetCSVForm().EditManager.DidCellValueChange(dataGridView.CurrentCell.ColumnIndex,
               dataGridView.CurrentCell.RowIndex,
               oldValue,
               newValue);

            MainForm.Instance.GetCSVForm().BeforeChangeCellValue();
            dataGridView.CurrentCell.Value = newValue;
            MainForm.Instance.GetCSVForm().AfterChangeCellValue();
            return;
        }

        int startRow = 0;
        int startCol = 0;
        if (dataGridView.CurrentCell != null)
        {
            startRow = dataGridView.CurrentCell.RowIndex;
            startCol = dataGridView.CurrentCell.ColumnIndex;
        }

        MainForm.Instance.GetCSVForm().BeforeChangeCellValue();
        DataGridViewCell cell = Searching(dataGridView, startRow, startCol);
        if (cell == null)
		{
			SetHintLabelText_NotSearched(ms_SearchText);
		}
		else
        {
            string oldValue = (string)cell.Value;
            string newValue = Replacing(oldValue);
            MainForm.Instance.GetCSVForm().EditManager.DidCellValueChange(cell.ColumnIndex, cell.RowIndex, oldValue, newValue);

            cell.Value = Replacing(oldValue);
            dataGridView.ClearSelection();
            dataGridView.CurrentCell = cell;
        }
        MainForm.Instance.GetCSVForm().AfterChangeCellValue();
    }

    private void OnReplaceAllButton_Click(object sender, EventArgs e)
    {
		ClearHintLabelText();

		if (!MainForm.Instance.SelCSVFormInitialized())
        {
            MessageBox.Show("当前没有打开CSV文件", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataGridView dataGridView = MainForm.Instance.GetCSVForm().GetDataGridView();

        MainForm.Instance.GetCSVForm().BeforeChangeCellValue();
        List<CellValueChangeItem> ChangeList = new List<CellValueChangeItem>();
        DataGridViewCell cell = Searching(dataGridView, 0, 0);
		int replaceCount = 0;
        while(cell != null)
        {
			replaceCount++;
			CellValueChangeItem changeItem = new CellValueChangeItem();
            changeItem.OldValue = (string)cell.Value;
            changeItem.NewValue = Replacing(changeItem.OldValue);
            changeItem.Row = cell.RowIndex;
            changeItem.Column = cell.ColumnIndex;
            ChangeList.Add(changeItem);

            cell.Value = changeItem.NewValue;
            cell = Searching(dataGridView, cell.RowIndex, cell.ColumnIndex);
        }
        MainForm.Instance.GetCSVForm().EditManager.DidCellsValueChange(ChangeList);
        MainForm.Instance.GetCSVForm().AfterChangeCellValue();

		MessageBox.Show(string.Format("完成{0}处替换", replaceCount));
	}

    private void OnValueChanged(object sender, EventArgs e)
    {
        if (!Initialized)
        {
            return;
        }
        ms_IsWildcard = m_WildcardCheckBox.Checked;
        ms_IsCase = m_CaseCheckBox.Checked;
        ms_SearchText = m_SearchTextBox.Text;
		ms_SearchText.Replace("\r\n", "\n");
		ms_ReplaceText = m_ReplaceTextBox.Text;
		ms_ReplaceText.Replace("\r\n", "\n");

		// TEMP 未实现不区分大小写的替换
		m_ReplaceButton.Enabled = ms_IsCase;
        m_ReplaceAllButton.Enabled = ms_IsCase;
    }

	private void OnSearchForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
	{
		MessageBox.Show(CSVEditor.Properties.Resources.SearchFormWildcardHelpText,
			"帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);

		e.Cancel = true;
	}
	#endregion // END UIEvent
}