using System.Globalization;
using System.Text.RegularExpressions;

namespace FileSorterByDate
{
	internal class Program
	{
		//const string sourcePath = @"C:\Users\FabrizioZanette\OneDrive\Immagini\Camera Uploads";
		//const string sourcePath = @"C:\Users\FabrizioZanette\OneDrive\Immagini\Rullino";
		const string sourcePath = @"C:\Immagini";
		const string baseTargetPath = @"C:\ImmaginiTarget";
		const int maxTimeGapSeconds = 60;
		const ExcessiveTimeGapReactionEnum excessiveGapReaction = ExcessiveTimeGapReactionEnum.ask;
		static readonly bool testMode = false;
		static readonly bool ignoreSystemFileCreateDate = true;
		/// <summary>
		/// Ignore metadata timestamps earlier than this year. File-name dates are always kept.
		/// Set to 0 (or less) to disable.
		/// </summary>
		static readonly int ignoreMetadataBeforeYear = 2010;

		private static readonly TimeSpan MaxTimeGap = TimeSpan.FromSeconds(maxTimeGapSeconds);

		private static readonly SortConfiguration[] configs =
		[
			new SortConfiguration(
				"Camera images",
				Path.Combine(baseTargetPath, @"Immagini\Rullino"),
				[
					FileNameRegexes.StandardDateAndTime(),
					FileNameRegexes.DateAndTimeSeparatedByUnderscore(),
					FileNameRegexes.WindowsPhoneDatesWithTime(),
					FileNameRegexes.WindowsPhoneDatesWithoutTime(),
				],
				[
					new SortFileType(".jpg", SorterFileTypesEnum.Image),
					new SortFileType(".jpeg", SorterFileTypesEnum.Image),
					new SortFileType(".png", SorterFileTypesEnum.Image),
				]
			),
			new SortConfiguration(
				"Screenshots",
				Path.Combine(baseTargetPath, @"Immagini\Screenshots"),
				[
					FileNameRegexes.ScreenshotsDateAndTimeFieldsDividedByDashes(),
					FileNameRegexes.ScreenshotsDateAndTimeDividedByDash(),
				],
				[
					new SortFileType(".jpg", SorterFileTypesEnum.Image),
					new SortFileType(".jpeg", SorterFileTypesEnum.Image),
					new SortFileType(".png", SorterFileTypesEnum.Image),
				]
			),
			new SortConfiguration(
				"Whatsapp images",
				Path.Combine(baseTargetPath, @"Immagini\WhatsApp Images"),
				[
					FileNameRegexes.WhatsappImages(),
				],
				[
					new SortFileType(".jpg", SorterFileTypesEnum.Image),
					new SortFileType(".jpeg", SorterFileTypesEnum.Image),
					new SortFileType(".png", SorterFileTypesEnum.Image),
				]
			),
			new SortConfiguration(
				"Whatsapp videos",
				Path.Combine(baseTargetPath, @"Immagini\WhatsApp Videos"),
				[
					FileNameRegexes.WhatsappVideos(),
				],
				[
					new SortFileType(".mp4", SorterFileTypesEnum.Video),
				]
			),
			new SortConfiguration(
				"Videos",
				Path.Combine(baseTargetPath, @"Immagini\Videos"),
				[
					FileNameRegexes.VideosWithDateAndTime(),
					FileNameRegexes.StandardDateAndTime(),
					FileNameRegexes.DateAndTimeSeparatedByUnderscore(),
				],
				[
					new SortFileType(".mp4", SorterFileTypesEnum.Video),
				]
			)
		];

		static int Main(string[] args)
		{
			if (!TimeZoneArgumentParser.TryParse(args, out var defaultTimeZone, out var timeZoneError))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(timeZoneError);
				Console.ResetColor();
				return 1;
			}

			if (!Directory.Exists(sourcePath))
			{
				Console.WriteLine($"Source folder '{sourcePath}' does not exist.");
				Console.WriteLine();
				return 1;
			}

			Console.WriteLine($"Default time zone: {defaultTimeZone.Id} (used when a file has no offset or only +00:00).");
			Console.WriteLine();

			using var exiftool = new ExifToolClient();
			ProcessFolder(sourcePath, exiftool, defaultTimeZone);
			return 0;
		}

		private static void ProcessFolder(string folder, ExifToolClient exiftool, TimeZoneInfo defaultTimeZone)
		{
			Console.WriteLine($"Processing folder: {folder}");
			Console.WriteLine();

			foreach (var path in Directory.GetFiles(folder))
			{
				ProcessFile(path, exiftool, defaultTimeZone);
			}

			foreach (var path in Directory.GetDirectories(folder))
			{
				ProcessFolder(path, exiftool, defaultTimeZone);
			}
		}

