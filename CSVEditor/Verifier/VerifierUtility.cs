using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public static class VerifierUtility
{
	private static VerifierInfo[] ms_VerisierInfos = new VerifierInfo[(int)VerifierName.Count]{
		new VerifierInfo(VerifierName.Default, "默认", typeof(DefaultVerifier)),
		new VerifierInfo(VerifierName.Localization, "本地化", typeof(LocalizationVerifier)),
	};

	private static string[] ms_VerifyMessagesCaption;
	private static string[] ms_VerifyMessagesText;

	public static DataGridViewConsoleForm.Level VerifyWithVerifier(VerifierName verifierName, DataGridView dataGridView, out List<DataGridViewConsoleForm.Message> messageList)
	{
		VerifierInfo verifierInfo = GetVerifierInfo(verifierName);
		BaseVerifier verifier = Activator.CreateInstance(verifierInfo.ClassType) as BaseVerifier;
		return verifier.Verify(dataGridView, out messageList);
	}

	public static VerifierInfo GetVerifierInfo(VerifierName verifierName)
	{
		return ms_VerisierInfos[(int)verifierName];
	}

	/// <summary>
	/// 非法字符("\t", "\r\n")，csv文件中就不应该有这些字符
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_TabOrCRLF(DataGridViewConsoleForm.Level level, int rowIdx, int colIdx, string cellValue)
	{
		if (cellValue.Contains('\t') || cellValue.Contains("\r\n"))
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, colIdx, VerifyType.TabOrCRLF);
			message.Text = string.Format(message.Text, cellValue);
			return message;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// 头尾空白字符
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_HeadAndTailWhiteSpace(DataGridViewConsoleForm.Level level, int rowIdx, int colIdx, string cellValue)
	{
		if (cellValue.Trim() == cellValue)
		{
			return null;
		}
		else
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, colIdx, VerifyType.HeadAndTailWhiteSpace);
			message.Text = string.Format(message.Text, cellValue);
			return message;
		}
	}

	/// <summary>
	/// 一行内，单元格值重复
	/// </summary>
	/// <param name="exclueds">排除的单元格</param>
	/// <returns>见StringUtility.CheckRepeat</returns>
	public static DataGridViewConsoleForm.Message Verify_RepeatCellInRow(DataGridViewConsoleForm.Level level, int rowIdx, DataGridViewRow dataRow, int[] exclueds)
	{
		string[] strs = new string[dataRow.Cells.Count];
		for (int iCell = 0; iCell < strs.Length; iCell++)
		{
			strs[iCell] = (string)dataRow.Cells[iCell].Value;
		}

		int[][] repeats = StringUtility.CheckRepeat(strs, exclueds);
		if (repeats == null)
		{
			return null;
		}
		else
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, -1, VerifyType.RepeatCellInRow);

			StringBuilder textSb = new StringBuilder(256);
			for (int iRepeats = 0; iRepeats < repeats.Length; iRepeats++)
			{
				int[] repeat = repeats[iRepeats];
				// 重复的列号(A,B,C……)
				for (int iRepeat = 0; iRepeat < repeat.Length; iRepeat++)
				{
					textSb.Append(ConvertUtility.NumberToLetter(repeat[iRepeat] + 1));
					if (iRepeat < repeat.Length - 1)
					{
						textSb.Append(", ");
					}
				}
				textSb.Append(string.Format("    重复的值:({0})", dataRow.Cells[repeat[0]].Value));
				textSb.AppendLine();
			}
			message.Text = string.Format(message.Text, textSb.ToString());
			return message;
		}
	}

	/// <summary>
	/// 本地化表第一行第一个单元格应为"String ID"
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_Localization_FirstCell(DataGridViewConsoleForm.Level level, int rowIdx, int colIdx, string cellValue)
	{
		if (cellValue == "String ID")
		{
			return null;
		}
		else
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, colIdx, VerifyType.Localization_FirstCell);
			message.Text = string.Format(message.Text, cellValue);
			return message;
		}
	}

	/// <summary>
	/// 本地化Key不能为空
	///		除非整行都是空
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_Localization_EmptyKey(DataGridViewConsoleForm.Level level, int rowIdx, DataGridViewRow dataRow, string key)
	{
		// 本地化key为空
		if (string.IsNullOrWhiteSpace(key))
		{
			// 如果这一行所有单元格都为空，就说明这行是分割行
			for (int colIdx = 0; colIdx < dataRow.Cells.Count; colIdx++)
			{
				string cellValue = (string)dataRow.Cells[colIdx].Value;
				if (!string.IsNullOrWhiteSpace(cellValue))
				{
					DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, 0, VerifyType.Localization_EmptyKey);
					message.Text = string.Format(message.Text, cellValue);
					return message;
				}
			}
			return null;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// 本地化Key不能包含空白字符
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_Localization_KeyWhiteSpace(DataGridViewConsoleForm.Level level, int rowIdx, string key)
	{
		// 本地化key为空
		if (key.Contains(" ") || key.Trim() != key)
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, 0, VerifyType.Localization_KeyWhiteSpace);
			message.Text = string.Format(message.Text, key);
			return message;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// 本地化单元格内容中开头包含占位符"PLACEHOLDER"
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_Localization_PlaceHolder(DataGridViewConsoleForm.Level level, int rowIdx, int colIdx, string cellValue)
	{
		if (cellValue.Trim().StartsWith("PLACEHOLDER"))
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, colIdx, VerifyType.Localization_PlaceHolder);
			message.Text = string.Format(message.Text, cellValue);
			return message;
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// key不为空的情况下，未填写文本
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_Localization_EmptyText(DataGridViewConsoleForm.Level level, int rowIdx, int colIdx, string key, string text)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		else
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, colIdx, VerifyType.Localization_EmptyText);
				return message;
			}
			else
			{
				return null;
			}
		}
	}

	/// <summary>
	/// 本地化不能包含重复Key
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_Localization_RepeatKey(DataGridViewConsoleForm.Level level, string[] keys)
	{
		List<int> excludes = new List<int>();
		// 排除"String ID"
		excludes.Add(0);
		for (int iKey = 1; iKey < keys.Length; iKey++)
		{
			string iterKey = keys[iKey];
			if (string.IsNullOrWhiteSpace(iterKey))
			{
				// 排除空Key
				excludes.Add(iKey);
			}
		}

		int[][] repeats = StringUtility.CheckRepeat(keys, excludes.ToArray());
		if (repeats == null)
		{
			return null;
		}
		else
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, -1, 0, VerifyType.Localization_RepeatKey);
			StringBuilder textSb = new StringBuilder(256);
			for (int iRepeats = 0; iRepeats < repeats.Length; iRepeats++)
			{
				int[] repeat = repeats[iRepeats];
				// 重复的行号
				for (int iRepeat = 0; iRepeat < repeat.Length; iRepeat++)
				{
					textSb.Append(repeat[iRepeat] + 1);
					if (iRepeat < repeat.Length - 1)
					{
						textSb.Append(", ");
					}
				}
				textSb.Append(string.Format("    Key:({0})", keys[repeat[0]]));
				textSb.AppendLine();
			}
			message.Text = string.Format(message.Text, textSb.ToString());
			return message;
		}
	}

	/// <summary>
	/// 本地化单元格内容去掉换行符后，一段文本重复了两次则验证失败
	/// 检测规则并不严格。例，以下内容是重复的：
	///		"ABCDABCD"
	///		"ABC\r\nD\rABCD"
	///	但以下内容不属于重复的：
	///		"ABCD ABCD"
	///		"ABCDxABCD"
	/// 单元格内容的长度小于m的不再检测范围
	///		m是硬编码写死的，因为这个校验规则并不常用
	///	Q：为什么会有这个校验规则？
	///	A：因为新来的策划，不知道怎么操作的，造成了这个错误，为了筛选出有问题的本地化内容，所以添加了这个校验规则
	/// </summary>
	public static DataGridViewConsoleForm.Message Verify_Localization_Special1(DataGridViewConsoleForm.Level level, int rowIdx, int colIdx, string key, string cellValue)
	{
		string formatedValue = cellValue.Replace("\r", "").Replace("\n", "");
		if (formatedValue.Length < 10 || formatedValue.Length % 2 != 0)
		{
			return null;
		}

		if (formatedValue.Substring(0, formatedValue.Length / 2) == formatedValue.Substring(formatedValue.Length / 2))
		{
			DataGridViewConsoleForm.Message message = CreateMessage(level, rowIdx, colIdx, VerifyType.Localization_Special1);
			message.Caption = string.Format(message.Caption, key);
			message.Text = string.Format(message.Text, cellValue);
			return message;
		}
		else
		{
			return null;
		}
	}

	#region VerifyMessage
	public static void GetVerifyMessage(VerifyType verifyType, out string messageCaption, out string messageText)
	{
		if (ms_VerifyMessagesCaption == null || ms_VerifyMessagesText == null)
		{
			ms_VerifyMessagesCaption = new string[(int)VerifyType.End];
			ms_VerifyMessagesText = new string[(int)VerifyType.End];

			Type resourcesType = typeof(CSVEditor.Properties.Resources);

			for (int iMessage = 0; iMessage < ms_VerifyMessagesCaption.Length; iMessage++)
			{
				string iterCaptionPropertyName = "VerifyMessageCaption_" + Enum.GetName(typeof(VerifyType), iMessage);
				ms_VerifyMessagesCaption[iMessage] = CSVEditor.Properties.Resources.ResourceManager.GetString(iterCaptionPropertyName);

				if (string.IsNullOrEmpty(ms_VerifyMessagesCaption[iMessage]))
				{
					MessageBox.Show(string.Format("是不是添加了新校验规则({0})后没在Resources.resx填写VerifyMessageCaption", (VerifyType)iMessage), "提示");
				}

				string iterTextPropertyName = "VerifyMessageText_" + Enum.GetName(typeof(VerifyType), iMessage);
				ms_VerifyMessagesText[iMessage] = CSVEditor.Properties.Resources.ResourceManager.GetString(iterTextPropertyName);

				if (string.IsNullOrEmpty(ms_VerifyMessagesText[iMessage]))
				{
					MessageBox.Show(string.Format("是不是添加了新校验规则({0})后没在Resources.resx填写VerifyMessageText", (VerifyType)iMessage), "提示");
				}
			}
		}

		messageCaption = ms_VerifyMessagesCaption[(int)verifyType];
		messageText = ms_VerifyMessagesText[(int)verifyType];
	}

	private static DataGridViewConsoleForm.Message CreateMessage(DataGridViewConsoleForm.Level level, int rowIdx, int colIdx, VerifyType verifyType)
	{
		GetVerifyMessage(verifyType, out string messageCaption, out string messageText);
		DataGridViewConsoleForm.Message message = new DataGridViewConsoleForm.Message
		{
			Level = level,
			Row = rowIdx,
			Column = colIdx,
			Caption = messageCaption,
			Text = messageText
		};
		return message;
	}
	#endregion
}

