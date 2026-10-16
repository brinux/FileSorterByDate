namespace FileSorterByDate
{
	internal static class MetadataTimeCollector
	{
		public static IReadOnlyList<TimeReference> Collect(
			DateTimeOffset fileNameTime,
			IReadOnlyDictionary<string, string> metadata,
			IEnumerable<TimeMetadataConfig> fields,
			TimeZoneInfo defaultTimeZone)
		{
			var fieldList = fields.ToList();
			var offset = ReadPreferredOffset(metadata, fieldList);

			var candidates = new List<TimeReference>
			{
				ToReference("File name", fileNameTime, fileNameTime.Offset != TimeSpan.Zero, defaultTimeZone)
			};

			foreach (var field in fieldList.Where(f => f.Kind != MetadataTimeKind.TimeZoneOffset))
			{
				if (!MetadataLookup.TryGetValue(metadata, field.Name, out var rawValue) ||
					!ExifDateTimeParser.TryParse(rawValue, field.Kind, defaultTimeZone, out var timestamp, out var hasExplicitOffset))
				{
					continue;
				}

				if (field.Kind == MetadataTimeKind.Local && offset.HasValue && !hasExplicitOffset)
				{
					timestamp = ExifDateTimeParser.ApplyOffset(timestamp, offset.Value);
					hasExplicitOffset = offset.Value != TimeSpan.Zero;
				}

				var reference = ToReference(field.Name, timestamp, hasExplicitOffset, defaultTimeZone);
				if (candidates.Any(c => c.Value == reference.Value))
				{
					continue;
				}

				candidates.Add(reference);
			}

			return candidates;
		}

		private static TimeSpan? ReadPreferredOffset(
			IReadOnlyDictionary<string, string> metadata,
			IReadOnlyList<TimeMetadataConfig> fields)
		{
			foreach (var field in fields.Where(f => f.Kind == MetadataTimeKind.TimeZoneOffset))
			{
				if (MetadataLookup.TryGetValue(metadata, field.Name, out var rawValue) &&
					ExifDateTimeParser.TryParseOffset(rawValue, out var offset))
				{
					return offset;
				}
			}

			return null;
		}

		private static TimeReference ToReference(
			string source,
			DateTimeOffset timestamp,
			bool hasExplicitOffset,
			TimeZoneInfo defaultTimeZone)
		{
			if (timestamp.Offset == TimeSpan.Zero)
			{
				return new TimeReference(source, TimeZoneSettings.AssumeIfMissingOrZero(timestamp, defaultTimeZone), false);
			}

			return new TimeReference(source, timestamp, hasExplicitOffset);
		}
	}
}
