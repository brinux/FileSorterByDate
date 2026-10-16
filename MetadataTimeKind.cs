namespace FileSorterByDate
{
	internal enum MetadataTimeKind
	{
		/// <summary>
		/// Local wall-clock time without an offset (EXIF DateTimeOriginal / CreateDate).
		/// </summary>
		Local,

		/// <summary>
		/// UTC time without an offset.
		/// </summary>
		Utc,

		/// <summary>
		/// Date-time that may include an offset (XMP, Keys:CreationDate, System:FileCreateDate).
		/// </summary>
		WithOffset,

		/// <summary>
		/// Timezone offset tag such as OffsetTimeOriginal (+01:00).
		/// </summary>
		TimeZoneOffset
	}
}
