using System;

[Serializable]
public class CSVLayout
{
	public string Key;
	public int[] ColumnWidths;

	/// <summary>
	/// 小于0时不冻结
	/// </summary>
	public int FrozenColumn = -1;
	/// <summary>
	/// 小于0时不冻结
	/// </summary>
	public int FrozenRow = -1;

	public VerifierName Verifier = VerifierName.Default;

	public CSVLayout(string key)
	{
		Key = key;
	}
}