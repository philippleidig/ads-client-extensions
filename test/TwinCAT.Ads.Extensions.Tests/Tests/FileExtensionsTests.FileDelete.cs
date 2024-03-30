using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class FileExtensionsTests
	{
		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteFileAsync(file.Path);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.DeleteFileAsync(file.Path);
				});
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteFileAsync(file.Path);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.DeleteFileAsync("");
				});
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowDeviceNotFoundException_WhenFileDoesNotExist()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteFileAsync("DoesNotExist.txt");
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.DeviceNotFound);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldDeleteFile()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.DeleteFileAsync(file.Path);

				var fileExists = File.Exists(file.Path);

				Assert.IsFalse(fileExists);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldDeleteFileInBootFolder()
		{
			var boolFolder = Path.Combine(Environment.GetEnvironmentVariable("TWINCAT3DIR"), "Boot");
			var fileName = Path.GetTempFileName();

			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.DeleteFileAsync(fileName, AdsDirectory.BootDir);

				var fileExists = File.Exists(fileName);

				File.Delete(Path.Combine(boolFolder,fileName));

				Assert.IsFalse(fileExists);
			}
		}
	}
}