		private static void ProcessFile(string filePath, ExifToolClient exiftool, TimeZoneInfo defaultTimeZone)
		{
			Console.Write("Processing file: ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(filePath);
			Console.ResetColor();

			try
			{
				var matched = false;
				foreach (var config in configs)
				{
					if (ProcessFileForSpecificConfig(filePath, config, exiftool, defaultTimeZone))
					{
						matched = true;
						break;
					}
				}

				if (!matched)
				{
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.WriteLine("# The file didn't match any configuration and will be skipped.");
					Console.ResetColor();
				}
			}
			catch (OperationCanceledException)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("The file was skipped.");
				Console.ResetColor();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("! An error occurred while processing the file and it will be skipped.");

				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine(e.Message);
				Console.ResetColor();
			}

			Console.WriteLine();
		}

		private static bool ProcessFileForSpecificConfig(
			string filePath,
			SortConfiguration config,
			ExifToolClient exiftool,
			TimeZoneInfo defaultTimeZone)
		{
			var extension = Path.GetExtension(filePath).ToLowerInvariant();
			if (!config.Extensions.Any(e => e.Extension == extension))
			{
				return false;
			}

			foreach (var pattern in config.Patterns)
			{
				if (ProcessFileForSpecificConfigAndPattern(filePath, extension, config, pattern, exiftool, defaultTimeZone))
				{
					return true;
				}
			}

			return false;
		}

		private static bool ProcessFileForSpecificConfigAndPattern(
			string filePath,
			string extension,
			SortConfiguration config,
			Regex pattern,
			ExifToolClient exiftool,
			TimeZoneInfo defaultTimeZone)
		{
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
			var match = pattern.Match(fileNameWithoutExtension);
			if (!FileNameTimestampParser.TryParse(match, defaultTimeZone, out var timeReference))
			{
				return false;
			}

			Console.WriteLine($"- Pattern recognized for configuration '{config.Name}'.");

			var fileType = config.Extensions.Single(e => e.Extension == extension).Type;
			timeReference = SynchronizeMetadata(timeReference, filePath, fileType, exiftool, defaultTimeZone);
			UpdateFileTimes(filePath, timeReference.LocalDateTime);
			MoveFile(filePath, config.Destination, timeReference.LocalDateTime);
			return true;
		}

		private static DateTimeOffset SynchronizeMetadata(
			DateTimeOffset fileNameTime,
			string filePath,
			SorterFileTypesEnum fileType,
			ExifToolClient exiftool,
			TimeZoneInfo defaultTimeZone)
		{
			var metadata = exiftool.ExtractAllMetadata(filePath);
			var selection = SortPipeline.SelectTimestamp(
				fileNameTime,
				metadata,
				fileType,
				MaxTimeGap,
				excessiveGapReaction,
				defaultTimeZone,
				ignoreSystemFileCreateDate,
				ignoreMetadataBeforeYear);

			WriteCandidateSummary(selection);

			var pickedTime = selection.RequiresUserChoice
				? AskTheUserToSelectTimeReference(selection.Candidates, defaultTimeZone)
				: selection.Selected;

			SaveFileMetadata(pickedTime, filePath, fileType, exiftool);
			return pickedTime;
		}

		private static void WriteCandidateSummary(TimeSelectionResult selection)
		{
			if (selection.Candidates.Count == 1)
			{
				Console.WriteLine($"- Only one time reference recognized: {selection.Candidates[0].ToDisplayString()}.");
				return;
			}

			Console.WriteLine("- Multiple time references recognized:");
			var earliest = selection.Candidates.Min(c => c.Value);
			for (var i = 0; i < selection.Candidates.Count; i++)
			{
				var candidate = selection.Candidates[i];
				var gap = (candidate.Value - earliest).TotalSeconds;
				var gapLabel = gap == 0 ? "earliest time" : $"+{gap} seconds";
				Console.WriteLine($"{i + 1}: {candidate.ToDisplayString()}. [{gapLabel}]");
			}
		}

