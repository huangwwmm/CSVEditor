using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Debug;

namespace Debug
{
	public partial class LogConsoleWindow : Form
	{
		private List<LogItem> m_AllLogItem = new List<LogItem>();
		private string m_LogFilePath = "";
		/// <summary>
		/// 日志筛选条件
		/// </summary>
		private LogFilter m_LogFilter = new LogFilter();

		private LogConsoleType m_ConsoleType;

		private bool m_ShowStackTrace = true;

		public LogConsoleWindow(LogConsoleType consoleType)
		{
			InitializeComponent();

			m_ConsoleType = consoleType;

			m_OpenButton.Visible = m_ConsoleType == LogConsoleType.File;
			m_RefreshButton.Visible = m_ConsoleType == LogConsoleType.File;
			m_ClearButton.Visible = m_ConsoleType == LogConsoleType.Console;
		}

		public void AddLogItemToAllLogItemAndListView(LogItem item)
		{
			lock (m_AllLogItem)
			{
				m_AllLogItem.Add(item);
			}

			ListViewItem listViewItem = ConvertLogItemToListViewItem(item);
			m_ListView.Items.Add(listViewItem);
			if (m_EnsureCheckBox.Checked)
			{
				listViewItem.EnsureVisible();
				listViewItem.Selected = true;
			}
		}

		private ListViewItem ConvertLogItemToListViewItem(LogItem item)
		{
			ListViewItem listViewItem = new ListViewItem(Debug.Log.ConvertLevelToSign(item.Level))
			{
				ForeColor = GetLevelColor(item.Level),
				Tag = item
			};
			listViewItem.SubItems.Add(item.DT.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss.fff"));
			listViewItem.SubItems.Add(item.Tag);
			listViewItem.SubItems.Add(FormatLogText(item.Text));
			return listViewItem;
		}

		/// <summary>
		/// 解析日志文件
		/// </summary>
		private void ParseLogFile()
		{
			// 获取文件路径
			string filePath = m_OpenFileDialog.FileName;

			// 文件不存在
			if (!File.Exists(filePath))
			{
				MessageBox.Show(string.Format("文件({0})不存在", filePath), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string[] lines = null;
			//  加载日志文件
			try
			{
				lines = File.ReadAllLines(filePath);
			}
			catch (Exception e)
			{
				MessageBox.Show(string.Format("读取文件({0})失败\n{1}", filePath, e.ToString()), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// 解析
			m_LogFilePath = filePath;
			m_AllLogItem.Clear();
			for (int iItem = 0; iItem < lines.Length; iItem++)
			{
				string iterItemStr = lines[iItem];
				try
				{
					LogItem iterItem = LitJson.JsonMapper.ToObject<LogItem>(iterItemStr);
					if (iterItem.Level != LogLevel.NotSet)
					{
						m_AllLogItem.Add(iterItem);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(iterItemStr);
					Console.WriteLine(e.ToString());
				}
			}

			UpdateListView();
		}

		private void UpdateListView()
		{
			Text = m_LogFilePath;

			m_ListView.BeginUpdate();
			m_ListView.Items.Clear();

			for (int iItem = 0; iItem < m_AllLogItem.Count; iItem++)
			{
				LogItem iterItem = m_AllLogItem[iItem];
				if (m_LogFilter.Filter(iterItem))
				{
					m_ListView.Items.Add(ConvertLogItemToListViewItem(iterItem));
				}
			}

			m_ListView.EndUpdate();
		}

		private Color GetLevelColor(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Exception:
					return Color.Red;
				case LogLevel.Error:
					return Color.Red;
				case LogLevel.Warn:
					return Color.FromArgb(232, 143, 3);
				case LogLevel.Info:
					return Color.Green;
				case LogLevel.Debug:
					return Color.Black;
				case LogLevel.Verbose:
					return Color.Gray;
				default:
					return Color.Black;
			}
		}

		private string FormatLogText(string text)
		{
			int textIndex = text.IndexOf('\n');
			if (textIndex > 0)
			{
				text = text.Substring(0, textIndex);
			}
			return text;
		}

		#region UI Event
		private void OnConsoleWindow_Shown(object sender, EventArgs e)
		{
			// 初始化LevelComboBox
			foreach (LogLevel iterLevel in Enum.GetValues(typeof(LogLevel)))
			{
				if (iterLevel == LogLevel.NotSet)
				{
					continue;
				}
				m_LevelComboBox.Items.Add(iterLevel);
				m_LevelComboBox.CheckBoxItems[m_LevelComboBox.Items.Count - 1].Checked = m_LogFilter.FilterLevel(iterLevel);
			}

			m_LevelComboBox.CheckBoxCheckedChanged += OnLevelComboBox_CheckBoxCheckedChanged;

			OnStackTraceCheckBox_CheckedChanged(null, EventArgs.Empty);
		}

		/// <summary>
		/// 打开日志文件
		/// </summary>
		private void OnOpenButton_Click(object sender, EventArgs e)
		{
			if (m_OpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				ParseLogFile();
			}
		}

		/// <summary>
		/// 重新打开日志文件
		/// </summary>
		private void OnRefreshButton_Click(object sender, EventArgs e)
		{
			ParseLogFile();
		}

		private void OnListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_LogRichTextBox.Text = "";

			if (m_ListView.SelectedIndices.Count > 0)
			{
				ListViewItem lViewItem = m_ListView.Items[m_ListView.SelectedIndices[0]];
				LogItem logItem = (LogItem)lViewItem.Tag;
				m_LogRichTextBox.AppendText(logItem.Text);
				m_LogRichTextBox.AppendText("\n\n");
				m_LogRichTextBox.SelectionColor = Color.Red;
				if (m_ShowStackTrace)
				{
					m_LogRichTextBox.AppendText("StackTrace:\n");
					m_LogRichTextBox.SelectionColor = Color.Red;
					m_LogRichTextBox.AppendText(logItem.ST);
				}
			}
		}

		private void OnFilterButton_Click(object sender, EventArgs e)
		{
			UpdateListView();
		}

		/// <summary>
		/// 修改筛选等级
		/// </summary>
		private void OnLevelComboBox_CheckBoxCheckedChanged(object sender, EventArgs e)
		{
			for (int iLevelItem = 0; iLevelItem < m_LevelComboBox.Items.Count; iLevelItem++)
			{
				LogLevel iterLevel = (LogLevel)m_LevelComboBox.Items[iLevelItem];
				bool iterChecked = m_LevelComboBox.CheckBoxItems[iLevelItem].Checked;
				m_LogFilter.SetLevel(iterLevel, iterChecked);
			}
		}

		private void OnClearButton_Click(object sender, EventArgs e)
		{
			m_AllLogItem.Clear();
			m_ListView.Items.Clear();
		}

		private void OnStackTraceCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			m_ShowStackTrace = m_StackTraceCheckBox.Checked;
		}
		#endregion
	}
}