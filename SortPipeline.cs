namespace FileSorterByDate
{
	internal static class SortPipeline
	{
		public static TimeSelectionResult SelectTimestamp(
			DateTimeOffset fileNameTime,
			IReadOnlyDictionary<string, string> metadata,
			SorterFileTypesEnum fileType,
			TimeSpan maxGap,
			ExcessiveTimeGapReactionEnum reaction,
			TimeZoneInfo defaultTimeZone,
			bool ignoreSystemFileCreateDate = true,
			int ignoreMetadataBeforeYear = 2010)
		{
			IEnumerable<TimeMetadataConfig> profile = MetadataProfiles.For(fileType);
			if (ignoreSystemFileCreateDate)
			{
				profile = profile.Where(f =>
					!f.Name.Equals("System:FileCreateDate", StringComparison.OrdinalIgnoreCase));
			}

			var candidates = MetadataTimeCollector.Collect(fileNameTime, metadata, profile, defaultTimeZone);
			candidates = FilterByMinimumYear(candidates, ignoreMetadataBeforeYear);
			return TimeReferenceSelector.Select(candidates, maxGap, reaction);
		}

		public static Dictionary<string, string> BuildTags(
			DateTimeOffset timestamp,
			SorterFileTypesEnum fileType) =>
			MetadataTagBuilder.Build(timestamp, MetadataProfiles.For(fileType));

		private static IReadOnlyList<TimeReference> FilterByMinimumYear(
			IReadOnlyList<TimeReference> candidates,
			int ignoreMetadataBeforeYear)
		{
			if (ignoreMetadataBeforeYear <= 0)
			{
				return candidates;
			}

			return candidates
				.Where(c =>
					c.Source.Equals("File name", StringComparison.OrdinalIgnoreCase) ||
					c.Value.Year >= ignoreMetadataBeforeYear)
				.ToList();
		}
	}
}
