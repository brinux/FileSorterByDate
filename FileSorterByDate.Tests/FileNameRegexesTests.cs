using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class FileNameRegexesTests
{
	[Theory]
	[InlineData("2024-03-15 14.30.22", 2024, 3, 15, 14, 30, 22)]
	[InlineData("2024-03-15 14.30.22 HDR", 2024, 3, 15, 14, 30, 22)]
	public void StandardDateAndTime_ParsesCameraNames(string fileName, int year, int month, int day, int hour, int minute, int second)
	{
		AssertParsed(FileNameRegexes.StandardDateAndTime(), fileName, year, month, day, hour, minute, second);
	}

	[Fact]
	public void DateAndTimeSeparatedByUnderscore_ParsesCompactNames()
	{
		AssertParsed(FileNameRegexes.DateAndTimeSeparatedByUnderscore(), "20240315_143022", 2024, 3, 15, 14, 30, 22);
	}

	[Theory]
	[InlineData("WP_20240315_14_30_22_Pro")]
	[InlineData("WP_20240315_14_30_22_Rich")]
	[InlineData("WP_20240315_14_30_22_Selfie")]
	public void WindowsPhoneDatesWithTime_ParsesKnownSuffixes(string fileName)
	{
		AssertParsed(FileNameRegexes.WindowsPhoneDatesWithTime(), fileName, 2024, 3, 15, 14, 30, 22);
	}

	[Fact]
	public void WindowsPhoneDatesWithoutTime_DefaultsToMidnight()
	{
		AssertParsed(FileNameRegexes.WindowsPhoneDatesWithoutTime(), "WP_20240315_001", 2024, 3, 15, 0, 0, 0);
	}

	[Fact]
	public void ScreenshotsDateAndTimeFieldsDividedByDashes_Parses()
	{
		AssertParsed(
			FileNameRegexes.ScreenshotsDateAndTimeFieldsDividedByDashes(),
			"Screenshot_2024-03-15-14-30-22",
			2024, 3, 15, 14, 30, 22);
	}

	[Fact]
	public void ScreenshotsDateAndTimeDividedByDash_ParsesOptionalDeviceSuffix()
	{
		AssertParsed(
			FileNameRegexes.ScreenshotsDateAndTimeDividedByDash(),
			"Screenshot_20240315-143022_GBoard",
			2024, 3, 15, 14, 30, 22);
	}

	[Fact]
	public void WhatsappImages_HaveDateOnly()
	{
		AssertParsed(FileNameRegexes.WhatsappImages(), "IMG-20240315-WA0001", 2024, 3, 15, 0, 0, 0);
	}

	[Fact]
	public void WhatsappVideos_HaveDateOnly()
	{
		AssertParsed(FileNameRegexes.WhatsappVideos(), "VID-20240315-WA0123", 2024, 3, 15, 0, 0, 0);
	}

	[Theory]
	[InlineData("random-file")]
	[InlineData("IMG_1234")]
	[InlineData("video")]
	public void UnknownNames_DoNotMatchCameraPattern(string fileName)
	{
		Assert.DoesNotMatch(FileNameRegexes.StandardDateAndTime(), fileName);
	}

	private static void AssertParsed(
		System.Text.RegularExpressions.Regex regex,
		string fileName,
		int year,
		int month,
		int day,
		int hour,
		int minute,
		int second)
	{
		var match = regex.Match(fileName);
		Assert.True(FileNameTimestampParser.TryParse(match, TestTimeZones.Rome, out var timestamp));
		Assert.Equal(new DateTime(year, month, day, hour, minute, second), timestamp.DateTime);
		Assert.Equal(TestTimeZones.Rome.GetUtcOffset(timestamp.DateTime), timestamp.Offset);
	}
}
