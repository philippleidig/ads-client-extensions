using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;

namespace TwinCAT.Ads.Extensions
{
	public static partial class AdsClientExtensions
	{
		public static async Task<Guid> ReadSystemIDAsync(
			this IAdsConnection connection,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			byte[] readData = new byte[16];

			var result = await connection.ReadAsync(0x1010004, 0x1, readData.AsMemory(), cancel);
			result.ThrowOnError();

			return new Guid(readData);
			//bool containsVolumeID = result.ReadBytes == 32;
			//volumeIdData = readData.AsSpan().Slice(16, 32);
		}

		public static async Task StartProcessAsync(
			this IAdsConnection connection,
			string path,
			string directory,
			string args,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			int size = 12 + path.Length + 1 + directory.Length + 1 + args.Length + 1;
			byte[] writeData = new byte[size];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(path.Length);
					writer.Write(directory.Length);
					writer.Write(args.Length);

					writer.Write(path.ToCharArray());
					writer.Write('\0');
					writer.Write(directory.ToCharArray());
					writer.Write('\0');
					writer.Write(args.ToCharArray());
					writer.Write('\0');

					var result = await connection.WriteAsync(500, 0, writeData.AsMemory(), cancel);
					result.ThrowOnError();
				}
			}
		}

		public static async Task<Version> ReadTwinCATFullVersionAsync(
			this IAdsConnection connection,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			var result = await connection.ReadAnyAsync<ushort[]>(160, 0, new int[] { 4 }, cancel);
			result.ThrowOnError();

			return new Version(result.Value[1], result.Value[0], result.Value[3], result.Value[2]);
		}

		public static async Task<DeviceIdentification> ReadDeviceIdentificationAsync(
			this IAdsConnection connection,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			var buffer = new Memory<byte>(new byte[2048]);

			DeviceIdentification device = new DeviceIdentification();

			var result = await connection.ReadAsync(700, 1, buffer, cancel);
			result.ThrowOnError();

			string data = Encoding.ASCII.GetString(buffer.ToArray());

			device.TargetType = GetValueFromTag("<TargetType>", data);
			device.HardwareModel = GetValueFromTag("<Model>", data);
			device.HardwareSerialNo = GetValueFromTag("<SerialNo>", data);
			device.HardwareVersion = GetValueFromTag("<CPUArchitecture>", data);
			device.HardwareDate = GetValueFromTag("<Date>", data);
			device.HardwareCPU = GetValueFromTag("<CPUVersion>", data);

			device.ImageDevice = GetValueFromTag("<ImageDevice>", data);
			device.ImageVersion = GetValueFromTag("<ImageVersion>", data);
			device.ImageLevel = GetValueFromTag("<ImageLevel>", data);
			device.ImageOsName = GetValueFromTag("<OsName>", data);
			device.ImageOsVersion = GetValueFromTag("<OsVersion>", data);

			device.TwinCATVersion = await ReadTwinCATFullVersionAsync(connection, cancel);

			//device.RTPlatform = RTOperatingSystem.GetRTPlatform(device.ImageOsName);

			return device;
		}

		public static async Task<string> QueryRegistryValueAsync(
			this IAdsConnection connection,
			string subKey,
			string valueName,
			CancellationToken cancel = default
		)
		{
			var writeData = new byte[512];
			var readData = new byte[255];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(subKey.ToCharArray());
					writer.Write('\0');
					writer.Write(valueName.ToCharArray());
					writer.Write('\0');

					var result = await connection.ReadWriteAsync(
						200,
						0,
						readData.AsMemory(),
						writeData.AsMemory(),
						cancel
					);
					result.ThrowOnError();

					return Encoding.UTF8.GetString(readData, 0, result.ReadBytes).Trim('\0');
				}
			}
		}

		public static async Task SetRegistryValueAsync(
			this IAdsConnection connection,
			string subKey,
			string valueName,
			RegistryValueType type,
			Memory<byte> data,
			CancellationToken cancel = default
		)
		{
			var writeData = new byte[subKey.Length + 1 + valueName.Length + 1 + data.Length];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(subKey.ToCharArray());
					writer.Write('\0');
					writer.Write(valueName.ToCharArray());
					writer.Write('\0');
					writer.Write(data.ToArray());

					var result = await connection.WriteAsync(200, 0, writeData.AsMemory(), cancel);
					result.ThrowOnError();
				}
			}
		}

		public static async Task<string> ReadHostnameAsync(
			this IAdsConnection connection,
			CancellationToken cancel = default
		)
		{
			var result = await connection.ReadAnyStringAsync(702, 0, 256, Encoding.ASCII, cancel);
			result.ThrowOnError();

			return result.Value.ToString().Trim('\0');
		}

		public static async Task SavePersistentDataAsync(
			this IAdsConnection connection,
			PersistentMode mode = PersistentMode.SPDM_2PASS,
			CancellationToken cancel = default
		)
		{
			if (
				connection.Address.Port < (int)AmsPort.PlcRuntime_851
				|| connection.Address.Port > (int)AmsPort.PlcRuntime_860
			)
			{
				throw new AdsErrorException("Invalid ADS target port.", AdsErrorCode.InvalidPort);
			}

			ResultAds result = await connection.WriteControlAsync(
				AdsState.SaveConfig,
				(ushort)mode,
				cancel
			);
			result.ThrowOnError();
		}

		private static string GetValueFromTag(string tag, string value)
		{
			try
			{
				int idxstart = value.IndexOf(tag) + tag.Length;
				int endidx = value.IndexOf("</", idxstart);
				return value.Substring(idxstart, endidx - idxstart);
			}
			catch (Exception ex)
			{
				return "";
			}
		}
	}
}
