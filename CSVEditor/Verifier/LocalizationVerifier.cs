using System.Collections.Generic;
using System.Windows.Forms;

public class LocalizationVerifier : BaseVerifier
{
	public override void Verify(DataGridView dataGridView, ref List<DataGridViewConsoleForm.Message> messageList, ref bool hasError, ref bool hasWarning)
	{
		List<string> localizationKeys = new List<string>();

		for (int iRow = 0; iRow < dataGridView.Rows.Count; iRow++)
		{
			DataGridViewRow iterDataRow = dataGridView.Rows[iRow];

			// 魔法数字：Key不需要检测。 注释列, 中文列的重复太多不检测
			AddMessageToMessageList(VerifierUtility.Verify_RepeatCellInRow(DataGridViewConsoleForm.Level.Info, iRow, iterDataRow, new int[] { 0, 1, 7, 12, 14 }),
				ref messageList, ref hasError, ref hasWarning);

			string iterKey = null;
			for (int iCol = 0; iCol < dataGridView.Columns.Count; iCol++)
			{
				string iterCellValue = (string)iterDataRow.Cells[iCol].Value;

				// 非法字符
				AddMessageToMessageList(VerifierUtility.Verify_TabOrCRLF(DataGridViewConsoleForm.Level.Error, iRow, iCol, iterCellValue),
					ref messageList, ref hasError, ref hasWarning);

				// 列头
				if (iRow == 0)
				{
					if (iCol == 0)
					{
						// 第一个单元格应为"String ID"
						AddMessageToMessageList(VerifierUtility.Verify_Localization_FirstCell(DataGridViewConsoleForm.Level.Error, iRow, iCol, iterCellValue),
							ref messageList, ref hasError, ref hasWarning);
					}
				}
				else
				{
					// 本地化Key
					if (iCol == 0)
					{
						iterKey = iterCellValue;
						localizationKeys.Add(iterKey);

						// Key不能为空
						AddMessageToMessageList(VerifierUtility.Verify_Localization_EmptyKey(DataGridViewConsoleForm.Level.Error, iRow, iterDataRow, iterKey),
							ref messageList, ref hasError, ref hasWarning);

						// Key不能包含空白字符
						// UNDONE Level应该是Error，但是现在本地化表里有些Key已经包含空格了。这些Key没人敢动
						AddMessageToMessageList(VerifierUtility.Verify_Localization_KeyWhiteSpace(DataGridViewConsoleForm.Level.Warning, iRow, iterKey),
							ref messageList, ref hasError, ref hasWarning);
					}
					// 本地化文本
					else
					{
						// UNDONE 因为消息数量太多，影响阅读，暂时注释掉
						// 头尾有空白字符
						//AddMessageToMessageList(VerifierUtility.VerifyHeadAndTailWhiteSpace(DataGridViewConsoleForm.Level.Warning, iRow, iCol, iterCellValue),
						//	ref messageList, ref hasError, ref hasWarning);

						// 文本以占位符开头
						AddMessageToMessageList(VerifierUtility.Verify_Localization_PlaceHolder(DataGridViewConsoleForm.Level.Warning, iRow, iCol, iterCellValue),
							ref messageList, ref hasError, ref hasWarning);

						AddMessageToMessageList(VerifierUtility.Verify_Localization_EmptyText(DataGridViewConsoleForm.Level.Warning, iRow, iCol, iterKey, iterCellValue),
							ref messageList, ref hasError, ref hasWarning);

						// 特殊的校验规则，详见Verify_Localization_Special1的注释
						AddMessageToMessageList(VerifierUtility.Verify_Localization_Special1(DataGridViewConsoleForm.Level.Warning, iRow, iCol, iterKey, iterCellValue),
							ref messageList, ref hasError, ref hasWarning);
					}
				}
			}
		}

		// 重复的Key
		string[] localizationKeyArray = localizationKeys.ToArray();
		AddMessageToMessageList(VerifierUtility.Verify_Localization_RepeatKey(DataGridViewConsoleForm.Level.Error, localizationKeyArray),
			ref messageList, ref hasError, ref hasWarning);
	}
}