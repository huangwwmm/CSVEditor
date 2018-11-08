using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

class DefaultVerifier : BaseVerifier
{
	public override void Verify(DataGridView dataGridView, ref List<DataGridViewConsoleForm.Message> messageList, ref bool hasError, ref bool hasWarning)
	{
		for (int iRow = 0; iRow < dataGridView.Rows.Count; iRow++)
		{
			DataGridViewRow dataRow = dataGridView.Rows[iRow];
			for (int iCol = 0; iCol < dataGridView.Columns.Count; iCol++)
			{
				string iterCellValue = (string)dataRow.Cells[iCol].Value;

				AddMessageToMessageList(VerifierUtility.Verify_TabOrCRLF(DataGridViewConsoleForm.Level.Error, iRow, iCol, iterCellValue),
					ref messageList, ref hasError, ref hasWarning);

				AddMessageToMessageList(VerifierUtility.Verify_HeadAndTailWhiteSpace(DataGridViewConsoleForm.Level.Warning, iRow, iCol, iterCellValue),
					ref messageList, ref hasError, ref hasWarning);
			}
		}
	}
}