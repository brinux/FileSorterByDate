namespace FileSorterByDate
{
	internal static class TimeZoneSettings
	{
		public const string DefaultTimeZoneId = "Europe/Rome";

		public static TimeZoneInfo Resolve(string timeZoneId)
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			}
			catch (TimeZoneNotFoundException) when (IsRomeId(timeZoneId))
			{
				return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			}
		}

		public static DateTimeOffset Attach(DateTime wallClock, TimeZoneInfo timeZone)
		{
			ArgumentNullException.ThrowIfNull(timeZone);
			var unspecified = DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified);
			return new DateTimeOffset(unspecified, timeZone.GetUtcOffset(unspecified));
		}

		public static DateTimeOffset AssumeIfMissingOrZero(DateTimeOffset timestamp, TimeZoneInfo timeZone)
		{
			return timestamp.Offset == TimeSpan.Zero
				? Attach(timestamp.DateTime, timeZone)
				: timestamp;
		}

		private static bool IsRomeId(string timeZoneId) =>
			timeZoneId.Equals("Europe/Rome", StringComparison.OrdinalIgnoreCase);
	}
}
