using System;

namespace Debug
{
	/// <summary>
	/// 用缩写是为了转json时减少字符
	/// </summary>
	public struct LogItem
	{
		public readonly LogLevel Level;
		/// <summary>
		/// DataTime的缩写
		/// </summary>
		public readonly DateTime DT;
		public readonly string Tag;
		public readonly string Text;
		/// <summary>
		/// StackTrace的缩写
		/// </summary>
		public readonly string ST;

		public LogItem(LogLevel level, DateTime dateTime, string tag, string text, string stackTrace)
		{
			Level = level;
			DT = dateTime;
			Tag = tag;
			Text = text;
			ST = stackTrace;
		}
	}
}
