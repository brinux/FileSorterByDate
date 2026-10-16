namespace FileSorterByDate
{
	internal static class MetadataLookup
	{
		public static bool TryGetValue(
			IReadOnlyDictionary<string, string> metadata,
			string tagName,
			out string value)
		{
			if (TryGetExact(metadata, tagName, out value))
			{
				return true;
			}

			var shortName = ShortName(tagName);
			return !shortName.Equals(tagName, StringComparison.OrdinalIgnoreCase) &&
				TryGetExact(metadata, shortName, out value);
		}

		private static bool TryGetExact(
			IReadOnlyDictionary<string, string> metadata,
			string tagName,
			out string value)
		{
			if (metadata.TryGetValue(tagName, out value!))
			{
				return true;
			}

			foreach (var pair in metadata)
			{
				if (pair.Key.Equals(tagName, StringComparison.OrdinalIgnoreCase))
				{
					value = pair.Value;
					return true;
				}
			}

			value = string.Empty;
			return false;
		}

		private static string ShortName(string tagName)
		{
			var separator = tagName.LastIndexOf(':');
			return separator >= 0 ? tagName[(separator + 1)..] : tagName;
		}
	}
}
