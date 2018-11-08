public class GlobalData
{
	public const string VERSION = "v0.4.2";

	public static System.Windows.Forms.ToolTip DefaultTooltip = new System.Windows.Forms.ToolTip
	{
		AutoPopDelay = 0,
		InitialDelay = 1000,
		ReshowDelay = 500,
		ShowAlways = true
	};

    #region Setting
    public const string SETTING_FILE_NAME = "Setting.bin";

	public const string CSVLAYOUT_FILE_NAME = "Layout.bin";

	public const string SPECIFIC_CSVLAYOUT_FILE_NAME = "ShareLayout.bin";
	#endregion // End Setting

	public const int UNDO_MAX_COUNT = 65535;
	public const int UNDO_SAVE_COUNT = 65535;

	public const int CSV_NOTE_POLYGON_SIZE = 6;
	public const int CSV_MULTILINE_POLYGON_SIZE = 6;

	public const int SEARCHFORM_HINTLABEL_NOTSEARCHED_LIMIT_LENGHT = 20;
}