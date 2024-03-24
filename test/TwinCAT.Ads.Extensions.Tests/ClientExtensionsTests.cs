using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	[TestClass]
	public class ClientExtensionsTests
	{
		[TestMethod]
		public async Task ReadTwinCATFullVersionAsync_ShouldThrowInvalidAmsPortException_WhenAmsPortIsInvalid()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.PlcRuntime_851);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
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
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					Version version = await adsClient.ReadTwinCATFullVersionAsync();
				});
			}
		}

		[TestMethod]
		public async Task ReadTwinCATFullVersionAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
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

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					DeviceIdentification deviceIdent = await adsClient.ReadDeviceIdentificationAsync();
				});

				Assert.AreEqual(exception.ErrorCode, AdsErrorCode.InvalidAmsPort);
			}
		}

		[TestMethod]
		public async Task ReadDeviceIdentificationAsync_ShouldThrowClientNotConnectedException_WhenClientIsNotConnected()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				var exception = await Assert.ThrowsExceptionAsync<ClientNotConnectedException>(async () => {
					DeviceIdentification deviceIdent = await adsClient.ReadDeviceIdentificationAsync();
				});
			}
		}

		[TestMethod]
		public async Task ReadDeviceIdentificationAsync_ShouldThrowTargetMachineNotFoundException_WhenTargetIsNotReachable()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(AmsNetId.Parse("111.111.111.111.1.1"), AmsPort.SystemService);

				var exception = await Assert.ThrowsExceptionAsync<AdsErrorException>(async () => {
					DeviceIdentification deviceIdent = await adsClient.ReadDeviceIdentificationAsync();
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

				Assert.AreEqual("Windows 10 Pro", deviceIdent.ImageOsName);
			}
		}
	}
}
