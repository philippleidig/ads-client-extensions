using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	[TestClass]
	public class FileExtensionsTests
	{
		[TestMethod]
		public async Task FileExistsAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestFileExists.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var fileExists = await adsClient.FileExistsAsync(file);
				});

				File.Delete(file);

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestFileExists.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					var fileExists = await adsClient.FileExistsAsync(file);
				});

				File.Delete(file);
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestFileExists.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					var fileExists = await adsClient.FileExistsAsync(file);
				});

				File.Delete(file);

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			var file = "";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					var fileExists = await adsClient.FileExistsAsync(file);
				});
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestFileExists.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var fileExistsAds = await adsClient.FileExistsAsync(file);

				var fileExists = File.Exists(file);

				File.Delete(file);

				Assert.AreEqual(fileExists, fileExistsAds);
			}
		}

		[TestMethod]
		public async Task FileExistsAsync_ShouldReturnFalse_WhenFileDoesNotExist()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestFileDoesNotExist.txt");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var fileExists = await adsClient.FileExistsAsync(file);

				Assert.IsFalse(fileExists);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestDeleteFile.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteFileAsync(file);
				});

				File.Delete(file);

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestDeleteFile.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.DeleteFileAsync(file);
				});

				File.Delete(file);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestDeleteFile.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteFileAsync(file);
				});

				File.Delete(file);

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			var file = "";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.DeleteFileAsync(file);
				});
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldThrowDeviceNotFoundException_WhenFileDoesNotExist()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "DoesNotExist.txt");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.DeleteFileAsync(file);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.DeviceNotFound);
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldDeleteFile()
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "TestDeleteFile.txt");
			File.Create(file).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.DeleteFileAsync(file);

				var fileExists = File.Exists(file);

				Assert.IsFalse(fileExists);
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var fileOld = Path.Combine(Directory.GetCurrentDirectory(), "TestRenameFile.txt");
			File.Create(fileOld).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameFileAsync(fileOld, "TestRenameFileNew.txt");
				});

				File.Delete(fileOld);

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var fileOld = Path.Combine(Directory.GetCurrentDirectory(), "TestRenameFile.txt");

			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					await adsClient.RenameFileAsync(fileOld, "TestRenameFileNew.txt");
				});
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			var fileOld = Path.Combine(Directory.GetCurrentDirectory(), "TestRenameFile.txt");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameFileAsync(fileOld, "TestRenameFileNew.txt");
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			var fileOld = "";

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
					await adsClient.RenameFileAsync(fileOld, "TestRenameFileNew.txt");
				});
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowDeviceNotFoundException_WhenFileDoesNotExist()
		{
			var fileOld = Path.Combine(Directory.GetCurrentDirectory(), "DoesNotExist.txt");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					await adsClient.RenameFileAsync(fileOld, "TestRenameFileNew.txt");
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.DeviceNotFound);
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldThrowIOException_WhenFileExtensionDiffers()
		{
			var fileOld = Path.Combine(Directory.GetCurrentDirectory(), "TestRenameFile.txt");

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<IOException>(async () => {
					await adsClient.RenameFileAsync(fileOld, "TestRenameFileNew.xml");
				});
			}
		}

		[TestMethod]
		public async Task RenameFileAsync_ShouldRenameFile()
		{
			var fileOld = Path.Combine(Directory.GetCurrentDirectory(), "TestRenameFile.txt");
			var fileNew = Path.Combine(Directory.GetCurrentDirectory(), "TestRenameFileNew.txt");
			File.Create(fileOld).Dispose();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.RenameFileAsync(fileOld, "TestRenameFileNew.txt");

				var fileExistsOld = File.Exists(fileOld);
				var fileExistsNew = File.Exists(fileNew);

				Assert.IsFalse(fileExistsOld);
				Assert.IsTrue(fileExistsNew);
			}
		}
	}
}
