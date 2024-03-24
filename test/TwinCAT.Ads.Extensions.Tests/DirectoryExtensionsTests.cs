using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	[TestClass]
	public class DirectoryExtensionsTests 
	{
		[TestInitialize]
		public void InitializeWorkingDirectory()
		{
			Directory.CreateDirectory(WorkingDirectory);

			var filePath = Path.Combine(WorkingDirectory, "WorkingDirectoryFile_1.txt");
			File.Create(filePath).Dispose();

			filePath = Path.Combine(WorkingDirectory, "WorkingDirectoryFile_2.txt");
			File.Create(filePath).Dispose();

			filePath = Path.Combine(WorkingDirectory, "WorkingDirectoryFile_3.xml");
			File.Create(filePath).Dispose();

			var subDirectory = Path.Combine(WorkingDirectory, "SubDirectory");
			Directory.CreateDirectory(subDirectory);

			filePath = Path.Combine(subDirectory, "SubDirectoryFile_1.txt");
			File.Create(filePath).Dispose();

			filePath = Path.Combine(subDirectory, "SubDirectoryFile_2.txt");
			File.Create(filePath).Dispose();

			filePath = Path.Combine(subDirectory, "SubDirectoryFile_3.xml");
			File.Create(filePath).Dispose();

			var subSubDirectory = Path.Combine(subDirectory, "SubSubDirectory");
			Directory.CreateDirectory(subSubDirectory);

			filePath = Path.Combine(subSubDirectory, "SubSubDirectoryFile_1.txt");
			File.Create(filePath).Dispose();

			filePath = Path.Combine(subSubDirectory, "SubSubDirectoryFile_2.txt");
			File.Create(filePath).Dispose();
		}

		[TestCleanup]
		public void CleanUpWorkingDirectory()
		{
			Directory.Delete(WorkingDirectory, true);
		}

		[TestMethod]
		public async Task CreateDirectoryAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestCreateDirectory");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.CreateDirectoryAsync(directory);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task CreateDirectoryAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestCreateDirectory");

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.CreateDirectoryAsync(directory);
				});
			}
		}

		[TestMethod]
		public async Task CreateDirectoryAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestCreateDirectory");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.CreateDirectoryAsync(directory);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task CreateDirectoryAsync_ShouldThrowArgumentNullException_WhenDirectoryPathIsInvalid()
		{
			var directory = "";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.CreateDirectoryAsync(directory);
				});
			}
		}

		[TestMethod]
		public async Task CreateDirectoryAsync_ShouldCreateDirectory()
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestCreateDirectory");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.CreateDirectoryAsync(directory);
			}

			var isCreated = Directory.Exists(directory);
			Directory.Delete(directory);

			Assert.IsTrue(isCreated);
		}

		[TestMethod]
		public async Task CreateDirectoryAsync_ShouldCreateDirectoryInBootFolder()
		{
			var directory = "TestCreateDirectory";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.CreateDirectoryInBootFolderAsync(directory);
			}

			var isCreated = Directory.Exists(directory);
			Directory.Delete(directory);

			Assert.IsTrue(isCreated);
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestRemoveDirectory");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RemoveDirectoryAsync(directory);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestRemoveDirectory");

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.RemoveDirectoryAsync(directory);
				});
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestRemoveDirectory");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RemoveDirectoryAsync(directory);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldThrowArgumentNullException_WhenDirectoryIsInvalid()
		{
			var directory = "";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.RemoveDirectoryAsync(directory);
				});
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldRemoveDirectory()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.RemoveDirectoryAsync(directory, false);
			}

			var isDeleted = !Directory.Exists(directory);

			Assert.IsTrue(isDeleted);
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldRemoveDirectoryRecursive()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.RemoveDirectoryAsync(directory, true);
			}

			var isDeleted = !Directory.Exists(directory);

			Assert.IsTrue(isDeleted);
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldRemoveDirectoryInBootFolder()
		{
			var directory = WorkingDirectory;

			Directory.CreateDirectory(directory);

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.RemoveDirectoryInBootFolderAsync(directory);
			}

			var isDeleted = !Directory.Exists(directory);

			Assert.IsTrue(isDeleted);
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.*", SearchOption.AllDirectories);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.*", SearchOption.AllDirectories);
				});
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.*", SearchOption.AllDirectories);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldThrowArgumentNullException_WhenDirectoryIsInvalid()
		{
			var directory = "NonExistingDirectory";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.*", SearchOption.AllDirectories);
				});
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnEntriesInAllDirectories()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.*", SearchOption.AllDirectories);

				Assert.AreEqual(10, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnEntriesInTopDirectoryOnly()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.*", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(4, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnFilteredEntriesInAllDirectories()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.xml", SearchOption.AllDirectories);

				Assert.AreEqual(1, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFileSystemEntriesAsync_ShouldReturnFilteredEntriesInTopDirectoryOnly()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var entries = await adsClient.EnumerateFileSystemEntriesAsync(directory, "*.xml", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(2, entries.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInAllDirectories()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory, "*.*", SearchOption.AllDirectories);

				Assert.AreEqual(8, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInBootFolderRecursive()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync("", "*.*", SearchOption.AllDirectories, AdsDirectory.BootDir);

				Assert.AreEqual(8, files.Count());
			}
		}


		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnFilteredEntriesInAllDirectories()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory, "*.xml", SearchOption.AllDirectories);

				Assert.AreEqual(1, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInTopDirectoryOnly()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory, "*.*", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(3, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnEntriesInBootFolder()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync("", "*.*", SearchOption.TopDirectoryOnly, AdsDirectory.BootDir);

				Assert.AreEqual(3, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnFilteredEntriesInTopDirectoryOnly()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var files = await adsClient.EnumerateFilesAsync(directory, "*.xml", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(1, files.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInAllDirectories()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync(directory, "*.*", SearchOption.AllDirectories);

				Assert.AreEqual(2, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInBootFolderRecursive()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync("", "*.*", SearchOption.AllDirectories, AdsDirectory.BootDir);

				Assert.AreEqual(2, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnFilteredEntriesInAllDirectories()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync(directory, "SubSubDirectory", SearchOption.AllDirectories);

				Assert.AreEqual(1, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInTopDirectoryOnly()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync("", "*.*", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(1, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnEntriesInBootFolder()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync("", "*.*", SearchOption.TopDirectoryOnly, AdsDirectory.BootDir);

				Assert.AreEqual(1, directories.Count());
			}
		}

		[TestMethod]
		public async Task EnumerateDirectoriesAsync_ShouldReturnFilteredEntriesInTopDirectoryOnly()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var directories = await adsClient.EnumerateDirectoriesAsync(directory, "WorkingDirectory", SearchOption.TopDirectoryOnly);

				Assert.AreEqual(0, directories.Count());
			}
		}


		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.ExistsDirectoryAsync(directory);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.ExistsDirectoryAsync(directory);
				});
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.ExistsDirectoryAsync(directory);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowArgumentNullException_WhenDirectoryPathIsInvalid()
		{
			var directory = "";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.ExistsDirectoryAsync(directory);
				});
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldReturnFalseWhenDirectoryDoesNotExist()
		{
			var directory = @"NonExistingDirectory";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var exists = await adsClient.ExistsDirectoryAsync(directory);

				Assert.IsFalse(exists);
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldReturnTrueWhenDirectoryExists()
		{
			var directory = WorkingDirectory;

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var exists = await adsClient.ExistsDirectoryAsync(directory);

				Assert.IsTrue(exists);
			}		
		}
	}
}