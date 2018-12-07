namespace Debug
{
	public enum LogLevel
	{
		NotSet = -1,
		/// <summary>
		/// 冗长啰嗦的调试过程中的一些详细信息
		/// </summary>
		Verbose = 1,
		/// <summary>
		/// 调试信息
		/// </summary>
		Debug = 2,
		Info = 4,
		Warn = 8,
		Error = 16,
		Exception = 32,
	}
}
