namespace FileSorterByDate
{
	internal sealed class TimeReference
	{
		public string Source { get; }
		public DateTimeOffset Value { get; }
		public bool HasExplicitOffset { get; }

		public TimeReference(string source, DateTimeOffset value, bool hasExplicitOffset)
		{
			Source = source;
			Value = value;
			HasExplicitOffset = hasExplicitOffset;
		}

		public string ToDisplayString()
		{
			var formatted = ExifDateFormats.FormatWithOffset(Value).Replace("+", " +", StringComparison.Ordinal);
			return HasExplicitOffset
				? $"{formatted} [{Source}]"
				: $"{formatted} [{Source}, assumed offset]";
		}
	}
}
