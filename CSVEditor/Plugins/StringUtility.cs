using System.Collections.Generic;

public class StringUtility
{
	public static int[][] CheckRepeat(string[] strs)
	{
		return CheckRepeat(strs, new int[0]);
	}

	/// <summary>
	/// 检测一组数据有没有重复
	/// UNDONE 添加简体中文和繁体中文的判断
	/// UNDONE 可以参考OS中用的第三方插件WordFilter中先按长度排序再查重。现在查重的速度可以接受，暂时不做
	/// </summary>
	/// <param name="strs">数据</param>
	/// <param name="excludes">不检查的索引</param>
	/// <returns>重复的数据索引，为null代表没有重复</returns>
	public static int[][] CheckRepeat(string[] strs, int[] excludes)
	{
		// 检测过为true
		bool[] checks = new bool[strs.Length];
		for (int iCheck = 0; iCheck < checks.Length; iCheck++)
		{
			checks[iCheck] = false;
		}
		// 排除
		for (int iExclude = 0; iExclude < excludes.Length; iExclude++)
		{
			int iterExclude = excludes[iExclude];
			if (iterExclude >= checks.Length)
			{
				continue;
			}
			checks[iterExclude] = true;
		}

		// 检测重复
		List<List<int>> allRepeatList = new List<List<int>>();
		List<int> repeatList = new List<int>();
		for (int iRaw = 0; iRaw < strs.Length - 1; iRaw++)
		{
			if (checks[iRaw])
			{
				continue;
			}
			checks[iRaw] = true;

			repeatList = new List<int>();
			repeatList.Add(iRaw);
			string iterRaw = strs[iRaw];
			bool isBlankLine = true;
			for (int iCompare = iRaw + 1; iCompare < strs.Length; iCompare++)
			{
				if (checks[iCompare])
				{
					continue;
				}

				string iterCompare = strs[iCompare];
				if (!string.IsNullOrWhiteSpace(iterCompare))
				{
					isBlankLine = false;
				}

				if (iterRaw == iterCompare)
				{
					checks[iCompare] = true;
					repeatList.Add(iCompare);
				}
			}

			// 有重复
			if (repeatList.Count > 1 && !isBlankLine)
			{
				allRepeatList.Add(repeatList);
			}
		}

		// 检测结果 -> int[][]
		if (allRepeatList.Count < 1)
		{
			return null;
		}
		else
		{
			int[][] reslut = new int[allRepeatList.Count][];
			for (int iAllRepeat = 0; iAllRepeat < allRepeatList.Count; iAllRepeat++)
			{
				repeatList = allRepeatList[iAllRepeat];
				reslut[iAllRepeat] = new int[repeatList.Count];
				for (int iRepeat = 0; iRepeat < repeatList.Count; iRepeat++)
				{
					reslut[iAllRepeat][iRepeat] = repeatList[iRepeat];
				}
			}

			return reslut;
		}
	}
}