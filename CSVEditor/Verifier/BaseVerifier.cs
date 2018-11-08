using System.Collections.Generic;
using System.Windows.Forms;

public abstract class BaseVerifier
{
	/// <summary>
	/// 校验DataGridView中的数据
	/// </summary>
	/// <param name="dataGridView"></param>
	/// <param name="messageList">要输出到控制台的信息（包含但不限于错误、警告）</param>
	/// <returns>数据错误等级</returns>
	public DataGridViewConsoleForm.Level Verify(DataGridView dataGridView, out List<DataGridViewConsoleForm.Message> messageList)
	{
		messageList = new List<DataGridViewConsoleForm.Message>();
		bool hasError = false;
		bool hasWarning = false;

		Verify(dataGridView, ref messageList, ref hasError, ref hasWarning);

		if (hasError)
		{
			return DataGridViewConsoleForm.Level.Error;
		}
		else if (hasWarning)
		{
			return DataGridViewConsoleForm.Level.Warning;
		}
		else
		{
			return DataGridViewConsoleForm.Level.Info;
		}
	}

	public abstract void Verify(DataGridView dataGridView, ref List<DataGridViewConsoleForm.Message> messageList, ref bool hasError, ref bool hasWarning);

	/// <summary>
	/// 把一条message添加到messageList
	/// </summary>
	/// <param name="message">如果为null 不做处理</param>
	protected void AddMessageToMessageList(DataGridViewConsoleForm.Message message,
		ref List<DataGridViewConsoleForm.Message> messageList,
		ref bool hasError,
		ref bool hasWarning)
	{
		if (message == null)
		{
			return;
		}
		else
		{
			switch (message.Level)
			{
				case DataGridViewConsoleForm.Level.Error:
					hasError = true;
					break;
				case DataGridViewConsoleForm.Level.Warning:
					hasWarning = true;
					break;
				case DataGridViewConsoleForm.Level.Info:
					break;
				default:
					MessageBox.Show(string.Format("意外的Message.Level:({0})", message.Level), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
					break;
			}
			messageList.Add(message);
		}
	}
}