namespace FileSorterByDate
{
    internal class SortFileType
	{
		public string Extension { get; }
        public SorterFileTypesEnum Type { get; }

		public SortFileType(string extension, SorterFileTypesEnum type)
		{
			Extension = extension;
			Type = type;
		}
	}
}