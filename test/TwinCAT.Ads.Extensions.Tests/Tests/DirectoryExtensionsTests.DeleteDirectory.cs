using System.IO;
using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class DirectoryExtensionsTests 
	{
		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteDirectoryAsync(directory.Path);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.DeleteDirectoryAsync(directory.Path);
				});
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteDirectoryAsync(directory.Path);
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
					await adsClient.DeleteDirectoryAsync(directory);
				});
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldRemoveDirectory()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.DeleteDirectoryAsync(directory.Path, true);

				var isDeleted = !Directory.Exists(directory.Path);

				Assert.IsTrue(isDeleted);
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldRemoveDirectoryRecursive()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.DeleteDirectoryAsync(directory.Path, true);

				var isDeleted = !Directory.Exists(directory.Path);

				Assert.IsTrue(isDeleted);
			}
		}

		[TestMethod]
		public async Task RemoveDirectoryAsync_ShouldRemoveDirectoryInBootFolder()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var folderName = Guid.NewGuid().ToString();
			var folder = Path.Combine(boolFolder, folderName);

			Directory.CreateDirectory(folder);

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.DeleteDirectoryInBootFolderAsync(folderName);

				var isDeleted = !Directory.Exists(folder);

				Assert.IsTrue(isDeleted);
			}
		}
	}
}