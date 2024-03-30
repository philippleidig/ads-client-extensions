using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;

namespace TwinCAT.Ads.Extensions
{
	public static partial class AdsClientExtensions
	{
		public static async Task<Guid> ReadSystemIDAsync(this IAdsConnection connection, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			byte[] readData = new byte[32];

			var result = await connection.ReadAsync(0x1010004, 0x1, readData.AsMemory(), cancel);
			result.ThrowOnError();

			return new Guid(readData);
			//bool containsVolumeID = result.ReadBytes == 32;
			//volumeIdData = readData.AsSpan().Slice(16, 32);
		}

		public static async Task StartProcessAsync(this IAdsConnection connection, string path, string directory, string args, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService) 
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

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

		public static async Task<Version> ReadTwinCATFullVersionAsync(this IAdsConnection connection, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			var result = await connection.ReadAnyAsync<ushort[]>(160, 0, new int[] { 4 }, cancel);
			result.ThrowOnError();

			return new Version(result.Value[1], result.Value[0], result.Value[3], result.Value[2]);
		}

		public static async Task<DeviceIdentification> ReadDeviceIdentificationAsync(this IAdsConnection connection, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

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

		public static async Task<string> QueryRegistryValueAsync(this IAdsConnection connection, string subKey, string valueName)
		{
			var readBuffer = new Memory<byte>(new byte[255]);

			var data = new List<byte>();

			data.AddRange(Encoding.UTF8.GetBytes(subKey));
			data.Add(new byte()); // End delimiter
			data.AddRange(Encoding.UTF8.GetBytes(valueName));
			data.Add(new byte());

			var writeBuffer = new ReadOnlyMemory<byte>(data.ToArray());

			var result = await connection.ReadWriteAsync(200, 0, readBuffer, writeBuffer, CancellationToken.None);
			result.ThrowOnError();

			return Encoding.UTF8.GetString(readBuffer.ToArray(), 0, result.ReadBytes);
		}

		public static async Task SetRegistryValueAsync(this IAdsConnection connection, string subKey, string valueName, RegistryValueType type, IEnumerable<byte> data)
		{
			var writeBuffer = new List<byte>();

			writeBuffer.AddRange(Encoding.UTF8.GetBytes(subKey));
			writeBuffer.Add(new byte()); // End delimiter
			writeBuffer.AddRange(Encoding.UTF8.GetBytes(valueName));
			writeBuffer.Add(new byte());
			writeBuffer.AddRange(data);

			var result = await connection.WriteAsync(200, 0, new ReadOnlyMemory<byte>(writeBuffer.ToArray()), CancellationToken.None);
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
