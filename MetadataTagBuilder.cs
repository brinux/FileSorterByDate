namespace FileSorterByDate
{
	internal static class MetadataTagBuilder
	{
		public static Dictionary<string, string> Build(
			DateTimeOffset timestamp,
			IEnumerable<TimeMetadataConfig> fields)
		{
			var tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (var field in fields.Where(f => f.CanWrite))
			{
				tags[field.Name] = field.Kind switch
				{
					MetadataTimeKind.Local => ExifDateFormats.FormatLocal(timestamp),
					MetadataTimeKind.Utc => ExifDateFormats.FormatUtc(timestamp),
					MetadataTimeKind.WithOffset => ExifDateFormats.FormatWithOffset(timestamp),
					MetadataTimeKind.TimeZoneOffset => ExifDateFormats.FormatOffset(timestamp.Offset),
					_ => throw new ArgumentOutOfRangeException(nameof(fields), field.Kind, "Unknown metadata time kind.")
				};
			}

			return tags;
		}
	}
}
