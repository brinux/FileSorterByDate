using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class ExifDateFormatsTests
{
	[Fact]
	public void FormatOffset_FormatsPositiveAndNegativeOffsets()
	{
		Assert.Equal("+01:00", ExifDateFormats.FormatOffset(TimeSpan.FromHours(1)));
		Assert.Equal("-05:30", ExifDateFormats.FormatOffset(new TimeSpan(-5, -30, 0)));
		Assert.Equal("+00:00", ExifDateFormats.FormatOffset(TimeSpan.Zero));
	}

	[Fact]
	public void FormatLocal_DoesNotIncludeOffset()
	{
		var timestamp = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));

		Assert.Equal("2024:03:15 14:30:22", ExifDateFormats.FormatLocal(timestamp));
		Assert.Equal("2024:03:15 13:30:22", ExifDateFormats.FormatUtc(timestamp));
		Assert.Equal("2024:03:15 14:30:22+01:00", ExifDateFormats.FormatWithOffset(timestamp));
	}
}
