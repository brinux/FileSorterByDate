using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class DateTimeOffsetExtensionsTests
{
	[Fact]
	public void IsAtMidnight_WhenTimeIsExactlyMidnight_ReturnsTrue()
	{
		var value = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.FromHours(1));

		Assert.True(value.IsAtMidnight());
	}

	[Fact]
	public void IsAtMidnight_WhenSecondsAreNotZero_ReturnsFalse()
	{
		var value = new DateTimeOffset(2024, 3, 15, 0, 0, 1, TimeSpan.FromHours(1));

		Assert.False(value.IsAtMidnight());
	}

	[Fact]
	public void IsAtMidnight_WhenMinutesAreNotZero_ReturnsFalse()
	{
		var value = new DateTimeOffset(2024, 3, 15, 0, 1, 0, TimeSpan.FromHours(1));

		Assert.False(value.IsAtMidnight());
	}
}
