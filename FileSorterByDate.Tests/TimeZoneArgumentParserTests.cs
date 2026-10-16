using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class TimeZoneArgumentParserTests
{
	[Fact]
	public void TryParse_WithoutArguments_UsesRome()
	{
		Assert.True(TimeZoneArgumentParser.TryParse([], out var timeZone, out var error));

		Assert.Null(error);
		Assert.Equal(TestTimeZones.Rome.Id, timeZone.Id);
	}

	[Theory]
	[InlineData("--timezone", "America/New_York")]
	[InlineData("-tz", "America/New_York")]
	public void TryParse_ReadsFlagAndValue(string flag, string id)
	{
		Assert.True(TimeZoneArgumentParser.TryParse([flag, id], out var timeZone, out var error));

		Assert.Null(error);
		Assert.Equal(TestTimeZones.NewYork.Id, timeZone.Id);
	}

	[Fact]
	public void TryParse_ReadsInlineValue()
	{
		Assert.True(TimeZoneArgumentParser.TryParse(["--timezone=America/New_York"], out var timeZone, out var error));

		Assert.Null(error);
		Assert.Equal(TestTimeZones.NewYork.Id, timeZone.Id);
	}

	[Fact]
	public void TryParse_UnknownId_ReturnsError()
	{
		Assert.False(TimeZoneArgumentParser.TryParse(["--timezone", "Not/AZone"], out _, out var error));

		Assert.Contains("Unknown time zone", error);
	}

	[Fact]
	public void TryParse_MissingValue_ReturnsError()
	{
		Assert.False(TimeZoneArgumentParser.TryParse(["--timezone"], out _, out var error));

		Assert.Contains("Missing value", error);
	}
}
