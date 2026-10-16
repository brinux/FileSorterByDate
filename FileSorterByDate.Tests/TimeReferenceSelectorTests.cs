using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class TimeReferenceSelectorTests
{
	private static readonly TimeSpan MaxGap = TimeSpan.FromSeconds(90);

	[Fact]
	public void Select_SingleCandidate_DoesNotRequireChoice()
	{
		var candidates = new[]
		{
			Ref("File name", "2024-03-15T14:30:22+01:00")
		};

		var result = TimeReferenceSelector.Select(candidates, MaxGap, ExcessiveTimeGapReactionEnum.ask);

		Assert.False(result.RequiresUserChoice);
		Assert.Equal(candidates[0].Value, result.Selected);
	}

	[Fact]
	public void Select_PrefersNonMidnightTimes()
	{
		var midnight = Ref("WhatsApp", "2024-03-15T00:00:00+01:00");
		var afternoon = Ref("EXIF", "2024-03-15T14:30:22+01:00");

		var result = TimeReferenceSelector.Select(
			[midnight, afternoon],
			MaxGap,
			ExcessiveTimeGapReactionEnum.ask);

		Assert.Equal(afternoon.Value, result.Selected);
		Assert.False(result.RequiresUserChoice);
		Assert.DoesNotContain(result.Candidates, c => c.Value.IsAtMidnight());
	}

	[Fact]
	public void Select_DropsSameDayMidnightBeforeMeasuringGap()
	{
		var midnight = Ref("File name", "2013-09-12T00:00:00+02:00");
		var morning = Ref("EXIF", "2013-09-12T09:15:00+02:00");

		var result = TimeReferenceSelector.Select(
			[midnight, morning],
			MaxGap,
			ExcessiveTimeGapReactionEnum.ask);

		Assert.False(result.RequiresUserChoice);
		Assert.Equal(morning.Value, result.Selected);
		Assert.Single(result.Candidates);
	}

	[Fact]
	public void Select_KeepsMidnightWhenNoRealTimeExistsOnThatDay()
	{
		var midnight = Ref("File name", "2013-09-12T00:00:00+02:00");
		var nextDay = Ref("EXIF", "2013-09-13T09:15:00+02:00");

		var result = TimeReferenceSelector.Select(
			[midnight, nextDay],
			MaxGap,
			ExcessiveTimeGapReactionEnum.ask);

		Assert.True(result.RequiresUserChoice);
		Assert.Contains(result.Candidates, c => c.Value.IsAtMidnight());
		Assert.Equal(2, result.Candidates.Count);
	}

	[Fact]
	public void Select_TreatsZeroOffsetAsUnreliableDefault()
	{
		var utcDefault = new TimeReference(
			"XMP",
			new DateTimeOffset(2024, 3, 15, 13, 30, 22, TimeSpan.Zero),
			true);
		var italy = new TimeReference(
			"EXIF",
			new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1)),
			false);

		var result = TimeReferenceSelector.Select([utcDefault, italy], MaxGap, ExcessiveTimeGapReactionEnum.ask);

		Assert.Equal(italy.Value, result.Selected);
		Assert.Equal(TimeSpan.FromHours(1), result.Selected.Offset);
	}

	[Fact]
	public void Select_WhenGapExceedsThresholdInAskMode_RequiresChoice()
	{
		var first = Ref("File name", "2024-03-15T14:30:22+01:00");
		var second = Ref("EXIF", "2024-03-15T16:30:22+01:00");

		var result = TimeReferenceSelector.Select(
			[first, second],
			MaxGap,
			ExcessiveTimeGapReactionEnum.ask);

		Assert.True(result.RequiresUserChoice);
	}

	[Fact]
	public void Select_WhenGapExceedsThresholdInSkipMode_Throws()
	{
		var first = Ref("File name", "2024-03-15T14:30:22+01:00");
		var second = Ref("EXIF", "2024-03-15T16:30:22+01:00");

		Assert.Throws<ExcessiveTimeGapException>(() =>
			TimeReferenceSelector.Select([first, second], MaxGap, ExcessiveTimeGapReactionEnum.skip));
	}

	[Fact]
	public void Select_SmallGap_DoesNotRequireChoice()
	{
		var first = Ref("File name", "2024-03-15T14:30:22+01:00");
		var second = Ref("EXIF", "2024-03-15T14:31:00+01:00");

		var result = TimeReferenceSelector.Select(
			[first, second],
			MaxGap,
			ExcessiveTimeGapReactionEnum.ask);

		Assert.False(result.RequiresUserChoice);
		Assert.Equal(first.Value, result.Selected);
	}

	private static TimeReference Ref(string source, string iso) =>
		new(source, DateTimeOffset.Parse(iso), true);
}
