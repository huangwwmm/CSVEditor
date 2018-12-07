using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Debug
{
	/// <summary>
	/// 使用之前先初始化(Initialize) 并注册OnApplicationExit
	/// </summary>
	public static class Log
	{
		public static bool Initialized = false;

		public static EventHandler LogItemHandler;
		/// <summary>
		/// 是否输出Log到文件
		/// </summary>
		public static bool LogToFile = true;
		/// <summary>
		/// 是否输出调用栈到文件
		/// </summary>
		public static bool LogStackTraceToFile = true;

		/// <summary>
		/// 是否打印Log到控制台
		/// </summary>
		public static bool LogToConsole = true;
		/// <summary>
		/// 是否输出调用栈到控制台
		/// </summary>
		public static bool LogStackTraceToConsole = true;

		/// <summary>
		/// 打印到控制台的等级 按位
		/// </summary>
		public static int LogToConsoleLevel = 0 |
			(int)LogLevel.Verbose |
			(int)LogLevel.Info |
			(int)LogLevel.Debug |
			(int)LogLevel.Warn |
			(int)LogLevel.Error |
			(int)LogLevel.Exception;

		private static string ms_LogPath;

		private static FileStream ms_FS;
		private static StreamWriter ms_SW;

		public static void Initialize(string logDirectory)
		{
			// 如果路径不存在则创建
			if (!Directory.Exists(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}

			ms_LogPath = string.Format("{0}{1}.log", logDirectory, DateTime.UtcNow.ToString("yyyy-dd-M--HH-mm-ss"));
			if (File.Exists(ms_LogPath))
			{
				File.Delete(ms_LogPath);
			}

			ms_FS = new FileStream(ms_LogPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
			ms_SW = new StreamWriter(ms_FS);

			Initialized = true;
		}

		public static void OnApplicationExit(object sender, EventArgs e)
		{
			if (ms_SW != null)
			{
				ms_SW.Flush();
				ms_SW.Close();
			}
			if (ms_FS != null)
			{
				ms_FS.Close();
			}
		}

		#region Log Mothed 这些函数因为经常用，用缩写比较方便(参考Android的Log类)
		public static void V(string text)
		{
			V("", text);
		}
		public static void V(string tag, string text)
		{
			Loging(LogLevel.Verbose, tag, text);
		}

		public static void D(string text)
		{
			D("", text);
		}
		public static void D(string tag, string text)
		{
			Loging(LogLevel.Debug, tag, text);
		}

		public static void I(string text)
		{
			I("", text);
		}
		public static void I(string tag, string text)
		{
			Loging(LogLevel.Info, tag, text);
		}

		public static void W(string text)
		{
			W("", text);
		}
		public static void W(string tag, string text)
		{
			Loging(LogLevel.Warn, tag, text);
		}

		public static void E(string text)
		{
			E("", text);
		}
		public static void E(string tag, string text)
		{
			Loging(LogLevel.Error, tag, text);
		}

		public static void EX(Exception exception)
		{
			EX("", exception);
		}

		public static void EX(string text, Exception exception)
		{
			EX("", text, exception);
		}

		public static void EX(string tag, string text, Exception exception)
		{
			Loging(LogLevel.Exception, tag, string.Format("{0}\n    Message:\n        {1}", text, exception.Message), exception.StackTrace);
		}
		#endregion

		public static string ConvertLevelToSign(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Debug:
					return "D";
				case LogLevel.Error:
					return "E";
				case LogLevel.Exception:
					return "EX";
				case LogLevel.Info:
					return "I";
				case LogLevel.Verbose:
					return "V";
				case LogLevel.Warn:
					return "W";
				default:
					return level.ToString();
			}
		}

		private static void Loging(LogLevel level, string tag, string text)
		{
			string stackTrace = "";
			if (LogStackTraceToConsole || LogStackTraceToFile)
			{
				stackTrace = GenerateStackTraceString(3);
			}
			Loging(level, tag, text, stackTrace);
		}

		private static void Loging(LogLevel level, string tag, string text, string stackTrace)
		{
			bool isExLevel = level == LogLevel.Exception;

			LogItem item = new LogItem(level, DateTime.UtcNow, tag, text, isExLevel || LogStackTraceToFile ? stackTrace : "");
			LogItemHandler?.Invoke(item, EventArgs.Empty);
			if (LogToFile)
			{
				bool oldPrettryPrint = LitJson.JsonMapper.GetStaticJsonWriter().PrettyPrint;
				LitJson.JsonMapper.GetStaticJsonWriter().PrettyPrint = false;
				string jsonStr = LitJson.JsonMapper.ToJson(item);
				LitJson.JsonMapper.GetStaticJsonWriter().PrettyPrint = oldPrettryPrint;
				ms_SW.WriteLine(jsonStr);
			}

			if (LogToConsole)
			{
				ConsoleWriteLine(level, tag, text, isExLevel || LogStackTraceToConsole ? stackTrace : "");
			}
		}

		private static void ConsoleWriteLine(LogLevel level, string tag, string text, string stackTrace)
		{
			if (((int)level & LogToConsoleLevel) != 0)
			{
				Console.WriteLine("");
				if (string.IsNullOrEmpty(stackTrace))
				{
					Console.WriteLine("{0}|{1}:{2}", Enum.GetName(typeof(LogLevel), level), tag, text);
				}
				else
				{
					Console.WriteLine("{0}|{1}:{2}\n    StackTrace:\n{3}", Enum.GetName(typeof(LogLevel), level), tag, text, stackTrace);
				}
			}
		}

		/// <summary>
		/// 生成调用栈信息
		/// </summary>
		/// <param name="startStackFrameIndex">调用栈起始位置</param>
		/// <param name="endStackFrameIndex">调用栈结束位置, 小于等于0表示栈底</param>
		private static string GenerateStackTraceString(int startStackFrameIndex = 1, int endStackFrameIndex = 0)
		{
			//设置为true，这样才能捕获到文件路径名和当前行数，当前行数为GetFrames代码的函数，也可以设置其他参数
			StackTrace stackTrace = new StackTrace(true);
			//得到当前的所以堆栈
			StackFrame[] stackFrames = stackTrace.GetFrames();

			StringBuilder stackTraceString = new StringBuilder(512);

			int startIndex = Math.Max(0, startStackFrameIndex);
			int endIndex = endStackFrameIndex > 0 ? Math.Max(endStackFrameIndex, stackFrames.Length) : stackFrames.Length;
			for (int iFrame = startStackFrameIndex; iFrame < endIndex; ++iFrame)
			{
				StackFrame iterFrame = stackFrames[iFrame];

				stackTraceString.Append(string.Format("        在 {0}.{1}(",
					iterFrame.GetMethod().DeclaringType.FullName,
					iterFrame.GetMethod().Name));

				ParameterInfo[] iterParameters = iterFrame.GetMethod().GetParameters();
				for (int iParameter = 0; iParameter < iterParameters.Length; iParameter++)
				{
					ParameterInfo iterParameter = iterParameters[iParameter];
					stackTraceString.Append(iterParameter.ToString());
					if (iParameter < iterParameters.Length - 1)
					{
						stackTraceString.Append(", ");
					}
				}

				string stackFileName = iterFrame.GetFileName();
				if (string.IsNullOrWhiteSpace(stackFileName))
				{
					stackTraceString.Append(")\n");
				}
				else
				{
					stackTraceString.Append(string.Format(") 在 {0} 中: 第 {1} 行\n",
							stackFileName,
							iterFrame.GetFileLineNumber()));
				}
			}
			return stackTraceString.ToString();
		}
	}
}