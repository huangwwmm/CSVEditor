using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public partial class DataGridViewConsoleForm : Form
{
	private List<Message> m_Messages;
	/// <summary>
	/// ListView中的index对应Messages中的index
	/// </summary>
	private List<int> m_ListViewItemIndexToMessagesIndex;
	private int m_ListViewSelectedIndex = -1;

	private DataGridView m_DataGridView;

	/// <summary>
	/// 测试功能，暂时隐藏
	/// </summary>
	private bool m_Collapse = false;
	private string m_LastCaption = "";
	private int m_CollapseCount = 0;

	/// <summary>
	/// 显示控制台
	/// </summary>
	/// <param name="messages">控制台内显示的消息</param>
	/// <param name="dataGridView">消息对应的DataGridView控件，用于点击MessageItem跳转到DataGridView</param>
	/// <param name="operateName">操作的名字，用于显示窗口标题</param>
	public static void ShowForm(List<Message> messages, DataGridView dataGridView, string operateName)
	{
		if (messages == null || messages.Count < 1)
		{
			return;
		}
		else
		{
			messages.Sort(SortComparison);
			DataGridViewConsoleForm form = new DataGridViewConsoleForm(messages, dataGridView);
			form.Text = string.Format("控制台 - 操作({0}) - 时间({1})", operateName, DateTime.Now);
			form.Show();
		}
	}

	private static int SortComparison(Message x, Message y)
	{
		if (y.Level != x.Level)
		{
			return (int)y.Level - (int)x.Level;
		}
		else
		{
			if (x.Row != y.Row)
			{
				return x.Row - y.Row;
			}
			else
			{
				return x.Column - y.Column;
			}
		}
	}

	private DataGridViewConsoleForm(List<Message> messageList, DataGridView dataGridView)
	{
		InitializeComponent();

		m_Messages = messageList;
		m_DataGridView = dataGridView;
	}

	private void UpdateListView()
	{
		m_MessageListView.BeginUpdate();

		m_Collapse = m_CollapseCheckBox.Checked;

		// 当前选中的消息的索引
		int messageSelectedIndex = 0;
		// 需要选中ListView的索引
		int selectIndex = 0;
		if (m_ListViewSelectedIndex >= 0 && m_ListViewSelectedIndex < m_ListViewItemIndexToMessagesIndex.Count)
		{
			messageSelectedIndex = m_ListViewItemIndexToMessagesIndex[m_ListViewSelectedIndex];
		}

		m_MessageListView.Items.Clear();
		m_ListViewItemIndexToMessagesIndex = new List<int>();

		m_LastCaption = "";
		m_CollapseCount = 0;

		int infoCount = 0;
		int warningCount = 0;
		int errorCount = 0;

		bool info = m_InfoCheckBox.Checked;
		bool warning = m_WarningCheckBox.Checked;
		bool error = m_ErrorCheckBox.Checked;

		for (int msgIdx = 0; msgIdx < m_Messages.Count; msgIdx++)
		{
			Message iterMessage = m_Messages[msgIdx];

			switch (iterMessage.Level)
			{
				case Level.Info:
					infoCount++;
					if (info)
					{
						AddMessageToListView(iterMessage, msgIdx);
					}
					break;
				case Level.Warning:
					warningCount++;
					if (warning)
					{
						AddMessageToListView(iterMessage, msgIdx);
					}
					break;
				case Level.Error:
					errorCount++;
					if (error)
					{
						AddMessageToListView(iterMessage, msgIdx);
					}
					break;
			}
			if (messageSelectedIndex == msgIdx)
			{
				selectIndex = m_MessageListView.Items.Count - 1;
			}
		}

		AddCollapseToLastItem();

		if (selectIndex >= 0 && selectIndex < m_MessageListView.Items.Count)
		{
			m_MessageListView.Items[selectIndex].Selected = true;
		}

		m_InfoCheckBox.Text = string.Format("{0} {1}", LevelToString(Level.Info), infoCount);
		m_WarningCheckBox.Text = string.Format("{0} {1}", LevelToString(Level.Warning), warningCount);
		m_ErrorCheckBox.Text = string.Format("{0} {1}", LevelToString(Level.Error), errorCount);

		m_MessageListView.EndUpdate();
	}

	private void AddMessageToListView(Message message, int index)
	{
		if (m_Collapse)
		{
			if (m_LastCaption == message.Caption)
			{
				m_CollapseCount++;
				return;
			}
			AddCollapseToLastItem();
			m_LastCaption = message.Caption;
			m_CollapseCount = 0;
		}

		string item = FormatMessageCaption(message);
		ListViewItem newItem = new ListViewItem(LevelToString(message.Level));
		newItem.SubItems.Add(FormatMessagePosition(message));
		newItem.SubItems.Add(message.Caption);
		newItem.SubItems.Add("1");
		m_MessageListView.Items.Add(newItem);
		m_ListViewItemIndexToMessagesIndex.Add(index);
	}

	private string FormatMessagePosition(Message message)
	{
		string rowColumn = "";
		if (message.Row < 0 && message.Column < 0)
		{
			rowColumn = "na,na";
		}
		else if (message.Row < 0)
		{
			rowColumn = string.Format("na,{0}", ConvertUtility.NumberToLetter(message.Column + 1));
		}
		else if (message.Column < 0)
		{
			rowColumn = string.Format("{0},na", message.Row + 1);
		}
		else
		{
			rowColumn = string.Format("{0},{1}", message.Row + 1, ConvertUtility.NumberToLetter(message.Column + 1));
		}
		return rowColumn;
	}

	private string FormatMessageCaption(Message message)
	{
		string rowColumn = FormatMessagePosition(message);
		return string.Format("{0,-4}{1,-12}\t{2}",
			LevelToString(message.Level),
			rowColumn,
			message.Caption);
	}

	private string LevelToString(Level level)
	{
		if (level == Level.Info)
		{
			return "信息";
		}
		else if (level == Level.Warning)
		{
			return "警告";
		}
		else if (level == Level.Error)
		{
			return "错误";
		}
		else
		{
			return "未知";
		}
	}

	/// <summary>
	/// 获取ListView中的Message
	/// </summary>
	private Message GetMessageInListView(int listViewItemIndex)
	{
		if (listViewItemIndex >= 0 && listViewItemIndex < m_ListViewItemIndexToMessagesIndex.Count)
		{
			int messageIdx = m_ListViewItemIndexToMessagesIndex[listViewItemIndex];
			if (messageIdx >= 0 && messageIdx < m_Messages.Count)
			{
				Message message = m_Messages[messageIdx];
				return message;
			}
		}
		return null;
	}

	/// <summary>
	/// 添加Collapse到上一次条消息
	/// </summary>
	private void AddCollapseToLastItem()
	{
		if (m_CollapseCount > 0)
		{
			ListViewItem lastItem = m_MessageListView.Items[m_MessageListView.Items.Count - 1];
			// 魔法数字3:"折叠"
			lastItem.SubItems[3].Text = (m_CollapseCount + 1).ToString();
		}
	}

	#region UIEvent
	private void OnDataGridViewConsoleForm_Load(object sender, EventArgs e)
	{
		GlobalData.DefaultTooltip.SetToolTip(m_CollapseCheckBox, "折叠说明相同的消息");

		UpdateListView();
	}

	private void OnCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		UpdateListView();
	}

	private void OnMessageListView_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_MessageListView.SelectedItems.Count > 0)
		{
			m_DetailTextBox.Text = "";
			m_ListViewSelectedIndex = m_MessageListView.SelectedItems[0].Index;

			Message message = GetMessageInListView(m_ListViewSelectedIndex);
			if (message == null)
			{
				return;
			}
			string caption = FormatMessageCaption(message);
			string text = string.Format("{0}\n{1}", caption.Replace("\t", "\n\n"), message.Text);
			m_DetailTextBox.Text = text.Replace("\n", "\r\n");
		}
	}

	private void OnMessageListView_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (m_MessageListView.SelectedItems.Count > 0)
		{
			int itemIndex = m_MessageListView.SelectedItems[0].Index;
			Message message = GetMessageInListView(itemIndex);
			if (message == null)
			{
				return;
			}

			m_DataGridView.Focus();
			DataGridViewUtility.SelectCell(m_DataGridView, message.Row, message.Column);
		}
	}
	#endregion //End UIEvent

	public class Message
	{
		public Level Level;
		public int Row;
		public int Column;
		public string Caption;
		public string Text;
	}

	/// <summary>
	/// 数字越小，表示错误等级越低
	/// </summary>
	public enum Level
	{
		Info = 0,
		Warning,
		Error
	}
}