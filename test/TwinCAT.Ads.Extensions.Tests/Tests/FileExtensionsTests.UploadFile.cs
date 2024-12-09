using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class FileExtensionsTests
	{
		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			var destination = Path.GetTempFileName();

			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.R0_NCSAF);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					await adsClient.UploadFileToTargetAsync(file.Path, destination, false);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			var destination = Path.GetTempFileName();

			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(
					async () =>
					{
						await adsClient.UploadFileToTargetAsync(file.Path, destination);
					}
				);
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetNotReachable()
		{
			var destination = Path.GetTempFileName();

			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					await adsClient.UploadFileToTargetAsync(file.Path, destination);
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldThrowArgumentNullException_WhenPathIsInvalid()
		{
			var destination = Path.GetTempFileName();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
				{
					await adsClient.UploadFileToTargetAsync("", destination);
				});
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldThrowDeviceNotFoundException_WhenSourceFileDoesNotExist()
		{
			var destination = Path.GetTempFileName();

			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
				{
					await adsClient.UploadFileToTargetAsync("DoesNotExist.tmp", destination);
				});
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldThrowIOException_WhenFileExtensionDiffers()
		{
			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<IOException>(async () =>
				{
					await adsClient.UploadFileToTargetAsync(file.Path, "Destination.txt");
				});
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldUploadEmptyFile()
		{
			var destination = Path.GetTempFileName();

			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.UploadFileToTargetAsync(file.Path, Path.GetFileName(destination));

				var fileExistsOld = File.Exists(file.Path);
				var fileExistsNew = File.Exists(destination);

				var sourceFileSize = new FileInfo(file.Path).Length;
				var destinationFileSize = new FileInfo(destination).Length;

				File.Delete(destination);

				Assert.IsTrue(fileExistsOld);
				Assert.IsTrue(fileExistsNew);

				Assert.AreEqual(sourceFileSize, destinationFileSize);
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldUploadLargeFile()
		{
			var destination = Path.GetTempFileName();

			using (TemporaryFile file = new TemporaryFile(1048L * 1024)) // 1MB
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.UploadFileToTargetAsync(file.Path, destination, true);

				var fileExistsOld = File.Exists(file.Path);
				var fileExistsNew = File.Exists(destination);

				var sourceFileSize = new FileInfo(file.Path).Length;
				var destinationFileSize = new FileInfo(destination).Length;

				File.Delete(destination);

				Assert.IsTrue(fileExistsOld);
				Assert.IsTrue(fileExistsNew);

				Assert.AreEqual(sourceFileSize, destinationFileSize);
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldThrowIOException_WhenDestinationAlreadyExists()
		{
			var destination = Path.GetTempFileName();

			File.Create(destination).Dispose();

			using (TemporaryFile file = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<IOException>(async () =>
				{
					await adsClient.UploadFileToTargetAsync(file.Path, destination, false);
				});

				var fileExistsOld = File.Exists(file.Path);
				var fileExistsNew = File.Exists(destination);

				File.Delete(destination);

				Assert.IsTrue(fileExistsOld);
				Assert.IsTrue(fileExistsNew);
			}
		}

		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldCopyFile_WhenDestinationAlreadyExists()
		{
			var destination = Path.GetTempFileName();

			File.Create(destination).Dispose();

			using (TemporaryFile sourceFile = new TemporaryFile())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				await adsClient.UploadFileToTargetAsync(sourceFile.Path, destination, true);

				var fileExistsOld = File.Exists(sourceFile.Path);
				var fileExistsNew = File.Exists(destination);

				File.Delete(destination);

				Assert.IsTrue(fileExistsOld);
				Assert.IsTrue(fileExistsNew);
			}
		}
	}
}
