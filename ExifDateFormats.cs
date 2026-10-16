namespace FileSorterByDate
{
	internal static class ExifDateFormats
	{
		public const string WithOffset = "yyyy:MM:dd HH:mm:ssK";
		public const string WithoutOffset = "yyyy:MM:dd HH:mm:ss";

		public static string FormatLocal(DateTimeOffset timestamp) =>
			timestamp.ToString(WithoutOffset);

		public static string FormatUtc(DateTimeOffset timestamp) =>
			timestamp.ToUniversalTime().ToString(WithoutOffset);

		public static string FormatWithOffset(DateTimeOffset timestamp) =>
			timestamp.ToString(WithoutOffset) + FormatOffset(timestamp.Offset);

		public static string FormatOffset(TimeSpan offset)
		{
			var sign = offset < TimeSpan.Zero ? "-" : "+";
			var absolute = offset.Duration();
			return $"{sign}{absolute.Hours:00}:{absolute.Minutes:00}";
		}
	}
}
