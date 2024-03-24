using System;
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

			//BitConverter.GetBytes(path.Length).CopyTo(writeData, 0);
			//BitConverter.GetBytes(directory.Length).CopyTo(writeData, 4);
			//BitConverter.GetBytes(args.Length).CopyTo(writeData, 8);
			//
			//Encoding.ASCII.GetBytes(path).CopyTo(writeData, 12);
			//Encoding.ASCII.GetBytes(directory).CopyTo(writeData, 12 + path.Length + 1);
			//Encoding.ASCII.GetBytes(args).CopyTo(writeData, 12 + path.Length + 1 + directory.Length + 1);
			//
			//var res = await connection.WriteAsync(500, 0, writeData.AsMemory(), cancel);
			//res.ThrowOnError();
		}

		public static async Task<Version> ReadTwinCATFullVersionAsync(this IAdsConnection connection, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			ushort[] adsVersionInfo = new ushort[4];
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
