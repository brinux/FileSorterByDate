namespace FileSorterByDate
{
	internal static class DestinationPathBuilder
	{
		public static string Build(string destinationFolder, DateTime timestamp) =>
			Path.Combine(destinationFolder, timestamp.Year.ToString(), timestamp.Month.ToString());
	}
}
