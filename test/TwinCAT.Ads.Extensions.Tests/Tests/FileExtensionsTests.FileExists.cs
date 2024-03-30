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
		public async Task FileExistsAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var fileExists = await adsClient.FileExistsAsync(file.Path);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					var fileExists = await adsClient.FileExistsAsync(file.Path);
				});
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var fileExists = await adsClient.FileExistsAsync(file.Path);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					var fileExists = await adsClient.FileExistsAsync("");
				});
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var fileExistsAds = await adsClient.FileExistsAsync(file.Path);

				var fileExists = File.Exists(file.Path);

				Assert.AreEqual(fileExists, fileExistsAds);
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldReturnFalse_WhenFileDoesNotExist()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var fileExists = await adsClient.FileExistsAsync("TestFileDoesNotExist.txt");

				Assert.IsFalse(fileExists);
			}
		}
	}
}
