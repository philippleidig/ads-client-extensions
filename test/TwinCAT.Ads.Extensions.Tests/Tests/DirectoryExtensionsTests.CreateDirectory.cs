using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class DirectoryExtensionsTests 
	{
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
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var folderName = Guid.NewGuid().ToString();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.CreateDirectoryInBootFolderAsync(folderName);
			}

			var path = Path.Combine(boolFolder, folderName);
			var isCreated = Directory.Exists(path);

			Directory.Delete(path);

			Assert.IsTrue(isCreated);
		}
	}
}