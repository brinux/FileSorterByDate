using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class MetadataTagBuilderTests
{
	[Fact]
	public void Build_ImageTags_WriteLocalExifAndOffset()
	{
		var timestamp = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));

		var tags = MetadataTagBuilder.Build(timestamp, MetadataProfiles.Image);

		Assert.Equal("2024:03:15 14:30:22", tags["ExifIFD:DateTimeOriginal"]);
		Assert.Equal("2024:03:15 14:30:22", tags["ExifIFD:CreateDate"]);
		Assert.Equal("+01:00", tags["ExifIFD:OffsetTimeOriginal"]);
		Assert.Equal("+01:00", tags["ExifIFD:OffsetTime"]);
		Assert.Equal("2024:03:15 14:30:22+01:00", tags["XMP:CreateDate"]);
		Assert.False(tags.ContainsKey("System:FileCreateDate"));
	}

	[Fact]
	public void Build_VideoTags_WriteQuickTimeAndCreationDateWithOffset()
	{
		var timestamp = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));

		var tags = MetadataTagBuilder.Build(timestamp, MetadataProfiles.Video);

		Assert.Equal("2024:03:15 14:30:22+01:00", tags["QuickTime:CreateDate"]);
		Assert.Equal("2024:03:15 14:30:22+01:00", tags["QuickTime:MediaCreateDate"]);
		Assert.Equal("2024:03:15 14:30:22+01:00", tags["Keys:CreationDate"]);
		Assert.Equal("2024:03:15 14:30:22+01:00", tags["UserData:DateTimeOriginal"]);
		Assert.False(tags.ContainsKey("System:FileCreateDate"));
	}

	[Fact]
	public void Build_DoesNotWriteUtcIntoDateTimeOriginal()
	{
		var timestamp = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(2));
		var tags = MetadataTagBuilder.Build(timestamp, MetadataProfiles.Image);

		Assert.Equal("2024:03:15 14:30:22", tags["ExifIFD:DateTimeOriginal"]);
		Assert.NotEqual("2024:03:15 12:30:22", tags["ExifIFD:DateTimeOriginal"]);
	}
}
