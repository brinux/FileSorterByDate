using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class SortPipelineTests
{
	[Fact]
	public void SelectTimestamp_WhenExifMatchesFileName_DoesNotAskTheUser()
	{
		var fileNameTime = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));
		var metadata = new Dictionary<string, string>
		{
			["ExifIFD:DateTimeOriginal"] = "2024:03:15 14:30:22",
			["ExifIFD:OffsetTimeOriginal"] = "+01:00"
		};

		var result = SortPipeline.SelectTimestamp(
			fileNameTime,
			metadata,
			SorterFileTypesEnum.Image,
			TimeSpan.FromSeconds(90),
			ExcessiveTimeGapReactionEnum.ask,
			TestTimeZones.Rome);

		Assert.False(result.RequiresUserChoice);
		Assert.Equal(fileNameTime, result.Selected);
	}

	[Fact]
	public void SelectTimestamp_VideoQuickTimeLocalTime_AlignsWithFileName()
	{
		var fileNameTime = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));
		var metadata = new Dictionary<string, string>
		{
			["QuickTime:CreateDate"] = "2024:03:15 14:30:22+01:00",
			["Keys:CreationDate"] = "2024:03:15 14:30:22+01:00"
		};

		var result = SortPipeline.SelectTimestamp(
			fileNameTime,
			metadata,
			SorterFileTypesEnum.Video,
			TimeSpan.FromSeconds(90),
			ExcessiveTimeGapReactionEnum.ask,
			TestTimeZones.Rome);

		Assert.False(result.RequiresUserChoice);
		Assert.Equal(fileNameTime, result.Selected);
	}

	[Fact]
	public void BuildTags_ReturnsTheProfileForTheFileType()
	{
		var timestamp = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));

		var imageTags = SortPipeline.BuildTags(timestamp, SorterFileTypesEnum.Image);
		var videoTags = SortPipeline.BuildTags(timestamp, SorterFileTypesEnum.Video);

		Assert.Contains("ExifIFD:DateTimeOriginal", imageTags.Keys);
		Assert.Contains("Keys:CreationDate", videoTags.Keys);
		Assert.DoesNotContain("Keys:CreationDate", imageTags.Keys);
	}

	[Fact]
	public void SelectTimestamp_WhenIgnoringSystemFileCreateDate_DoesNotAskForDiscordantFileCreateDate()
	{
		var fileNameTime = new DateTimeOffset(2013, 9, 12, 0, 0, 0, TimeSpan.FromHours(2));
		var metadata = new Dictionary<string, string>
		{
			["System:FileCreateDate"] = "2022:10:18 05:52:42+02:00"
		};

		var ignored = SortPipeline.SelectTimestamp(
			fileNameTime,
			metadata,
			SorterFileTypesEnum.Video,
			TimeSpan.FromSeconds(90),
			ExcessiveTimeGapReactionEnum.ask,
			TestTimeZones.Rome,
			ignoreSystemFileCreateDate: true);

		Assert.False(ignored.RequiresUserChoice);
		Assert.Equal(fileNameTime, ignored.Selected);
		Assert.DoesNotContain(ignored.Candidates, c => c.Source == "System:FileCreateDate");
	}

	[Fact]
	public void SelectTimestamp_WhenKeepingSystemFileCreateDate_RequiresChoiceIfDiscordant()
	{
		var fileNameTime = new DateTimeOffset(2013, 9, 12, 0, 0, 0, TimeSpan.FromHours(2));
		var metadata = new Dictionary<string, string>
		{
			["System:FileCreateDate"] = "2022:10:18 05:52:42+02:00"
		};

		var kept = SortPipeline.SelectTimestamp(
			fileNameTime,
			metadata,
			SorterFileTypesEnum.Video,
			TimeSpan.FromSeconds(90),
			ExcessiveTimeGapReactionEnum.ask,
			TestTimeZones.Rome,
			ignoreSystemFileCreateDate: false);

		Assert.True(kept.RequiresUserChoice);
		Assert.Contains(kept.Candidates, c => c.Source == "System:FileCreateDate");
	}

	[Fact]
	public void SelectTimestamp_IgnoresMetadataBeforeConfiguredYear()
	{
		var fileNameTime = new DateTimeOffset(2013, 9, 12, 14, 30, 0, TimeSpan.FromHours(2));
		var metadata = new Dictionary<string, string>
		{
			["ExifIFD:DateTimeOriginal"] = "2008:01:01 12:00:00",
			["ExifIFD:OffsetTimeOriginal"] = "+01:00"
		};

		var result = SortPipeline.SelectTimestamp(
			fileNameTime,
			metadata,
			SorterFileTypesEnum.Image,
			TimeSpan.FromSeconds(90),
			ExcessiveTimeGapReactionEnum.ask,
			TestTimeZones.Rome,
			ignoreSystemFileCreateDate: true,
			ignoreMetadataBeforeYear: 2010);

		Assert.False(result.RequiresUserChoice);
		Assert.Equal(fileNameTime, result.Selected);
		Assert.DoesNotContain(result.Candidates, c => c.Source == "ExifIFD:DateTimeOriginal");
	}

	[Fact]
	public void SelectTimestamp_KeepsMetadataFromConfiguredYearOnward()
	{
		var fileNameTime = new DateTimeOffset(2013, 9, 12, 14, 30, 0, TimeSpan.FromHours(2));
		var metadata = new Dictionary<string, string>
		{
			["ExifIFD:DateTimeOriginal"] = "2010:01:01 12:00:00",
			["ExifIFD:OffsetTimeOriginal"] = "+01:00"
		};

		var result = SortPipeline.SelectTimestamp(
			fileNameTime,
			metadata,
			SorterFileTypesEnum.Image,
			TimeSpan.FromSeconds(90),
			ExcessiveTimeGapReactionEnum.ask,
			TestTimeZones.Rome,
			ignoreSystemFileCreateDate: true,
			ignoreMetadataBeforeYear: 2010);

		Assert.True(result.RequiresUserChoice);
		Assert.Contains(result.Candidates, c => c.Source == "ExifIFD:DateTimeOriginal");
	}

	[Fact]
	public void SelectTimestamp_WhenYearFilterDisabled_KeepsOldMetadata()
	{
		var fileNameTime = new DateTimeOffset(2013, 9, 12, 14, 30, 0, TimeSpan.FromHours(2));
		var metadata = new Dictionary<string, string>
		{
			["ExifIFD:DateTimeOriginal"] = "2008:01:01 12:00:00",
			["ExifIFD:OffsetTimeOriginal"] = "+01:00"
		};

		var result = SortPipeline.SelectTimestamp(
			fileNameTime,
			metadata,
			SorterFileTypesEnum.Image,
			TimeSpan.FromSeconds(90),
			ExcessiveTimeGapReactionEnum.ask,
			TestTimeZones.Rome,
			ignoreSystemFileCreateDate: true,
			ignoreMetadataBeforeYear: 0);

		Assert.True(result.RequiresUserChoice);
		Assert.Contains(result.Candidates, c => c.Source == "ExifIFD:DateTimeOriginal");
	}
}
