using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Timers;

namespace NathanAlden.FirefoxAutoRefresh
{
	internal static class Program
	{
		private static readonly HashSet<string> _changedPaths = new HashSet<string>();
		private static readonly string _directory = ConfigurationManager.AppSettings["Directory"];
		private static readonly object _lockObject = new object();
		private static readonly ushort _port = UInt16.Parse(ConfigurationManager.AppSettings["Port"]);
		private static readonly Timer _timer = new Timer
			{
				AutoReset = false
			};
		private static readonly HashSet<FileSystemWatcher> _watchers = new HashSet<FileSystemWatcher>();
		private static TelnetClient _client;
		private static string[] _filters = ConfigurationManager.AppSettings["Filters"].Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
		private static string _host = ConfigurationManager.AppSettings["Host"];
		private static TimeSpan _refreshDelay = TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["RefreshDelayInMilliseconds"]));

		private static void Main()
		{
			if (!ValidateConfiguration())
			{
				return;
			}
			ConfigureTimer();
			AddWatchers();

			Console.WriteLine("Press ESC to exit");
			Console.WriteLine();
			Console.WriteLine("Monitoring changes in {0}", _directory);
			Console.WriteLine("Filters: {0}", String.Join(" ", _filters));
			Console.WriteLine();
			Console.WriteLine("Connecting to {0} port {1}...", _host, _port);

			_client = new TelnetClient(_host, _port, Connected, () => Ready(_watchers));
			_client.Connect();

			while (true)
			{
				ConsoleKeyInfo key = Console.ReadKey();

				if (key.Key != ConsoleKey.Escape)
				{
					continue;
				}

				break;
			}

			_client.Disconnect();

			foreach (FileSystemWatcher watcher in _watchers)
			{
				watcher.EnableRaisingEvents = false;
			}
		}

		private static bool ValidateConfiguration()
		{
			if (_directory == null || !Directory.Exists(_directory))
			{
				Console.WriteLine("Invalid directory.");
				return false;
			}

			if (_filters.Length == 0)
			{
				_filters = new[] { "*.*" };
			}
			if (String.IsNullOrWhiteSpace(_host))
			{
				_host = "127.0.0.1";
			}
			if (_refreshDelay < TimeSpan.Zero)
			{
				_refreshDelay = TimeSpan.Zero;
			}

			return true;
		}

		private static void ConfigureTimer()
		{
			_timer.Interval = _refreshDelay.TotalMilliseconds;
			_timer.Elapsed += TimerOnElapsed;
		}

		private static void AddWatchers()
		{
			foreach (string filter in _filters)
			{
				var watcher = new FileSystemWatcher(_directory, filter)
					{
						IncludeSubdirectories = true
					};

				watcher.Changed += WatcherOnChanged;
				watcher.Created += WatcherOnChanged;
				watcher.Deleted += WatcherOnChanged;
				watcher.Renamed += WatcherOnChanged;

				_watchers.Add(watcher);
			}
		}

		private static void Connected()
		{
			Console.WriteLine("Connected, waiting for reply...");
		}

		private static void Ready(IEnumerable<FileSystemWatcher> watchers)
		{
			Console.WriteLine("Firefox is ready to accept commands");

			foreach (FileSystemWatcher watcher in watchers)
			{
				watcher.EnableRaisingEvents = true;
			}
		}

		private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
		{
			_timer.Stop();
			_timer.Start();

			lock (_lockObject)
			{
				_changedPaths.Add(e.FullPath);
			}
		}

		private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
		{
			lock (_lockObject)
			{
				Console.WriteLine();
				foreach (string changedPath in _changedPaths)
				{
					Console.WriteLine(changedPath);
				}
				_client.WriteBrowserReload();
				Console.WriteLine("Browser refreshed");
			}
		}
	}
}