using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class FileExtensionsTests
	{
		[TestMethod]
		public async Task CreateFileAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var file = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.CreateFileAsync(file);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task CreateFileAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var file = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.CreateFileAsync(file);
				});
			}
		}

		[TestMethod]
		public async Task CreateFileAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			var file = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.CreateFileAsync(file);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task CreateFileAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			var file = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.CreateFileAsync("");
				});
			}
		}


		[TestMethod]
		public async Task CreateFileAsync_ShouldCreateFile()
		{
			var file = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.CreateFileAsync(file);

				var fileExists = File.Exists(file);
				File.Delete(file);

				Assert.IsTrue(fileExists);
			}
		}

		[TestMethod]
		public async Task CreateFileAsync_ShouldCreateFile_WhenDestinationAlreadyExists()
		{
			var file = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.CreateFileAsync(file);

				var fileExists = File.Exists(file);
				File.Delete(file);

				Assert.IsTrue(fileExists);
			}
		}

		[TestMethod]
		public async Task CreateFileAsync_ShouldCreateFileInBootFolder()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var fileName = Path.GetFileName(Path.GetTempFileName());
			var path = Path.Combine(boolFolder, fileName);

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.CreateFileAsync(fileName, false, AdsDirectory.BootDir);

				var fileExists = File.Exists(path);

				File.Delete(path);

				Assert.IsTrue(fileExists);
			}
		}
	}
}
