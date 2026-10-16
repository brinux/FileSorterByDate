namespace FileSorterByDate
{
	internal static class UniqueFileName
	{
		public static string GetAvailablePath(string directory, string fileName)
		{
			var destination = Path.Combine(directory, fileName);
			if (!File.Exists(destination))
			{
				return destination;
			}

			var name = Path.GetFileNameWithoutExtension(fileName);
			var extension = Path.GetExtension(fileName);
			var index = 1;

			do
			{
				destination = Path.Combine(directory, $"{name}_{index}{extension}");
				index++;
			}
			while (File.Exists(destination));

			return destination;
		}
	}
}
