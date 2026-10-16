namespace FileSorterByDate
{
	internal sealed class TimeSelectionResult
	{
		public IReadOnlyList<TimeReference> Candidates { get; }
		public TimeReference Suggested { get; }
		public bool RequiresUserChoice { get; }

		public TimeSelectionResult(
			IReadOnlyList<TimeReference> candidates,
			TimeReference suggested,
			bool requiresUserChoice)
		{
			Candidates = candidates;
			Suggested = suggested;
			RequiresUserChoice = requiresUserChoice;
		}

		public DateTimeOffset Selected => Suggested.Value;
	}
}
