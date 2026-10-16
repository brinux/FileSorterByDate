using System.Text.RegularExpressions;

namespace FileSorterByDate
{
    internal class SortConfiguration
	{
		public string Name { get; }
        public string Destination { get; }
        public Regex[] Patterns { get; }
        public SortFileType[] Extensions { get; }

		public SortConfiguration(string name, string destination, Regex[] patterns, SortFileType[] extensions)
		{
			Name = name;
            Destination = destination;
			Patterns = patterns;
			Extensions = extensions;
		}
	}
}