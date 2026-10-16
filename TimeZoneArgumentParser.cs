namespace FileSorterByDate
{
	internal static class TimeZoneArgumentParser
	{
		public static bool TryParse(IReadOnlyList<string> args, out TimeZoneInfo timeZone, out string? error)
		{
			timeZone = TimeZoneSettings.Resolve(TimeZoneSettings.DefaultTimeZoneId);
			error = null;

			for (var i = 0; i < args.Count; i++)
			{
				var arg = args[i];
				string? id = null;

				if (arg.Equals("--timezone", StringComparison.OrdinalIgnoreCase) ||
					arg.Equals("-tz", StringComparison.OrdinalIgnoreCase))
				{
					if (i + 1 >= args.Count)
					{
						error = "Missing value for --timezone. Example: --timezone Europe/Rome";
						return false;
					}

					id = args[++i];
				}
				else if (arg.StartsWith("--timezone=", StringComparison.OrdinalIgnoreCase))
				{
					id = arg["--timezone=".Length..];
				}

				if (id is null)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(id))
				{
					error = "The --timezone value cannot be empty.";
					return false;
				}

				try
				{
					timeZone = TimeZoneSettings.Resolve(id);
					return true;
				}
				catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
				{
					error = $"Unknown time zone '{id}'. Use an IANA id such as Europe/Rome or a Windows id such as \"W. Europe Standard Time\".";
					return false;
				}
			}

			return true;
		}
	}
}
