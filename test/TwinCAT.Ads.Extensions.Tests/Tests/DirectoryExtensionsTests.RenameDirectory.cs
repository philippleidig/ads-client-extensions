using System.IO;
using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class DirectoryExtensionsTests 
	{
		[TestMethod]
		public async Task RenameDirectoryAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameDirectoryAsync(directory.Path, "NewDirectoryName", false);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task RenameDirectoryAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.RenameDirectoryAsync(directory.Path, "NewDirectoryName", false);
				});
			}
		}

		[TestMethod]
		public async Task RenameDirectoryAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameDirectoryAsync(directory.Path, "NewDirectoryName", false);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task RenameDirectoryAsync_ShouldThrowArgumentNullException_WhenDirectoryPathIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.RenameDirectoryAsync("", "NewDirectoryName", false);
				});
			}
		}


		[TestMethod]
		public async Task RenameDirectoryAsync_ShouldThrowDeviceNotFoundException_WhenDirectoryDoesNotExist()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameDirectoryAsync("DirectoryDoesNotExist", "NewDirectoryName", false);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.DeviceNotFound);
			}
		}

		[TestMethod]
		public async Task RenameDirectoryAsync_ShouldRenameDirectory()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.RenameDirectoryAsync(directory.Path, "NewDirectoryName", false);

				var path = Path.GetDirectoryName(directory.Path);

				var exists = Directory.Exists(Path.Combine(path, "NewDirectoryName"));

				Directory.Delete(Path.Combine(path, "NewDirectoryName"));

				Assert.IsTrue(exists);
			}		
		}
	}
}