public struct VerifierInfo
{
	public VerifierInfo(VerifierName name, string display, Type classType)
	{
		Name = name;
		Display = display;
		ClassType = classType;
	}

	public VerifierName Name;
	public string Display;
	public Type ClassType;
}

/// <summary>
/// 不能修改(包括顺序)，只能在Count前添加新VerifierName
/// 
/// 添加VerifierName后需要修改
/// <see cref="VerifierUtility.ms_VerisierInfos"/>
/// </summary>
public enum VerifierName
{
	/// <summary>
	/// 用于遍历，第一项必须为0
	/// 但是顺序可以随意更改
	/// </summary>
	Default = 0,
	Localization,
	/// <summary>
	/// 用于遍历，必须放在最后
	/// </summary>
	Count,
}

/// <summary>
/// 修改VerifyType后，需要修改Resources.resx中对应的VerifyMessageCaption、VerifyMessageText
/// </summary>
public enum VerifyType
{
	/// <summary>
	/// 用于遍历，第一项必须为Begin = -1
	/// 但是顺序可以随意更改
	/// </summary>
	Begin = -1,
	TabOrCRLF,
	HeadAndTailWhiteSpace,
	RepeatCellInRow,
	Localization_FirstCell,
	Localization_EmptyKey,
	Localization_KeyWhiteSpace,
	Localization_RepeatKey,
	Localization_PlaceHolder,
	Localization_EmptyText,
	Localization_Special1,
	/// <summary>
	/// 用于遍历，必须放在最后
	/// </summary>
	End
}