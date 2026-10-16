using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class MetadataTimeCollectorTests
{
	[Fact]
	public void Collect_AppliesExifOffsetToLocalDateTimeOriginal()
	{
		var fileNameTime = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));
		var metadata = new Dictionary<string, string>
		{
			["ExifIFD:DateTimeOriginal"] = "2024:03:15 16:45:00",
			["ExifIFD:OffsetTimeOriginal"] = "+02:00"
		};

		var candidates = MetadataTimeCollector.Collect(fileNameTime, metadata, MetadataProfiles.Image, TestTimeZones.Rome);

		var exif = Assert.Single(candidates, c => c.Source == "ExifIFD:DateTimeOriginal");
		Assert.Equal(new DateTimeOffset(2024, 3, 15, 16, 45, 0, TimeSpan.FromHours(2)), exif.Value);
		Assert.True(exif.HasExplicitOffset);
	}

	[Fact]
	public void Collect_DoesNotParseOffsetLessExifAsUtc()
	{
		var fileNameTime = new DateTimeOffset(2024, 3, 15, 10, 0, 0, TimeSpan.FromHours(1));
		var metadata = new Dictionary<string, string>
		{
			["ExifIFD:DateTimeOriginal"] = "2024:03:15 14:30:22"
		};

		var candidates = MetadataTimeCollector.Collect(fileNameTime, metadata, MetadataProfiles.Image, TestTimeZones.Rome);
		var exif = Assert.Single(candidates, c => c.Source == "ExifIFD:DateTimeOriginal");

		Assert.Equal(new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1)), exif.Value);
		Assert.False(exif.HasExplicitOffset);
	}

	[Fact]
	public void Collect_ReplacesZeroOffsetWithDefaultTimeZone()
	{
		var fileNameTime = new DateTimeOffset(2024, 7, 15, 10, 0, 0, TimeSpan.FromHours(2));
		var metadata = new Dictionary<string, string>
		{
			["XMP:CreateDate"] = "2024:07:15 14:30:22+00:00"
		};

		var candidates = MetadataTimeCollector.Collect(fileNameTime, metadata, MetadataProfiles.Image, TestTimeZones.Rome);
		var xmp = Assert.Single(candidates, c => c.Source == "XMP:CreateDate");

		Assert.Equal(new DateTimeOffset(2024, 7, 15, 14, 30, 22, TimeSpan.FromHours(2)), xmp.Value);
		Assert.False(xmp.HasExplicitOffset);
	}

	[Fact]
	public void Collect_SkipsUnsetQuickTimeDates()
	{
		var fileNameTime = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));
		var metadata = new Dictionary<string, string>
		{
			["QuickTime:CreateDate"] = "0000:00:00 00:00:00"
		};

		var candidates = MetadataTimeCollector.Collect(fileNameTime, metadata, MetadataProfiles.Video, TestTimeZones.Rome);

		Assert.DoesNotContain(candidates, c => c.Source == "QuickTime:CreateDate");
	}

	[Fact]
	public void Collect_ReadsGroupedAndUngroupedOffsetTags()
	{
		var fileNameTime = new DateTimeOffset(2024, 3, 15, 14, 30, 22, TimeSpan.FromHours(1));
		var metadata = new Dictionary<string, string>
		{
			["DateTimeOriginal"] = "2024:03:15 16:00:00",
			["OffsetTimeOriginal"] = "+02:00"
		};

		var candidates = MetadataTimeCollector.Collect(fileNameTime, metadata, MetadataProfiles.Image, TestTimeZones.Rome);
		var exif = Assert.Single(candidates, c => c.Source == "ExifIFD:DateTimeOriginal");

		Assert.Equal(TimeSpan.FromHours(2), exif.Value.Offset);
		Assert.Equal(16, exif.Value.Hour);
	}

	[Fact]
	public void Collect_UsesTheProvidedDefaultTimeZoneForZeroOffset()
	{
		var fileNameTime = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.FromHours(-5));
		var metadata = new Dictionary<string, string>
		{
			["XMP:CreateDate"] = "2024:01:15 14:30:22+00:00"
		};

		var candidates = MetadataTimeCollector.Collect(fileNameTime, metadata, MetadataProfiles.Image, TestTimeZones.NewYork);
		var xmp = Assert.Single(candidates, c => c.Source == "XMP:CreateDate");

		Assert.Equal(new DateTimeOffset(2024, 1, 15, 14, 30, 22, TimeSpan.FromHours(-5)), xmp.Value);
	}
}
