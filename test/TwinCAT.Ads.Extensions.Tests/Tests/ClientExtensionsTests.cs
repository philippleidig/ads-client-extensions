using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	[TestClass]
	public class ClientExtensionsTests
	{
		[TestMethod]
		public async Task ReadSystemIDAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.PlcRuntime_851);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					Guid systemID = await adsClient.ReadSystemIDAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task ReadSystemIDAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(
					async () =>
					{
						Guid systemID = await adsClient.ReadSystemIDAsync();
					}
				);
			}
		}

		[TestMethod]
		public async Task ReadSystemIDAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					Guid systemID = await adsClient.ReadSystemIDAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task ReadSystemIDAsync_ShouldReturnValidSystemID()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				Guid systemID = await adsClient.ReadSystemIDAsync();

				Assert.AreEqual(Guid.Empty, systemID);
			}
		}

		[TestMethod]
		public async Task ReadTwinCATFullVersionAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.PlcRuntime_851);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					Version version = await adsClient.ReadTwinCATFullVersionAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task ReadTwinCATFullVersionAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(
					async () =>
					{
						Version version = await adsClient.ReadTwinCATFullVersionAsync();
					}
				);
			}
		}

		[TestMethod]
		public async Task ReadTwinCATFullVersionAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					Version version = await adsClient.ReadTwinCATFullVersionAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task ReadTwinCATFullVersionAsync_ShouldReturnFullVersion()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				Version version = await adsClient.ReadTwinCATFullVersionAsync();

				Assert.AreEqual(version.Major, 3);
				Assert.AreEqual(version.Minor, 1);
				Assert.AreEqual(version.Build, 4024);
				Assert.AreEqual(version.Revision, 55);
			}
		}

		[TestMethod]
		public async Task ReadDeviceIdentificationAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.PlcRuntime_851);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					DeviceIdentification deviceIdent =
						await adsClient.ReadDeviceIdentificationAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task ReadDeviceIdentificationAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(
					async () =>
					{
						DeviceIdentification deviceIdent =
							await adsClient.ReadDeviceIdentificationAsync();
					}
				);
			}
		}

		[TestMethod]
		public async Task ReadDeviceIdentificationAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () =>
				{
					DeviceIdentification deviceIdent =
						await adsClient.ReadDeviceIdentificationAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.TargetMachineNotFound);
			}
		}

		[TestMethod]
		public async Task ReadDeviceIdentificationAsync_ShouldReturnDeviceIdent()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				DeviceIdentification deviceIdent = await adsClient.ReadDeviceIdentificationAsync();

				Assert.AreEqual("Windows 10 Enterprise", deviceIdent.ImageOsName);
			}
		}

		[TestMethod]
		public async Task ReadHostnameAsync_ShouldReturnHostname()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var hostname = await adsClient.ReadHostnameAsync();

				Assert.AreEqual("Test", hostname);
			}
		}

		[TestMethod]
		public async Task QueryRegistryValueAsync()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);
				var version = await adsClient.QueryRegistryValueAsync(
					"SOFTWARE\\WOW6432Node\\Beckhoff\\TwinCAT3 Functions\\Beckhoff TF5400 TC3 Advanced Motion Pack\\Common",
					"Version"
				);

				Assert.AreEqual("3.2.62.0", version);
			}
		}

		//[TestMethod]
		//public async Task StartProcessAsync()
		//{
		//	using (AdsClient adsClient = new AdsClient())
		//	{
		//		adsClient.Connect(TargetSystem, AmsPort.SystemService);
		//		await adsClient.StartProcessAsync("cmd.exe", "", "");
		//	}
		//}
	}
}
