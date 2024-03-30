using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class DirectoryExtensionsTests 
	{
		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.ExistsDirectoryAsync(directory.Path);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.ExistsDirectoryAsync(directory.Path);
				});
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.ExistsDirectoryAsync(directory.Path);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldThrowArgumentNullException_WhenDirectoryPathIsInvalid()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.ExistsDirectoryAsync("");
				});
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldReturnFalseWhenDirectoryDoesNotExist()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var exists = await adsClient.ExistsDirectoryAsync("NonExistingDirectory");

				Assert.IsFalse(exists);
			}
		}

		[TestMethod]
		public async Task ExistsDirectoryAsync_ShouldReturnTrueWhenDirectoryExists()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory(WorkingDirectory))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var exists = await adsClient.ExistsDirectoryAsync(directory.Path);

				Assert.IsTrue(exists);
			}		
		}
	}
}