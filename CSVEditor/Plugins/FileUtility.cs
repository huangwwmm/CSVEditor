using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;

public class FileUtility
{
	public static string[][] LoadFileToCSV(string path)
	{
		string fileText;
		try
		{
			fileText = File.ReadAllText(path, Encoding.UTF8);
		}
		catch (Exception ex)
		{
			DebugUtility.ShowExceptionMessageBox("读取文件失败:" + path, ex);
			return null;
		}

		// 检测文件编码
		using (FileStream fs = File.OpenRead(path))
		{
			Ude.CharsetDetector cdet = new Ude.CharsetDetector();
			cdet.Feed(fs);
			cdet.DataEnd();
			if (cdet.Charset != null)
			{
				if (cdet.Charset != "UTF-8")
				{
					MessageBox.Show(string.Format(CSVEditor.Properties.Resources.CharsetDetectorNotUTF8Hint, path, cdet.Charset),
						"提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				Console.WriteLine("Charset: {0}, confidence: {1}",
					cdet.Charset, cdet.Confidence);
			}
			else
			{
				MessageBox.Show(string.Format(CSVEditor.Properties.Resources.CharsetDetectorFailedHint, path),
					"提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		// 读取文件 -> csv
		string[][] csvTable;
		try
		{
			csvTable = CSVParser.Parse(fileText);
			return csvTable;
		}
		catch (Exception ex)
		{
			DebugUtility.ShowExceptionMessageBox("转csv失败:" + path, ex);
			return null;
		}
	}

	public static bool FilesAreEqual_Hash(string path1, string path2)
	{
		if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2))
		{
			return false;
		}

		if (!File.Exists(path1) || !File.Exists(path2))
		{
			return false;
		}

		try
		{
			MD5 md5 = MD5.Create();
			using (FileStream fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read))
			using (FileStream fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read))
			{
				byte[] hash1 = md5.ComputeHash(fs1);
				byte[] hash2 = md5.ComputeHash(fs2);

				for (int byteIdx = 0; byteIdx < hash1.Length; byteIdx++)
				{
					if (hash1[byteIdx] != hash2[byteIdx])
					{
						return false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			DebugUtility.ShowExceptionMessageBox(string.Format("对比\n文件:({0})\n文件:({1})\n内容失败", path1, path2), ex);
			return false;
		}

		return true;
	}

	public static string GetApplicationDirectory()
	{
		// HACK 这个程序最开始的命名是CsvEditor，后来改为CSVEditor。但是不知道为什么ApplicationBase中仍然是"CsvEditor"
		// 没时间细查原因，暂时先这样处理
		return AppDomain.CurrentDomain.SetupInformation.ApplicationBase.Replace("CsvEditor", "CSVEditor");
	}
}