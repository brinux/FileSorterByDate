using System.Text.RegularExpressions;

namespace FileSorterByDate
{
	internal static class FileNameTimestampParser
	{
		public static bool TryParse(Match match, TimeZoneInfo defaultTimeZone, out DateTimeOffset timestamp)
		{
			timestamp = default;

			if (!match.Success ||
				!TryGetComponent(match, "year", out var year) ||
				!TryGetComponent(match, "month", out var month) ||
				!TryGetComponent(match, "day", out var day))
			{
				return false;
			}

			TryGetComponent(match, "hour", out var hour);
			TryGetComponent(match, "minute", out var minute);
			TryGetComponent(match, "second", out var second);

			try
			{
				var local = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
				timestamp = TimeZoneSettings.Attach(local, defaultTimeZone);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				return false;
			}
		}

		private static bool TryGetComponent(Match match, string name, out int value)
		{
			value = 0;
			var group = match.Groups[name];
			return group.Success && int.TryParse(group.Value, out value);
		}
	}
}
