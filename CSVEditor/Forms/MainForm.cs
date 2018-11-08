using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
	public static MainForm Instance;

	/// <summary>
	/// 当前CSV窗口
	/// </summary>
	private CSVForm m_CSVForm;

	private GotoForm m_GotoForm;
	private SearchForm m_SearchForm;

	/// <summary>
	/// 关闭csv窗口之前的事件
	/// 某些操作需要先关闭当前csv窗口后才能执行，但是关闭窗口是异步的，所以要记录关闭窗口前的操作，在窗口关闭后继续之前的操作
	/// </summary>
	private BeforeCloseCSVFormEventType m_BeforeCloseCSVFormEventType = BeforeCloseCSVFormEventType.None;
	/// <summary>
	/// 关闭CSV窗口之前想要打开的文件路径，当m_BeforeCloseCSVFormEventType == BeforeCloseCSVFormEventType.OpenFile时有效
	/// </summary>
	private string m_BeforeCloaseCSVFormOpenFilePath = null;

	public MainForm()
	{
		Instance = this;
		InitializeComponent();

		Debug.Log.Initialize(string.Format("{0}csv_editor", Path.GetTempPath()));
	}

	public CSVForm GetCSVForm()
	{
		return m_CSVForm;
	}

	public TextBox GetCellEditTextBox()
	{
		return m_CellEditTextBox;
	}

	/// <summary>
	/// 当前选中的csv是否存在且完成初始化
	/// </summary>
	/// <returns></returns>
	public bool SelCSVFormInitialized()
	{
		if (m_CSVForm == null || m_CSVForm.IsDisposed)
		{
			return false;
		}
		return m_CSVForm.Initialized;
	}

	/// <summary>
	/// 更新窗口标题
	/// 文件有改动在结尾加 *
	/// </summary>
	public void UpdateFormText()
	{
		if (SelCSVFormInitialized())
		{
			Text = m_CSVForm.Text;
		}
		else
		{
			Text = "CSVEditor";
		}
	}

	public void UpdateCellEdit()
	{
		m_CellEditPanel.Visible = false;
		m_CellEditTipPanel.Visible = false;
		m_CellEditTextBox.Validated -= OnCellEditTextBox_Validated;

		if (SelCSVFormInitialized())
		{
			if (m_CSVForm.GetDataGridView().SelectedCells.Count == 0)
			{
				m_CellEditTipPanel.Visible = true;
				m_CellEditTipLabel.Text = "当前未选中单元格";
			}
			else if (m_CSVForm.GetDataGridView().SelectedCells.Count == 1)
			{
				m_CellEditPanel.Visible = true;
				object value = m_CSVForm.GetDataGridView().SelectedCells[0].Value;
				m_CellEditTextBox.Text = ((string)value).Replace("\n", "\r\n");
				m_CellEditTextBox.Validated += OnCellEditTextBox_Validated;
			}
			else if (m_CSVForm.GetDataGridView().SelectedCells.Count > 1)
			{
				m_CellEditTipPanel.Visible = true;
				m_CellEditTipLabel.Text = "不支持编辑多个单元格";
			}
		}
		else
		{
			m_CellEditTipPanel.Visible = true;
			m_CellEditTipLabel.Text = "";
		}
	}

	/// <summary>
	/// 打开一个文件
	/// </summary>
	/// <param name="filePath">如果等于null就打开FileDialog选择文件</param>
	private void OpenFile(string filePath = null)
	{
		if (m_CSVForm != null && !m_CSVForm.IsDisposed)
		{
			m_BeforeCloseCSVFormEventType = BeforeCloseCSVFormEventType.OpenFile;
			m_BeforeCloaseCSVFormOpenFilePath = filePath;
			m_CSVForm.Close();
			return;
		}

		if (filePath == null)
		{
			if (m_OpenCSVFileDialog.ShowDialog() == DialogResult.OK
				&& m_OpenCSVFileDialog.FileNames.Length > 0)
			{
				filePath = m_OpenCSVFileDialog.FileName;
			}
			else
			{
				return;
			}
		}

		LoadFile(filePath);
	}

	/// <summary>
	/// 加载csv文件
	/// </summary>
	/// <param name="path">文件完整路径</param>
	private void LoadFile(string path)
	{
		CSVForm newCSVForm = new CSVForm(path);
		if (newCSVForm == null)
		{
			return;
		}
		newCSVForm.TopLevel = false;
		newCSVForm.Visible = true;
		newCSVForm.FormBorderStyle = FormBorderStyle.None;
		newCSVForm.Dock = DockStyle.Fill;
		newCSVForm.Show();
		m_SplitContainer.Panel1.Controls.Add(newCSVForm);
		SetCSVForm(newCSVForm);
	}

	private void SetCSVForm(CSVForm csvForm)
	{
		m_CSVForm = csvForm;
		UpdateFormText();

		// 所有CSV窗口都被关闭
		if (m_CSVForm == null)
		{
			if (m_GotoForm != null && !m_GotoForm.IsDisposed)
			{
				m_GotoForm.Close();
			}
			if (m_SearchForm != null && !m_SearchForm.IsDisposed)
			{
				m_SearchForm.Close();
			}
		}
	}

	#region Update ToolStripMenu
	private void UpdateFileToolStripMenu()
	{
		m_SaveFileToolStripMenuItem.Enabled = false;
		m_SaveToFileToolStripMenuItem.Enabled = false;
		if (!SelCSVFormInitialized())
		{
			return;
		}
		m_SaveFileToolStripMenuItem.Enabled = m_CSVForm.DataChanged;
		m_SaveToFileToolStripMenuItem.Enabled = true;
	}

	private void UpdateEditToolStripMenu()
	{
		m_GotoEditToolStripMenuItem.Enabled = false;
		m_SearchEditStripMenuItem.Enabled = false;
		m_UndoEditToolStripMenuItem.Enabled = false;
		m_RedoEditToolStripMenuItem.Enabled = false;
		m_CopyEditToolStripMenuItem.Enabled = false;
		m_CutEditToolStripMenuItem.Enabled = false;
		m_PasteEditToolStripMenuItem.Enabled = false;
		if (!SelCSVFormInitialized())
		{
			return;
		}

		m_GotoEditToolStripMenuItem.Enabled = true;
		m_SearchEditStripMenuItem.Enabled = true;

		m_CopyEditToolStripMenuItem.Enabled = m_CSVForm.EditManager.CanCopy();
		m_CutEditToolStripMenuItem.Enabled = m_CSVForm.EditManager.CanCut();
		m_PasteEditToolStripMenuItem.Enabled = m_CSVForm.EditManager.CanPaste();

		m_UndoEditToolStripMenuItem.Enabled = m_CSVForm.EditManager.CanUndo();
		m_RedoEditToolStripMenuItem.Enabled = m_CSVForm.EditManager.CanRedo();
	}

	private void UpdateLayoutToolStripMenu()
	{
		m_SaveLayoutToolStripMenuItem.Enabled = false;
		m_ApplyLayoutToolStripMenuItem.Enabled = false;
		m_ManagerLayoutToolStripMenuItem.Enabled = false;
		m_ApplyVerifierLayoutToolStripMenuItem.Enabled = false;

		string[] specificLayoutKeys = CSVLayoutManager.Instance.GetSpecificKeys();
		m_ManagerLayoutToolStripMenuItem.Enabled = specificLayoutKeys.Length > 0;

		if (SelCSVFormInitialized())
		{
			m_SaveLayoutToolStripMenuItem.Enabled = true;

			// 应用布局
			if (specificLayoutKeys.Length > 0)
			{
				m_ApplyLayoutToolStripMenuItem.Enabled = true;

				m_ApplyLayoutToolStripMenuItem.DropDownItems.Clear();
				for (int keyIdx = 0; keyIdx < specificLayoutKeys.Length; keyIdx++)
				{
					ToolStripMenuItem newToolStripMenuItem = new ToolStripMenuItem();
					newToolStripMenuItem.Name = specificLayoutKeys[keyIdx];
					newToolStripMenuItem.Text = specificLayoutKeys[keyIdx];
					newToolStripMenuItem.Click += new EventHandler(OnApplyLayoutToolStripMenuItem_Click);

					m_ApplyLayoutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem });
				}
			}

			// 应用校验固规则
			m_ApplyVerifierLayoutToolStripMenuItem.Enabled = true;
			m_ApplyVerifierLayoutToolStripMenuItem.DropDownItems.Clear();
			for (int iVerifyer = 0; iVerifyer < (int)VerifierName.Count; iVerifyer++)
			{
				VerifierInfo iterVerifierInfo = VerifierUtility.GetVerifierInfo((VerifierName)iVerifyer);
				m_ApplyVerifierLayoutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
					CreateApplyVerifierLayoutToolSpritMenuItem(iterVerifierInfo.Name, iterVerifierInfo.Display)});
			}
		}
	}

	private ToolStripMenuItem CreateApplyVerifierLayoutToolSpritMenuItem(VerifierName name, string display)
	{
		ToolStripMenuItem newToolStripMenuItem = new ToolStripMenuItem();
		newToolStripMenuItem.Tag = name;
		newToolStripMenuItem.Text = display + "(?)";
		newToolStripMenuItem.Click += new EventHandler(OnApplyVerifierLayoutToolStripMenuItem_Click);
		newToolStripMenuItem.ToolTipText = CSVEditor.Properties.Resources.ResourceManager.GetString("VerifyDescription" + name.ToString());

		return newToolStripMenuItem;
	}

	private void UpdateToolsToolStripMenu()
	{
#if DEBUG
		m_DebugToolsToolStripMenuItem.Visible = true;
		m_CSVFileAssociationToolsToolStripMenuItem.Visible = true;
#else
		m_DebugToolsToolStripMenuItem.Visible = false;
		m_CSVFileAssociationToolsToolStripMenuItem.Visible = false;
#endif

		m_MergeLocalizationToolsToolStripMenuItem.Enabled = false;
		m_RemoveAllTabAndConvertAllCRLFToLFToolsToolStripMenuItem.Enabled = false;
		m_VerifierToolsToolStripMenuItem.Enabled = false;

		if (SelCSVFormInitialized())
		{
			m_MergeLocalizationToolsToolStripMenuItem.Enabled = true;
			m_RemoveAllTabAndConvertAllCRLFToLFToolsToolStripMenuItem.Enabled = true;
			m_VerifierToolsToolStripMenuItem.Enabled = true;
		}
	}
	#endregion // End Update ToolStripMenu

	private bool OnSaveLayout(string layoutName)
	{
		if (CSVLayoutManager.Instance.ExistSpecific(layoutName))
		{
			if (MessageBox.Show(string.Format("名为\"{0}\"的布局已存在。\n是否要替换？", layoutName), "提示", MessageBoxButtons.YesNo) == DialogResult.No)
			{
				return false;
			}
		}
		m_CSVForm.SaveLayout();
		CSVLayout csvLayout = SerializeUtility.ObjectCopy(MainForm.Instance.m_CSVForm.GetLayout());
		csvLayout.Key = layoutName;
		CSVLayoutManager.Instance.AddSpecific(csvLayout);
		CSVLayoutManager.Instance.SaveSpecific();
		return true;
	}

	#region UIEvent
	public void OnCSVForm_FormClosed()
	{
		UpdateFormText();
		switch (m_BeforeCloseCSVFormEventType)
		{
			case BeforeCloseCSVFormEventType.OpenFile:
				OpenFile(m_BeforeCloaseCSVFormOpenFilePath);
				break;
			case BeforeCloseCSVFormEventType.CloseForm:
				Close();
				break;
		}
		m_BeforeCloseCSVFormEventType = BeforeCloseCSVFormEventType.None;
	}

	private void OnMainForm_Load(object sender, EventArgs e)
	{
		Setting.Save();

		m_CellEditPanel.Dock = DockStyle.Fill;
		m_CellEditTipPanel.Dock = DockStyle.Fill;

		m_MergeLocalizationToolsToolStripMenuItem.ToolTipText = CSVEditor.Properties.Resources.MergeLocalizationToolHelpText;
		m_RemoveAllTabAndConvertAllCRLFToLFToolsToolStripMenuItem.ToolTipText = CSVEditor.Properties.Resources.RemoveAllTabAndConvertAllCRLFToLFToolsHint;
		m_CSVFileAssociationToolsToolStripMenuItem.ToolTipText = CSVEditor.Properties.Resources.CSVFileAssociationToolsHint;

		SetCSVForm(null);
		UpdateCellEdit();

		Text = "CSVEditor " + GlobalData.VERSION;

		// 检查cmd传入的参数
		string[] commands = Environment.GetCommandLineArgs();
		// 关联csv文件
		if (commands != null && commands.Length > 1)
		{
			LoadFile(commands[1]);
		}
	}

	private void OnForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (m_CSVForm != null && !m_CSVForm.IsDisposed)
		{
			m_BeforeCloseCSVFormEventType = BeforeCloseCSVFormEventType.CloseForm;
			m_CSVForm.Close();
			e.Cancel = true;
		}
	}

	/// <summary>
	/// 顶层菜单打开时，更新菜单里的菜单项状态
	/// </summary>
	private void OnTopToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (item == m_FileToolStripMenuItem)
		{
			UpdateFileToolStripMenu();
		}
		else if (item == m_EditToolStripMenuItem)
		{
			UpdateEditToolStripMenu();
		}
		else if (item == m_LayoutToolStripMenuItem)
		{
			UpdateLayoutToolStripMenu();
		}
		else if (item == m_ToolsToolStripMenuItem)
		{
			UpdateToolsToolStripMenu();
		}
	}

	/// <summary>
	/// 顶层菜单关闭时，启用菜单里的所有菜单项
	/// 不启用不支持快捷键的菜单项
	/// </summary>
	private void OnTopToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (item == m_FileToolStripMenuItem)
		{
			m_SaveFileToolStripMenuItem.Enabled = true;
			m_SaveToFileToolStripMenuItem.Enabled = true;
		}
		else if (item == m_EditToolStripMenuItem)
		{
			m_GotoEditToolStripMenuItem.Enabled = true;
			m_SearchEditStripMenuItem.Enabled = true;
			m_UndoEditToolStripMenuItem.Enabled = true;
			m_RedoEditToolStripMenuItem.Enabled = true;
			m_CopyEditToolStripMenuItem.Enabled = true;
			m_CutEditToolStripMenuItem.Enabled = true;
			m_PasteEditToolStripMenuItem.Enabled = true;
		}
	}

	/// <summary>
	/// 打开文件
	/// </summary>
	private void OnOpenFileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFile();
	}

	/// <summary>
	/// 文件编辑 Undo\Redo\Copy\Cut\Paste
	/// </summary>
	private void OnEditToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!SelCSVFormInitialized())
		{
			return;
		}

		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (item == m_UndoEditToolStripMenuItem)
		{
			m_CSVForm.EditManager.Undo();
		}
		else if (item == m_RedoEditToolStripMenuItem)
		{
			m_CSVForm.EditManager.Redo();
		}
		else if (item == m_CopyEditToolStripMenuItem)
		{
			m_CSVForm.EditManager.Copy();
		}
		else if (item == m_CutEditToolStripMenuItem)
		{
			// 不需要支持Cut
			//SelCSVForm.EditManager.Cut();
		}
		else if (item == m_PasteEditToolStripMenuItem)
		{
			m_CSVForm.EditManager.Paste();
		}
	}

	/// <summary>
	/// 保存文件
	/// </summary>
	private void OnSaveFileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!SelCSVFormInitialized())
		{
			return;
		}

		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (m_CSVForm.DataChanged && item == m_SaveFileToolStripMenuItem)
		{
			m_CSVForm.SaveFile();
		}
		else if (item == m_SaveToFileToolStripMenuItem)
		{
			if (m_SaveCSVFileDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			if (m_SaveCSVFileDialog.FileNames.Length == 0)
			{
				return;
			}
			m_CSVForm.SaveFile(m_SaveCSVFileDialog.FileName);
		}
	}

	/// <summary>
	/// 打开转到窗口
	/// </summary>
	private void OnGotoEditToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!SelCSVFormInitialized())
		{
			return;
		}
		if (m_GotoForm == null || m_GotoForm.IsDisposed)
		{
			m_GotoForm = new GotoForm();
		}
		if (!m_GotoForm.Visible)
		{
			m_GotoForm.Show();
		}

		m_GotoForm.Focus();
	}

	/// <summary>
	/// 打开查找和替换窗口
	/// </summary>
	private void OnSearchEditStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!SelCSVFormInitialized())
		{
			return;
		}
		if (m_SearchForm == null || m_SearchForm.IsDisposed)
		{
			m_SearchForm = new SearchForm();
		}
		if (!m_SearchForm.Visible)
		{
			m_SearchForm.Show();
		}
		m_SearchForm.Focus();
	}

	/// <summary>
	/// 布局 Save\Manager
	/// </summary>
	private void OnLayoutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		if (item == m_SaveLayoutToolStripMenuItem)
		{
			LayoutNameForm layoutNameForm = new LayoutNameForm();
			layoutNameForm.StartPosition = FormStartPosition.CenterParent;
			layoutNameForm.Text = "保存布局";
			layoutNameForm.OnApply = OnSaveLayout;
			layoutNameForm.ShowDialog();
		}
		else if (item == m_ManagerLayoutToolStripMenuItem)
		{
			LayoutManagerForm layoutManagerForm = new LayoutManagerForm();
			layoutManagerForm.StartPosition = FormStartPosition.CenterParent;
			layoutManagerForm.ShowDialog();
		}
	}

	/// <summary>
	/// 应用布局
	/// </summary>
	private void OnApplyLayoutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		CSVLayout layout = CSVLayoutManager.Instance.LoadOrCreateSpecific(item.Name);

		CSVLayoutManager.Instance.Replace(m_CSVForm.GetLayout(), layout);
		CSVLayoutManager.Instance.Save();
		m_CSVForm.LoadLayout();
	}

	/// <summary>
	/// 应用校验规则
	/// </summary>
	private void OnApplyVerifierLayoutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender;
		m_CSVForm.GetLayout().Verifier = (VerifierName)item.Tag;
		m_CSVForm.UpdateFormText();
	}

	private void OnCellEditTextBox_Validated(object sender, EventArgs e)
	{
		if (SelCSVFormInitialized())
		{
			string value = m_CellEditTextBox.Text;
			value = value.Replace("\r\n", "\n");
			m_CSVForm.GetDataGridView().SelectedCells[0].Value = value;
		}
	}

	/// <summary>
	/// 本地化合并
	/// </summary>
	private void OnMergeLocalizationToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		DataGridView dataGridView = m_CSVForm.GetDataGridView();
		// 用Trim是因为，文件在保存时会检测头尾空格
		string firstCellValue = ((string)dataGridView.Rows[0].Cells[0].Value).Trim();
		if (dataGridView.RowCount > 0 && dataGridView.ColumnCount > 0 && firstCellValue.ToLower() == "String ID".ToLower())
		{
			MergeLocalizationForm mergeLocalizationForm = new MergeLocalizationForm();
			mergeLocalizationForm.StartPosition = FormStartPosition.CenterParent;
			mergeLocalizationForm.ShowDialog();
		}
		else
		{
			MessageBox.Show(string.Format("当前打开的表可能不是本地化表\n本地化表的第一行第一个单元格为\"String ID\"\n当前打开的表第一行第一个单元格为\"{0}\"", firstCellValue), "提示");
		}
	}

	/// <summary>
	/// 移除所有制表符并转换所有换行符
	/// </summary>
	private void OnRemoveAllTabAndConvertAllCRLFToLFToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (SelCSVFormInitialized())
		{
			if (MessageBox.Show(CSVEditor.Properties.Resources.RemoveAllTabAndConvertAllCRLFToLFToolsHint + "\n" + CSVEditor.Properties.Resources.ConfirmThisOperation
				, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				m_CSVForm.RemoveAllTabAndConvertAllCRLFToLFs();
			}
		}
	}

	private void OnVerifierToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!SelCSVFormInitialized())
		{
			return;
		}

		m_CSVForm.VerifySelfAndShowConsole("检验当前打开的表");
	}

	private void OnDebugToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void OnCSVFileAssociationToolsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show(CSVEditor.Properties.Resources.CSVFileAssociationToolsHint + "\n" + CSVEditor.Properties.Resources.ConfirmThisOperation
				, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			RegistryUtility.SetRegisterFileExtendWithThisApp(".csv", "CSVEditor.CSV", "CSVEditor的csv文件", "在CSVEditor中打开");
		}
	}

	private void OnMainForm_DragDrop(object sender, DragEventArgs e)
	{
		object data = e.Data.GetData(DataFormats.FileDrop);
		if (data != null && data is string[] files)
		{
			if (files.Length == 1)
			{
				OpenFile(files[0]);
			}
			else
			{
				MessageBox.Show("不支持拖拽多个文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}

	private void OnMainForm_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			e.Effect = DragDropEffects.Copy;
		}
	}

	#endregion // END UIEvent

	/// <summary>
	/// 关闭csv窗口前的事件
	/// </summary>
	public enum BeforeCloseCSVFormEventType
	{
		None,
		OpenFile,
		CloseForm
	}
}