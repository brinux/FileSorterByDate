using System.Globalization;
using System.Text.RegularExpressions;

namespace FileSorterByDate
{
	internal static partial class ExifDateTimeParser
	{
		private static readonly string[] FormatsWithOffset =
		[
			"yyyy:MM:dd HH:mm:ssK",
			"yyyy:MM:dd HH:mm:ss.fK",
			"yyyy:MM:dd HH:mm:ss.ffK",
			"yyyy:MM:dd HH:mm:ss.fffK",
			"yyyy:MM:dd HH:mm:sszzz",
			"yyyy:MM:dd HH:mm:ss.fffzzz",
		];

		private static readonly string[] FormatsWithoutOffset =
		[
			"yyyy:MM:dd HH:mm:ss",
			"yyyy:MM:dd HH:mm:ss.f",
			"yyyy:MM:dd HH:mm:ss.ff",
			"yyyy:MM:dd HH:mm:ss.fff",
		];

		public static bool IsUnset(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return true;
			}

			var trimmed = value.Trim();
			return trimmed.StartsWith("0000:00:00", StringComparison.Ordinal);
		}

		public static bool TryParse(string? value, MetadataTimeKind kind, TimeZoneInfo defaultTimeZone, out DateTimeOffset timestamp, out bool hasExplicitOffset)
		{
			timestamp = default;
			hasExplicitOffset = false;

			if (IsUnset(value))
			{
				return false;
			}

			var normalized = Normalize(value!);

			if (ContainsOffset(normalized) &&
				DateTimeOffset.TryParseExact(
					normalized,
					FormatsWithOffset,
					CultureInfo.InvariantCulture,
					DateTimeStyles.AllowWhiteSpaces,
					out timestamp))
			{
				hasExplicitOffset = true;
				return true;
			}

			if (DateTime.TryParseExact(
				normalized,
				FormatsWithoutOffset,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.None,
				out var dateTime))
			{
				hasExplicitOffset = false;
				timestamp = kind == MetadataTimeKind.Utc
					? new DateTimeOffset(dateTime, TimeSpan.Zero)
					: TimeZoneSettings.Attach(dateTime, defaultTimeZone);
				return true;
			}

			return false;
		}

		public static bool TryParseOffset(string? value, out TimeSpan offset)
		{
			offset = default;
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			var trimmed = value.Trim();
			if (trimmed.Equals("Z", StringComparison.OrdinalIgnoreCase) ||
				trimmed.Equals("+00:00", StringComparison.Ordinal) ||
				trimmed.Equals("-00:00", StringComparison.Ordinal) ||
				trimmed.Equals("+0000", StringComparison.Ordinal))
			{
				offset = TimeSpan.Zero;
				return true;
			}

			var match = OffsetRegex().Match(trimmed);
			if (!match.Success)
			{
				return false;
			}

			var sign = match.Groups["sign"].Value == "-" ? -1 : 1;
			var hours = int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture);
			var minutes = int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture);
			offset = new TimeSpan(hours, minutes, 0) * sign;
			return true;
		}

		public static DateTimeOffset ApplyOffset(DateTimeOffset timestamp, TimeSpan offset)
		{
			var wallClock = DateTime.SpecifyKind(timestamp.DateTime, DateTimeKind.Unspecified);
			return new DateTimeOffset(wallClock, offset);
		}

		private static bool ContainsOffset(string value) =>
			OffsetSuffixRegex().IsMatch(value);

		private static string Normalize(string value)
		{
			var trimmed = value.Trim();
			if (trimmed.EndsWith('Z') || trimmed.EndsWith('z'))
			{
				trimmed = trimmed[..^1] + "+00:00";
			}

			trimmed = TimeZoneNameRegex().Replace(trimmed, string.Empty);
			return trimmed.Trim();
		}

		[GeneratedRegex(@"\s+[A-Za-z._]+$")]
		private static partial Regex TimeZoneNameRegex();

		[GeneratedRegex(@"[+-]\d{2}:?\d{2}$")]
		private static partial Regex OffsetSuffixRegex();

		[GeneratedRegex(@"^(?<sign>[+-])(?<hours>\d{2}):?(?<minutes>\d{2})$")]
		private static partial Regex OffsetRegex();
	}
}