		private static void SaveFileMetadata(
			DateTimeOffset timestamp,
			string filePath,
			SorterFileTypesEnum fileType,
			ExifToolClient exiftool)
		{
			var tags = SortPipeline.BuildTags(timestamp, fileType);
			var utcTimestamp = ExifDateFormats.FormatUtc(timestamp);
			var localTimestamp = ExifDateFormats.FormatWithOffset(timestamp).Replace("+", " +", StringComparison.Ordinal);

			Console.Write("Selected time reference: ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"{utcTimestamp} UTC / {localTimestamp}");
			Console.ResetColor();

			foreach (var tag in tags)
			{
				Console.WriteLine($"* {tag.Key} => {tag.Value}");
			}

			if (testMode)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"# Metadata won't be set to '{utcTimestamp} UTC / {localTimestamp}' [TEST-MODE ON].");
				Console.ResetColor();
				return;
			}

			try
			{
				exiftool.WriteTags(filePath, tags);
				Console.WriteLine($"- Time references in metadata set to '{utcTimestamp}' UTC / '{localTimestamp}'.");
			}
			catch (Exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"! An error occurred while trying to save metadata to '{utcTimestamp}'.");
				Console.ResetColor();
				throw;
			}
		}

		private static void UpdateFileTimes(string filePath, DateTime timestamp)
		{
			try
			{
				if (testMode)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"# File creation, access and modification times won't be updated to {timestamp} [TEST-MODE ON].");
					Console.ResetColor();
					return;
				}

				File.SetCreationTime(filePath, timestamp);
				File.SetLastWriteTime(filePath, timestamp);
				File.SetLastAccessTime(filePath, timestamp);
				Console.WriteLine($"- File creation, access and modification times set to '{timestamp.ToString(ExifDateFormats.WithoutOffset, CultureInfo.InvariantCulture)}'.");
			}
			catch (Exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"- An error occurred while trying to set file creation, access and modification times to '{timestamp.ToString(ExifDateFormats.WithoutOffset, CultureInfo.InvariantCulture)}'.");
				Console.ResetColor();
				throw;
			}
		}

		private static void MoveFile(string sourceFile, string destinationFolder, DateTime timeRef)
		{
			var fileName = Path.GetFileName(sourceFile);
			var targetPath = DestinationPathBuilder.Build(destinationFolder, timeRef);

			if (testMode)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"# The file won't be moved to target folder '{targetPath}'. [TEST-MODE ON]");
				Console.ResetColor();
				return;
			}

			if (!Directory.Exists(targetPath))
			{
				Directory.CreateDirectory(targetPath);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"# Target folder '{targetPath}' didn't exist and was created.");
				Console.ResetColor();
			}

			try
			{
				var destinationFile = UniqueFileName.GetAvailablePath(targetPath, fileName);
				File.Move(sourceFile, destinationFile);
				Console.WriteLine($"- The file was moved to '{destinationFile}'.");
			}
			catch (Exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"! An error occurred while trying to move the file to its target folder '{targetPath}'.");
				Console.ResetColor();
				throw;
			}
		}

		private static DateTimeOffset AskTheUserToSelectTimeReference(
			IReadOnlyList<TimeReference> referenceTimes,
			TimeZoneInfo defaultTimeZone)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("The file has time references that differ too much to automatically select one.");
			Console.ResetColor();

			while (true)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine($"You can pick a time reference among the listed ones (1-{referenceTimes.Count}),");
				Console.WriteLine("skip the file (S), or manually provide a different reference (M).");
				Console.Write("Please select: ");
				Console.ResetColor();

				var input = Console.ReadLine()?.Trim();

				if (string.Equals(input, "S", StringComparison.OrdinalIgnoreCase))
				{
					throw new OperationCanceledException();
				}

				if (string.Equals(input, "M", StringComparison.OrdinalIgnoreCase))
				{
					if (TryAskManualTime(defaultTimeZone, out var manual))
					{
						return manual;
					}

					continue;
				}

				if (int.TryParse(input, out var choice) &&
					choice >= 1 &&
					choice <= referenceTimes.Count)
				{
					return referenceTimes[choice - 1].Value;
				}
			}
		}

		private static bool TryAskManualTime(TimeZoneInfo defaultTimeZone, out DateTimeOffset userReference)
		{
			userReference = default;

			while (true)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write("Please insert a new time (eg. 2000:12:31 23:59:59+01:00), or 'C' to cancel: ");
				Console.ResetColor();

				var input = Console.ReadLine()?
					.Trim()
					.Replace(" +", "+", StringComparison.Ordinal)
					.Replace(" -", "-", StringComparison.Ordinal);

				if (string.Equals(input, "C", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				if (ExifDateTimeParser.TryParse(input, MetadataTimeKind.WithOffset, defaultTimeZone, out userReference, out _))
				{
					return true;
				}

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write("Invalid format. ");
			}
		}
	}
}
