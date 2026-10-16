namespace FileSorterByDate
{
	internal static class MetadataProfiles
	{
		public static IReadOnlyList<TimeMetadataConfig> For(SorterFileTypesEnum fileType) =>
			fileType switch
			{
				SorterFileTypesEnum.Image => Image,
				SorterFileTypesEnum.Video => Video,
				_ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, "Unknown file type.")
			};

		public static IReadOnlyList<TimeMetadataConfig> Image { get; } =
		[
			new TimeMetadataConfig("ExifIFD:DateTimeOriginal", MetadataTimeKind.Local),
			new TimeMetadataConfig("ExifIFD:CreateDate", MetadataTimeKind.Local),
			new TimeMetadataConfig("IFD0:ModifyDate", MetadataTimeKind.Local),
			new TimeMetadataConfig("XMP:DateTimeOriginal", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("XMP:CreateDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("XMP:ModifyDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("ExifIFD:OffsetTimeOriginal", MetadataTimeKind.TimeZoneOffset),
			new TimeMetadataConfig("ExifIFD:OffsetTimeDigitized", MetadataTimeKind.TimeZoneOffset),
			new TimeMetadataConfig("ExifIFD:OffsetTime", MetadataTimeKind.TimeZoneOffset),
			new TimeMetadataConfig("System:FileCreateDate", MetadataTimeKind.WithOffset, canWrite: false),
		];

		public static IReadOnlyList<TimeMetadataConfig> Video { get; } =
		[
			new TimeMetadataConfig("QuickTime:CreateDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("QuickTime:ModifyDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("QuickTime:MediaCreateDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("QuickTime:MediaModifyDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("QuickTime:TrackCreateDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("QuickTime:TrackModifyDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("Keys:CreationDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("UserData:DateTimeOriginal", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("XMP:DateTimeOriginal", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("XMP:CreateDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("XMP:ModifyDate", MetadataTimeKind.WithOffset),
			new TimeMetadataConfig("System:FileCreateDate", MetadataTimeKind.WithOffset, canWrite: false),
		];
	}
}
