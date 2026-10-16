using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class ExifDateTimeParserTests
{
	[Fact]
	public void TryParse_WithOffset_KeepsTheOffset()
	{
		var parsed = ExifDateTimeParser.TryParse(
			"2024:03:15 14:30:22+01:00",
			MetadataTimeKind.WithOffset,
			TestTimeZones.Rome,
			out var timestamp,
			out var hasExplicitOffset);

		Assert.True(parsed);
		Assert.True(hasExplicitOffset);
		Assert.Equal(new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1)), timestamp);
	}

	[Fact]
	public void TryParse_LocalWithoutOffset_UsesProvidedTimeZone()
	{
		var parsed = ExifDateTimeParser.TryParse(
			"2024:03:15 14:30:22",
			MetadataTimeKind.Local,
			TestTimeZones.Rome,
			out var timestamp,
			out var hasExplicitOffset);

		Assert.True(parsed);
		Assert.False(hasExplicitOffset);
		Assert.Equal(new DateTime(2024, 3, 15, 14, 30, 22), timestamp.DateTime);
		Assert.Equal(TimeSpan.FromHours(1), timestamp.Offset);
	}

	[Fact]
	public void TryParse_LocalWithoutOffset_CanUseAnotherTimeZone()
	{
		var parsed = ExifDateTimeParser.TryParse(
			"2024:01:15 14:30:22",
			MetadataTimeKind.Local,
			TestTimeZones.NewYork,
			out var timestamp,
			out _);

		Assert.True(parsed);
		Assert.Equal(TimeSpan.FromHours(-5), timestamp.Offset);
	}

	[Fact]
	public void TryParse_UtcWithoutOffset_DoesNotTreatTheValueAsLocal()
	{
		var parsed = ExifDateTimeParser.TryParse(
			"2024:03:15 14:30:22",
			MetadataTimeKind.Utc,
			TestTimeZones.Rome,
			out var timestamp,
			out var hasExplicitOffset);

		Assert.True(parsed);
		Assert.False(hasExplicitOffset);
		Assert.Equal(TimeSpan.Zero, timestamp.Offset);
		Assert.Equal(new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.Zero), timestamp);
	}

	[Fact]
	public void TryParse_ZuluSuffix_IsUtc()
	{
		var parsed = ExifDateTimeParser.TryParse(
			"2024:03:15 14:30:22Z",
			MetadataTimeKind.WithOffset,
			TestTimeZones.Rome,
			out var timestamp,
			out var hasExplicitOffset);

		Assert.True(parsed);
		Assert.True(hasExplicitOffset);
		Assert.Equal(new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.Zero), timestamp);
	}

	[Fact]
	public void TryParse_TrailingTimeZoneName_IsIgnored()
	{
		var parsed = ExifDateTimeParser.TryParse(
			"2024:03:15 14:30:22+01:00 CEST",
			MetadataTimeKind.WithOffset,
			TestTimeZones.Rome,
			out var timestamp,
			out var hasExplicitOffset);

		Assert.True(parsed);
		Assert.True(hasExplicitOffset);
		Assert.Equal(new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1)), timestamp);
	}

	[Theory]
	[InlineData("0000:00:00 00:00:00")]
	[InlineData("")]
	[InlineData(null)]
	public void TryParse_UnsetValues_ReturnFalse(string? value)
	{
		var parsed = ExifDateTimeParser.TryParse(value, MetadataTimeKind.Local, TestTimeZones.Rome, out _, out _);

		Assert.False(parsed);
	}

	[Theory]
	[InlineData("+01:00", 1, 0)]
	[InlineData("-05:30", -5, -30)]
	[InlineData("+0000", 0, 0)]
	[InlineData("Z", 0, 0)]
	public void TryParseOffset_ValidValues(string value, int hours, int minutes)
	{
		Assert.True(ExifDateTimeParser.TryParseOffset(value, out var offset));
		Assert.Equal(new TimeSpan(hours, minutes, 0), offset);
	}

	[Fact]
	public void ApplyOffset_ReplacesAssumedOffsetKeepingWallClock()
	{
		var assumedLocal = new DateTimeOffset(new DateTime(2024, 3, 15, 14, 30, 22));
		var applied = ExifDateTimeParser.ApplyOffset(assumedLocal, TimeSpan.FromHours(5.5));

		Assert.Equal(14, applied.Hour);
		Assert.Equal(30, applied.Minute);
		Assert.Equal(TimeSpan.FromHours(5.5), applied.Offset);
	}
}
