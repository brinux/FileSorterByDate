namespace FileSorterByDate
{
	internal class TimeMetadataConfig
	{
		public string Name { get; }
		public MetadataTimeKind Kind { get; }
		public bool CanWrite { get; }

		public TimeMetadataConfig(string name, MetadataTimeKind kind, bool canWrite = true)
		{
			Name = name;
			Kind = kind;
			CanWrite = canWrite;
		}
	}
}
