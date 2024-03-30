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
		public async Task RenameFileAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameFileAsync(file.Path, "TestRenameFileNew.tmp");
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.RenameFileAsync(file.Path, "TestRenameFileNew.tmp");
				});
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameFileAsync(file.Path, "TestRenameFileNew.tmp");
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.RenameFileAsync("", "TestRenameFileNew.tmp");
				});
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowDeviceNotFoundException_WhenFileDoesNotExist()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameFileAsync("DoesNotExist.tmp", "TestRenameFileNew.tmp");
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.DeviceNotFound);
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowIOException_WhenFileExtensionDiffers()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<IOException>(async () => {
					await adsClient.RenameFileAsync(file.Path, "TestRenameFileNew.xml");
				});
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldRenameFile()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.RenameFileAsync(file.Path, "TestRenameFileNew.tmp");

				var fileExistsOld = File.Exists(file.Path);
				var fileExistsNew = File.Exists(Path.Combine(Path.GetDirectoryName(file.Path), "TestRenameFileNew.tmp"));

				Assert.IsFalse(fileExistsOld);
				Assert.IsTrue(fileExistsNew);
			}
		}
	}
}
