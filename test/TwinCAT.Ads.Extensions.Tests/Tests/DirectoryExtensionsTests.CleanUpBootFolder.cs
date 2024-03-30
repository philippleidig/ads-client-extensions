using System.IO;
using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class DirectoryExtensionsTests 
	{
		[TestMethod]
		public async Task CleanUpBootFolderAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.CleanUpBootFolderAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task CleanUpBootFolderAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.CleanUpBootFolderAsync();
				});
			}
		}

		[TestMethod]
		public async Task CleanUpBootFolderAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.CleanUpBootFolderAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task CleanUpBootFolderAsync_ShouldRemoveAllFilesRecursive()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.CleanUpBootFolderAsync();

				var files = Directory.EnumerateFileSystemEntries(boolFolder);

				Assert.AreEqual(0, files.Count());
			}
		}
	}
}