namespace FileSorterByDate
{
	internal sealed class ExcessiveTimeGapException : Exception
	{
		public ExcessiveTimeGapException(string message)
			: base(message)
		{
		}
	}
}
