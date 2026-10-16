namespace FileSorterByDate
{
	public static class DateTimeOffsetExtensions
	{
		public static bool IsAtMidnight(this DateTimeOffset dateTime)
		{
			return dateTime.TimeOfDay == TimeSpan.Zero;
		}
	}
}
