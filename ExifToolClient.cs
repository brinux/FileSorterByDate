using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FileSorterByDate
{
	internal sealed class ExifToolClient : IDisposable
	{
		private const int ExitTimeoutMs = 15000;
		private readonly Encoding _utf8NoBom = new UTF8Encoding(false);
		private readonly Process _process;
		private readonly StreamWriter _writer;
		private readonly StreamReader _reader;
		private readonly StringBuilder _stderr = new();
		private bool _disposed;

		public ExifToolClient(string? exifToolPath = null)
		{
			var binary = ResolveBinaryPath(exifToolPath);
			var startInfo = new ProcessStartInfo(binary)
			{
				Arguments = "-stay_open 1 -@ - -common_args -charset UTF8 -G1 -args -api QuickTimeUTC=1",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				StandardOutputEncoding = _utf8NoBom,
				StandardErrorEncoding = _utf8NoBom,
			};

			_process = Process.Start(startInfo)
				?? throw new InvalidOperationException("Failed to launch ExifTool.");

			_writer = new StreamWriter(_process.StandardInput.BaseStream, _utf8NoBom);
			_reader = _process.StandardOutput;
			_process.ErrorDataReceived += (_, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					lock (_stderr)
					{
						_stderr.AppendLine(e.Data);
					}
				}
			};
			_process.BeginErrorReadLine();
		}

		public IReadOnlyDictionary<string, string> ExtractAllMetadata(string filePath)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);

			Execute(filePath);

			var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var line in ReadUntilReady())
			{
				if (line.Length == 0 || line[0] != '-')
				{
					continue;
				}

				var separator = line.IndexOf('=');
				if (separator <= 1)
				{
					continue;
				}

				var key = line[1..separator];
				var value = line[(separator + 1)..].Trim();
				metadata.TryAdd(key, value);
			}

			return metadata;
		}

		public void WriteTags(string filePath, IReadOnlyDictionary<string, string> tags)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);

			var commands = new List<string>(tags.Count + 3)
			{
				"-m",
				"-overwrite_original"
			};

			foreach (var tag in tags)
			{
				commands.Add($"-{tag.Key}={tag.Value}");
			}

			commands.Add(filePath);
			Execute(commands);

			var output = string.Join(Environment.NewLine, ReadUntilReady());
			if (output.Contains("Error", StringComparison.OrdinalIgnoreCase) &&
				!output.Contains("files updated", StringComparison.OrdinalIgnoreCase) &&
				!output.Contains("files created", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException($"ExifTool failed to write metadata:{Environment.NewLine}{output}");
			}
		}

		private void Execute(params string[] args) => Execute((IEnumerable<string>)args);

		private void Execute(IEnumerable<string> args)
		{
			foreach (var arg in args)
			{
				_writer.WriteLine(arg);
			}

			_writer.WriteLine("-execute");
			_writer.Flush();
		}

		private List<string> ReadUntilReady()
		{
			var lines = new List<string>();
			while (true)
			{
				var line = _reader.ReadLine();
				if (line is null)
				{
					throw new InvalidOperationException("ExifTool process ended unexpectedly.");
				}

				if (line.StartsWith("{ready", StringComparison.Ordinal))
				{
					break;
				}

				lines.Add(line);
			}

			return lines;
		}

		private static string ResolveBinaryPath(string? exifToolPath)
		{
			if (!string.IsNullOrWhiteSpace(exifToolPath))
			{
				if (!File.Exists(exifToolPath))
				{
					throw new FileNotFoundException("ExifTool not found.", exifToolPath);
				}

				return exifToolPath;
			}

			var directory = AppContext.BaseDirectory;
			var binary = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? Path.Combine(directory, "ExifTool.Win", "exiftool.exe")
				: Path.Combine(directory, "ExifTool.Unix", "exiftool");

			if (!File.Exists(binary))
			{
				throw new FileNotFoundException("ExifTool not found.", binary);
			}

			return binary;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			try
			{
				if (!_process.HasExited)
				{
					_writer.WriteLine("-stay_open");
					_writer.WriteLine("0");
					_writer.WriteLine("-execute");
					_writer.Flush();

					if (!_process.WaitForExit(ExitTimeoutMs))
					{
						_process.Kill(entireProcessTree: true);
					}
				}
			}
			catch
			{
				try
				{
					if (!_process.HasExited)
					{
						_process.Kill(entireProcessTree: true);
					}
				}
				catch
				{
					// ignored
				}
			}
			finally
			{
				_reader.Dispose();
				_writer.Dispose();
				_process.Dispose();
			}
		}
	}
}
