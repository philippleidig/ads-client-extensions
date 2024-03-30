using System.IO;
using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class DirectoryExtensionsTests 
	{
		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory.Path, "*.*", SearchOption.AllDirectories);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory.Path, "*.*", SearchOption.AllDirectories);
				});
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory.Path, "*.*", SearchOption.AllDirectories);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnNullEntries_WhenDirectoryIsInvalid()
		{
			var directory = "NonExistingDirectory";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.*", SearchOption.AllDirectories);

				Assert.AreEqual(0, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnEntriesInAllDirectories()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory.Path, "*.*", SearchOption.AllDirectories);

				Assert.AreEqual(14, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnEntriesInBootFolderRecursive()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var actualEntries = Directory.EnumerateFileSystemEntries(boolFolder, "*", SearchOption.AllDirectories);

			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFileSystemEntriesAsync("", "*.*", SearchOption.AllDirectories, AdsDirectory.BootDir);

				Assert.AreEqual(actualEntries.Count(), files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnEntriesInBootFolder()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var actualEntries = Directory.EnumerateFileSystemEntries(boolFolder, "*", SearchOption.TopDirectoryOnly);

			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFileSystemEntriesAsync("", "*.*", SearchOption.TopDirectoryOnly, AdsDirectory.BootDir);

				Assert.AreEqual(actualEntries.Count(), files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnEntriesInTopDirectoryOnly()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory.Path, "*.*", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(5, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnFilteredEntriesInAllDirectories()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory.Path, "*.xml", SearchOption.AllDirectories);

				Assert.AreEqual(1, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnFilteredEntriesInTopDirectoryOnly()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory.Path, "*.xml", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(1, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInAllDirectories()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory.Path, "*.*", SearchOption.AllDirectories);

				Assert.AreEqual(9, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInBootFolderRecursive()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var actualFiles = Directory.EnumerateFiles(boolFolder, "*", SearchOption.AllDirectories);

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync("", "*.*", SearchOption.AllDirectories, AdsDirectory.BootDir);

				Assert.AreEqual(actualFiles.Count(), files.Count());
			}
		}


		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnFilteredEntriesInAllDirectories()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory.Path, "*.xml", SearchOption.AllDirectories);

				Assert.AreEqual(1, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInTopDirectoryOnly()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory.Path, "*.*", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(3, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInBootFolder()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var actualEntries = Directory.EnumerateFiles(boolFolder, "*", SearchOption.TopDirectoryOnly);

			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync("", "*.*", SearchOption.TopDirectoryOnly, AdsDirectory.BootDir);

				Assert.AreEqual(actualEntries.Count(), files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnFilteredEntriesInTopDirectoryOnly()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory.Path, "*.xml", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(1, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInAllDirectories()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync(directory.Path, "*.*", SearchOption.AllDirectories);

				Assert.AreEqual(5, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInBootFolderRecursive()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var actualDirectories = Directory.EnumerateDirectories(boolFolder, "*", SearchOption.AllDirectories);

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync("", "*.*", SearchOption.AllDirectories, AdsDirectory.BootDir);

				Assert.AreEqual(actualDirectories.Count(), directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnFilteredEntriesInAllDirectories()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync(directory.Path, "SubFolder_2", SearchOption.AllDirectories);

				Assert.AreEqual(1, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInTopDirectoryOnly()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync(directory.Path, "*.*", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(2, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInBootFolder()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var actualDirectories = Directory.EnumerateDirectories(boolFolder, "*", SearchOption.TopDirectoryOnly);

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync("", "*.*", SearchOption.TopDirectoryOnly, AdsDirectory.BootDir);

				Assert.AreEqual(actualDirectories.Count(), directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnFilteredEntriesInTopDirectoryOnly()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync(directory.Path, "WorkingDirectory", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(0, directories.Count());
			}
		}
	}
}