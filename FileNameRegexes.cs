using System.Text.RegularExpressions;

namespace FileSorterByDate
{
	internal static partial class FileNameRegexes
	{
		[GeneratedRegex(@"^(?<year>20[0-2][0-9])-(?<month>0[0-9]|1[0-2])-(?<day>[0-2][0-9]|3[0-1]) (?<hour>[0-1][0-9]|2[0-3])\.(?<minute>[0-5][0-9])\.(?<second>[0-5][0-9])( HDR){0,1}")]
		public static partial Regex StandardDateAndTime();

		[GeneratedRegex(@"^(?<year>20[0-2][0-9])(?<month>0[0-9]|1[0-2])(?<day>[0-2][0-9]|3[0-1])_(?<hour>[0-1][0-9]|2[0-3])(?<minute>[0-5][0-9])(?<second>[0-5][0-9])")]
		public static partial Regex DateAndTimeSeparatedByUnderscore();
		
		[GeneratedRegex(@"WP_(?<year>20[0-2][0-9])(?<month>0[0-9]|1[0-2])(?<day>[0-2][0-9]|3[0-1])_(?<hour>[0-1][0-9]|2[0-3])_(?<minute>[0-5][0-9])_(?<second>[0-5][0-9])_(Pro|Rich|Selfie)")]
		public static partial Regex WindowsPhoneDatesWithTime();
		
		[GeneratedRegex(@"WP_(?<year>20[0-2][0-9])(?<month>0[0-9]|1[0-2])(?<day>[0-2][0-9]|3[0-1])_[0-9]{3}")]
		public static partial Regex WindowsPhoneDatesWithoutTime();
		
		[GeneratedRegex(@"Screenshot_(?<year>20[0-2][0-9])-(?<month>0[0-9]|1[0-2])-(?<day>[0-2][0-9]|3[0-1])-(?<hour>[0-1][0-9]|2[0-3])-(?<minute>[0-5][0-9])-(?<second>[0-5][0-9])")]
		public static partial Regex ScreenshotsDateAndTimeFieldsDividedByDashes();
		
		[GeneratedRegex(@"Screenshot_(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})[_-](?<hour>[0-9]{2})(?<minute>[0-9]{2})(?<second>[0-9]{2})(_[A-Za-z]+)?")]
		public static partial Regex ScreenshotsDateAndTimeDividedByDash();
		
		[GeneratedRegex(@"IMG-(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})-WA[0-9]{4}")]
		public static partial Regex WhatsappImages();

        [GeneratedRegex(@"VID-(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})-WA[0-9]{4}")]
        public static partial Regex WhatsappVideos();
		
        [GeneratedRegex(@"VID_(?<year>20[0-9]{2})(?<month>1[0-2]|0[0-9])(?<day>[0-2][0-9]|3[0-1])_(?<hour>[0-1][0-9]|2[0-4])(?<minute>[0-5][0-9])(?<second>[0-5][0-9])(_[0-9]{3,4})?")]
        public static partial Regex VideosWithDateAndTime();
    }
}
