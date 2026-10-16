using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class TimeZoneSettingsTests
{
	[Fact]
	public void Attach_UsesCetInMarch()
	{
		var timestamp = TimeZoneSettings.Attach(new DateTime(2024, 3, 15, 14, 30, 22), TestTimeZones.Rome);

		Assert.Equal(TimeSpan.FromHours(1), timestamp.Offset);
		Assert.Equal(14, timestamp.Hour);
	}

	[Fact]
	public void Attach_UsesCestInJuly()
	{
		var timestamp = TimeZoneSettings.Attach(new DateTime(2024, 7, 15, 14, 30, 22), TestTimeZones.Rome);

		Assert.Equal(TimeSpan.FromHours(2), timestamp.Offset);
		Assert.Equal(14, timestamp.Hour);
	}

	[Fact]
	public void Attach_UsesTheProvidedTimeZone()
	{
		var timestamp = TimeZoneSettings.Attach(new DateTime(2024, 1, 15, 14, 30, 22), TestTimeZones.NewYork);

		Assert.Equal(TimeSpan.FromHours(-5), timestamp.Offset);
	}

	[Fact]
	public void AssumeIfMissingOrZero_ReplacesUtcPlaceholderKeepingWallClock()
	{
		var utc = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.Zero);

		var assumed = TimeZoneSettings.AssumeIfMissingOrZero(utc, TestTimeZones.Rome);

		Assert.Equal(new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1)), assumed);
	}

	[Fact]
	public void AssumeIfMissingOrZero_KeepsNonZeroOffset()
	{
		var original = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(5.5));

		Assert.Equal(original, TimeZoneSettings.AssumeIfMissingOrZero(original, TestTimeZones.Rome));
	}
}
