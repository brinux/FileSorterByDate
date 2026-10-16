namespace FileSorterByDate
{
	internal static class TimeReferenceSelector
	{
		public static TimeSelectionResult Select(
			IReadOnlyList<TimeReference> candidates,
			TimeSpan maxGap,
			ExcessiveTimeGapReactionEnum excessiveGapReaction)
		{
			if (candidates == null || candidates.Count == 0)
			{
				throw new ArgumentException("At least one time reference is required.", nameof(candidates));
			}

			candidates = DropUnreliableMidnights(candidates);

			var suggested = Suggest(candidates);
			var gap = GetGap(candidates);
			var requiresChoice = gap > maxGap;

			if (requiresChoice && excessiveGapReaction == ExcessiveTimeGapReactionEnum.skip)
			{
				var earliest = candidates.MinBy(c => c.Value)!;
				var latest = candidates.MaxBy(c => c.Value)!;
				throw new ExcessiveTimeGapException(
					$"The file has time references that differ too much: {ExifDateFormats.FormatWithOffset(latest.Value)} vs. {ExifDateFormats.FormatWithOffset(earliest.Value)}.");
			}

			return new TimeSelectionResult(candidates, suggested, requiresChoice);
		}

		public static TimeSpan GetGap(IReadOnlyList<TimeReference> candidates)
		{
			if (candidates.Count <= 1)
			{
				return TimeSpan.Zero;
			}

			return candidates.Max(c => c.Value) - candidates.Min(c => c.Value);
		}

		/// <summary>
		/// Midnight is treated as a date-only default. When a real time exists on the same
		/// calendar day, drop midnight so it neither wins nor inflates the gap.
		/// </summary>
		private static IReadOnlyList<TimeReference> DropUnreliableMidnights(IReadOnlyList<TimeReference> candidates)
		{
			var notAtMidnight = candidates.Where(c => !c.Value.IsAtMidnight()).ToList();
			if (notAtMidnight.Count == 0)
			{
				return candidates;
			}

			var daysWithRealTime = notAtMidnight
				.Select(c => c.Value.Date)
				.ToHashSet();

			var filtered = candidates
				.Where(c => !c.Value.IsAtMidnight() || !daysWithRealTime.Contains(c.Value.Date))
				.ToList();

			return filtered.Count > 0 ? filtered : candidates;
		}

		private static TimeReference Suggest(IReadOnlyList<TimeReference> candidates)
		{
			var notAtMidnight = candidates.Where(c => !c.Value.IsAtMidnight()).ToList();
			var preferred = notAtMidnight.Count > 0 ? notAtMidnight : candidates.ToList();

			// +00:00 is treated as a placeholder default for this archive (photos are rarely shot at UTC).
			var withNonZeroOffset = preferred.Where(c => c.Value.Offset != TimeSpan.Zero).ToList();
			if (withNonZeroOffset.Count > 0)
			{
				preferred = withNonZeroOffset;
			}

			return preferred.MinBy(c => c.Value)!;
		}
	}
}
