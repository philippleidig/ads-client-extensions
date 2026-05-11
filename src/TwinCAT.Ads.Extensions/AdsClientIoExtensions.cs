using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads.Extensions.TypeSystem;

namespace TwinCAT.Ads.Extensions
{
	/// <summary>
	/// Provides extension methods for EtherCAT I/O operations via TwinCAT ADS.
	/// Includes EtherCAT master discovery, sync unit queries, physical register access,
	/// EEPROM reads, and CoE (CANopen over EtherCAT) SDO reads.
	/// </summary>
	public static class AdsClientIoExtensions
	{
		#region Constants

		// I/O device discovery (R0_IO)
		private const uint IOADS_IGR_IODEVICESTATE_BASE = 0x5000;
		private const uint IOADS_IOF_READDEVIDS = 0x1;
		private const uint IOADS_IOF_READDEVNAME = 0x1;
		private const uint IOADS_IOF_READDEVCOUNT = 0x2;
		private const uint IOADS_IOF_READDEVNETID = 0x5;
		private const uint IOADS_IOF_READDEVTYPE = 0x7;
		private const ushort CX7000_DEVICE_ID = 3;

		// EtherCAT ADS index groups
		private const uint EC_ADS_IGRP_MASTER_SENDCMD = 0x500;
		private const uint EC_ADS_IGRP_CANOPEN_SDO = 0xF302;

		// EtherCAT command header size (cmd + idx + adp + ado + len + irq = 10 bytes)
		private const int EC_HEADER_SIZE = 10;

		// EtherCAT command types
		private const byte EC_CMD_TYPE_APRD = 1;
		private const byte EC_CMD_TYPE_APWR = 2;
		private const byte EC_CMD_TYPE_FPRD = 4;
		private const byte EC_CMD_TYPE_FPWR = 5;
		private const byte EC_CMD_TYPE_BRD = 7;
		private const byte EC_CMD_TYPE_BWR = 8;

		// EEPROM registers
		private const ushort REGISTER_AL_STATUS = 0x0130;
		private const ushort REGISTER_EEPROM_CONTROL = 0x0502;
		private const ushort REGISTER_EEPROM_ADDRESS = 0x0504;
		private const ushort REGISTER_EEPROM_DATA0 = 0x0508;
		private const ushort REGISTER_EEPROM_DATA1 = 0x050A;
		private const ushort REGISTER_EEPROM_DATA2 = 0x050C;
		private const ushort REGISTER_EEPROM_DATA3 = 0x050E;

		// TcCOM object server
		private const uint TCCOM_IG_QUERY_CLASS = 0x201;
		private static readonly Guid SyncUnitClassId = new Guid("03020004-0000-0000-F000-000000000064");

		// EEPROM production data address
		private const ushort EEPROM_PRODUCTION_DATA_ADDRESS = 0x0020;

		private const int EEPROM_MAX_RETRIES = 25;

		#endregion

		#region I/O Device Discovery

		/// <summary>
		/// Discovers all EtherCAT masters connected to the target system via the I/O subsystem.
		/// CX7000 devices (device ID 3) are automatically excluded as they are not EtherCAT masters.
		/// The returned <see cref="EtherCatMasterInfo.AmsNetId"/> is a routable address
		/// combining the target's AmsNetId with the master's local route suffix.
		/// </summary>
		/// <param name="connection">
		/// An ADS connection to the target system on port <see cref="AmsPort.R0_IO"/> (300).
		/// </param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>An array of discovered EtherCAT master information.</returns>
		/// <exception cref="AdsErrorException">Thrown when the connection is not on port R0_IO.</exception>
		public static async Task<EtherCatMasterInfo[]> ListEtherCatMastersAsync(
			this IAdsConnection connection,
			CancellationToken cancel = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.R0_IO)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port R0_IO (300).",
					AdsErrorCode.InvalidAmsPort);

			AmsNetId target = connection.Address.NetId;

			// Read device count
			var countResult = await connection.ReadAnyAsync<uint>(
				IOADS_IGR_IODEVICESTATE_BASE, IOADS_IOF_READDEVCOUNT, cancel);
			countResult.ThrowOnError();
			uint deviceCount = countResult.Value;

			if (deviceCount == 0)
				return Array.Empty<EtherCatMasterInfo>();

			// Read device IDs. The first element contains the count; actual IDs start at index 1.
			Memory<byte> idBuffer = new byte[(deviceCount + 1) * sizeof(ushort)];
			var idsResult = await connection.ReadAsync(
				IOADS_IGR_IODEVICESTATE_BASE, IOADS_IOF_READDEVIDS, idBuffer, cancel);
			idsResult.ThrowOnError();

			ushort[] deviceIDs = new ushort[deviceCount + 1];
			using (var ms = new MemoryStream(idBuffer.ToArray()))
			{
				using (var br = new BinaryReader(ms))
				{
					for (int i = 0; i < deviceIDs.Length; i++)
					{
						deviceIDs[i] = br.ReadUInt16();
					}
				}
			}

			var masters = new List<EtherCatMasterInfo>();

			for (int i = 1; i <= (int)deviceCount; i++)
			{
				ushort deviceID = deviceIDs[i];

				// Skip CX7000 devices – they are not EtherCAT masters and have no AMS Net ID.
				if (deviceID == CX7000_DEVICE_ID)
					continue;

				// Read device type
				var typeResult = await connection.ReadAnyAsync<ushort>(
					IOADS_IGR_IODEVICESTATE_BASE + deviceID, IOADS_IOF_READDEVTYPE, cancel);
				typeResult.ThrowOnError();

				// Read device name
				var nameResult = await connection.ReadAnyStringAsync(
					IOADS_IGR_IODEVICESTATE_BASE + deviceID, IOADS_IOF_READDEVNAME,
					256, Encoding.ASCII, cancel);
				nameResult.ThrowOnError();

				// Read AMS Net ID (6 bytes)
				Memory<byte> amsBuffer = new byte[6];
				var amsResult = await connection.ReadAsync(
					IOADS_IGR_IODEVICESTATE_BASE + deviceID, IOADS_IOF_READDEVNETID,
					amsBuffer, cancel);
				amsResult.ThrowOnError();

				var localAmsNetId = new AmsNetId(amsBuffer.ToArray());

				// EtherCAT masters have a local AmsNetId (e.g. 192.168.5.89.2.1).
				// Combine the target's first 4 bytes with the local last 2 bytes
				// to create a remotely routable AmsNetId.
				byte[] combined = target.ToBytes();
				byte[] localBytes = localAmsNetId.ToBytes();
				combined[4] = localBytes[4];
				combined[5] = localBytes[5];

				masters.Add(new EtherCatMasterInfo
				{
					AmsNetId = new AmsNetId(combined),
					DeviceType = typeResult.Value,
					Name = nameResult.Value.ToString().TrimEnd('\0')
				});
			}

			return masters.ToArray();
		}

		#endregion

		#region TcCOM Object Server – Sync Units

		/// <summary>
		/// Reads all EtherCAT sync unit object IDs (OTCIDs) from the TwinCAT object server.
		/// These IDs can be used to query sync unit properties and associated slave addresses.
		/// </summary>
		/// <param name="connection">
		/// An ADS connection on port 10 (TcCOM / Router).
		/// </param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>An array of sync unit object IDs (OTCIDs).</returns>
		public static async Task<uint[]> GetSyncUnitObjectIdsAsync(
			this IAdsConnection connection,
			CancellationToken cancel = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			byte[] classIdBytes = SyncUnitClassId.ToByteArray();

			// Query count of objects matching the Sync Unit class ID
			byte[] countBuffer = new byte[sizeof(uint)];
			var countResult = await connection.ReadWriteAsync(
				TCCOM_IG_QUERY_CLASS, 0x01,
				countBuffer.AsMemory(), classIdBytes.AsMemory(), cancel);
			countResult.ThrowOnError();

			uint syncUnitCount = BitConverter.ToUInt32(countBuffer, 0);

			if (syncUnitCount == 0)
				return Array.Empty<uint>();

			// Query the OTCIDs
			byte[] oidsBuffer = new byte[syncUnitCount * sizeof(uint)];
			var oidsResult = await connection.ReadWriteAsync(
				TCCOM_IG_QUERY_CLASS, 0x02,
				oidsBuffer.AsMemory(), classIdBytes.AsMemory(), cancel);
			oidsResult.ThrowOnError();

			uint[] objectIds = new uint[syncUnitCount];
			using (var ms = new MemoryStream(oidsBuffer))
			{
				using (var br = new BinaryReader(ms))
				{
					for (int i = 0; i < syncUnitCount; i++)
					{
						objectIds[i] = br.ReadUInt32();
					}
				}
			}

			return objectIds;
		}

		#endregion

		#region EtherCAT Physical Access

		/// <summary>
		/// Performs an EtherCAT physical read operation on a slave's register via the EtherCAT master.
		/// This sends an EtherCAT datagram (FPRD/APRD/BRD) through the master's ADS command interface.
		/// </summary>
		/// <param name="connection">
		/// An ADS connection to the EtherCAT master's AmsNetId on port 0xFFFF (EC_AMSPORT_MASTER).
		/// </param>
		/// <param name="slaveAddress">Configured station address (fixed) or position (auto-increment).</param>
		/// <param name="registerOffset">Register offset in the EtherCAT Slave Controller (ESC).</param>
		/// <param name="dataLength">Number of bytes to read.</param>
		/// <param name="addressingType">Addressing type for the EtherCAT datagram.</param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>The data read from the slave register.</returns>
		public static async Task<byte[]> EcPhysicalReadAsync(
			this IAdsConnection connection,
			ushort slaveAddress,
			ushort registerOffset,
			int dataLength,
			EcAddressingType addressingType = EcAddressingType.Fixed,
			CancellationToken cancel = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (dataLength <= 0)
				throw new ArgumentOutOfRangeException(nameof(dataLength));

			byte cmdType = GetReadCommandType(addressingType);
			int totalSize = EC_HEADER_SIZE + dataLength + 2; // header + data + WKC

			byte[] writeBuffer = new byte[totalSize];
			byte[] readBuffer = new byte[totalSize];

			BuildEcCommandHeader(writeBuffer, cmdType, slaveAddress, registerOffset, dataLength);

			var result = await connection.ReadWriteAsync(
				EC_ADS_IGRP_MASTER_SENDCMD, 0,
				readBuffer.AsMemory(), writeBuffer.AsMemory(), cancel);
			result.ThrowOnError();

			byte[] data = new byte[dataLength];
			Array.Copy(readBuffer, EC_HEADER_SIZE, data, 0, dataLength);

			return data;
		}

		/// <summary>
		/// Performs an EtherCAT physical write operation on a slave's register via the EtherCAT master.
		/// This sends an EtherCAT datagram (FPWR/APWR/BWR) through the master's ADS command interface.
		/// </summary>
		/// <param name="connection">
		/// An ADS connection to the EtherCAT master's AmsNetId on port 0xFFFF (EC_AMSPORT_MASTER).
		/// </param>
		/// <param name="slaveAddress">Configured station address (fixed) or position (auto-increment).</param>
		/// <param name="registerOffset">Register offset in the EtherCAT Slave Controller (ESC).</param>
		/// <param name="data">Data to write to the register.</param>
		/// <param name="addressingType">Addressing type for the EtherCAT datagram.</param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>The working counter (WKC) from the EtherCAT response.</returns>
		public static async Task<ushort> EcPhysicalWriteAsync(
			this IAdsConnection connection,
			ushort slaveAddress,
			ushort registerOffset,
			byte[] data,
			EcAddressingType addressingType = EcAddressingType.Fixed,
			CancellationToken cancel = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (data == null || data.Length == 0)
				throw new ArgumentNullException(nameof(data));

			byte cmdType = GetWriteCommandType(addressingType);
			int dataLength = data.Length;
			int totalSize = EC_HEADER_SIZE + dataLength + 2;

			byte[] writeBuffer = new byte[totalSize];
			byte[] readBuffer = new byte[totalSize];

			BuildEcCommandHeader(writeBuffer, cmdType, slaveAddress, registerOffset, dataLength);

			// Copy source data into the command buffer after the header
			Buffer.BlockCopy(data, 0, writeBuffer, EC_HEADER_SIZE, dataLength);

			var result = await connection.ReadWriteAsync(
				EC_ADS_IGRP_MASTER_SENDCMD, 0,
				readBuffer.AsMemory(), writeBuffer.AsMemory(), cancel);
			result.ThrowOnError();

			// Extract working counter (little-endian, after data)
			ushort wkc = (ushort)(readBuffer[EC_HEADER_SIZE + dataLength]
				| (readBuffer[EC_HEADER_SIZE + dataLength + 1] << 8));

			return wkc;
		}

		#endregion

		#region EtherCAT EEPROM

		/// <summary>
		/// Reads 4 words (8 bytes) from a slave's EEPROM at the specified word address.
		/// The read is performed via physical register access to the ESC's EEPROM interface
		/// (registers 0x0500–0x050E). Automatically handles 2-word and 4-word access modes.
		/// </summary>
		/// <param name="connection">
		/// An ADS connection to the EtherCAT master's AmsNetId on port 0xFFFF (EC_AMSPORT_MASTER).
		/// </param>
		/// <param name="slaveAddress">Configured station address of the slave.</param>
		/// <param name="eepromWordAddress">EEPROM word address to read from (e.g. 0x0020 for production data).</param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>An array of 4 words read from the EEPROM.</returns>
		/// <exception cref="AdsErrorException">Thrown when the slave is in an invalid state or the EEPROM is unresponsive.</exception>
		public static async Task<ushort[]> ReadSlaveEepromAsync(
			this IAdsConnection connection,
			ushort slaveAddress,
			ushort eepromWordAddress,
			CancellationToken cancel = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			// Step 1: Check slave state via AL_Status register (0x0130)
			byte[] alStatus = await connection.EcPhysicalReadAsync(
				slaveAddress, REGISTER_AL_STATUS, 2, EcAddressingType.Fixed, cancel);

			ushort statusValue = BitConverter.ToUInt16(alStatus, 0);

			// Bit 0 = 1 indicates INIT or Bootstrap, Bit 4 = 1 indicates error, 0 = not reachable
			if ((statusValue & 0x01) != 0 || (statusValue & 0x10) != 0 || statusValue == 0)
				throw new AdsErrorException(
					"Slave is not in a valid state for EEPROM read (INIT, Bootstrap, Error, or not reachable).",
					AdsErrorCode.DeviceError);

			// Step 2: Check EEPROM control register – busy and 4-word access capability
			bool isFourWordAccess = false;
			int retryCount = 0;

			while (true)
			{
				byte[] ctrlData = await connection.EcPhysicalReadAsync(
					slaveAddress, REGISTER_EEPROM_CONTROL, 2, EcAddressingType.Fixed, cancel);

				ushort ctrlValue = BitConverter.ToUInt16(ctrlData, 0);

				// Bit 6: 4-word access capability
				isFourWordAccess = (ctrlValue & 0x0040) != 0;

				// Bit 15: Busy
				if ((ctrlValue & 0x8000) != 0)
				{
					retryCount++;
					if (retryCount > EEPROM_MAX_RETRIES)
						throw new AdsErrorException(
							"EEPROM busy timeout after maximum retries.",
							AdsErrorCode.DeviceError);

					await Task.Delay(1, cancel);
					continue;
				}

				// Bits 11–14: Error bits
				if ((ctrlValue & 0x7800) != 0)
				{
					// Clear control register to reload EEPROM configuration
					await connection.EcPhysicalWriteAsync(
						slaveAddress, REGISTER_EEPROM_CONTROL,
						new byte[] { 0, 0 }, EcAddressingType.Fixed, cancel);

					retryCount++;
					if (retryCount > EEPROM_MAX_RETRIES)
						throw new AdsErrorException(
							"EEPROM error bits could not be cleared after maximum retries.",
							AdsErrorCode.DeviceError);

					continue;
				}

				break;
			}

			// Step 3: Read EEPROM data
			ushort[] eepromData = new ushort[4];

			if (isFourWordAccess)
			{
				// 4-word access: single address setup reads all 4 data registers
				await WriteEepromAddressAndSendReadCommand(
					connection, slaveAddress, eepromWordAddress, cancel);

				eepromData[0] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA0, cancel);
				eepromData[1] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA1, cancel);
				eepromData[2] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA2, cancel);
				eepromData[3] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA3, cancel);
			}
			else
			{
				// 2-word access: read first 2 words, then set address+2 and read next 2 words
				await WriteEepromAddressAndSendReadCommand(
					connection, slaveAddress, eepromWordAddress, cancel);

				eepromData[0] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA0, cancel);
				eepromData[1] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA1, cancel);

				// Read next 2 words at address + 2
				await WriteEepromAddressAndSendReadCommand(
					connection, slaveAddress, (ushort)(eepromWordAddress + 2), cancel);

				eepromData[2] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA0, cancel);
				eepromData[3] = await ReadEepromRegisterWord(connection, slaveAddress, REGISTER_EEPROM_DATA1, cancel);
			}

			return eepromData;
		}

		/// <summary>
		/// Reads the production data (firmware version, hardware version, production date)
		/// from a slave's EEPROM at address 0x0020.
		/// The production date is decoded from a 16-bit value: year (bits 0–6),
		/// day of week (bits 7–9), calendar week (bits 10–15).
		/// </summary>
		/// <param name="connection">
		/// An ADS connection to the EtherCAT master's AmsNetId on port 0xFFFF (EC_AMSPORT_MASTER).
		/// </param>
		/// <param name="slaveAddress">Configured station address of the slave.</param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>The decoded production data.</returns>
		public static async Task<EcProductionData> ReadSlaveProductionDataAsync(
			this IAdsConnection connection,
			ushort slaveAddress,
			CancellationToken cancel = default)
		{
			ushort[] eepromData = await connection.ReadSlaveEepromAsync(
				slaveAddress, EEPROM_PRODUCTION_DATA_ADDRESS, cancel);

			// Decode production date from EEPROM word 2
			ushort raw = eepromData[2];
			int year = raw & 0x7F;                  // Bits 0–6: year offset from 2000
			int dayOfWeek = (raw >> 7) & 0x07;      // Bits 7–9: day of week
			int calendarWeek = (raw >> 10) & 0x3F;   // Bits 10–15: calendar week

			return new EcProductionData
			{
				FirmwareVersion = eepromData[0],
				HardwareVersion = eepromData[1],
				ProductionDate = new EcProductionDate
				{
					Year = (year > 0 && year <= 99) ? year + 2000 : 0,
					CalendarWeek = calendarWeek,
					DayOfWeek = dayOfWeek
				}
			};
		}

		#endregion

		#region EtherCAT CoE (CANopen over EtherCAT)

		/// <summary>
		/// Reads a typed value from a slave's CoE (CANopen over EtherCAT) object dictionary
		/// using an SDO upload request.
		/// </summary>
		/// <typeparam name="T">The type to read (e.g. uint, ushort, byte).</typeparam>
		/// <param name="connection">
		/// An ADS connection to the EtherCAT master's AmsNetId with the slave address as port number.
		/// For example, to read from slave 1001: connect to ecMasterAmsNetId on port 1001.
		/// </param>
		/// <param name="coeIndex">CANopen SDO index (e.g. 0x1018 for Identity Object).</param>
		/// <param name="subIndex">CANopen SDO sub-index.</param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>The value read from the object dictionary.</returns>
		public static async Task<T> EcCoEReadAsync<T>(
			this IAdsConnection connection,
			ushort coeIndex,
			byte subIndex,
			CancellationToken cancel = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			uint indexOffset = ((uint)coeIndex << 16) | subIndex;

			var result = await connection.ReadAnyAsync<T>(
				EC_ADS_IGRP_CANOPEN_SDO, indexOffset, cancel);
			result.ThrowOnError();

			return result.Value;
		}

		/// <summary>
		/// Reads raw bytes from a slave's CoE (CANopen over EtherCAT) object dictionary.
		/// Supports complete access mode for reading entire objects at once.
		/// </summary>
		/// <param name="connection">
		/// An ADS connection to the EtherCAT master's AmsNetId with the slave address as port number.
		/// </param>
		/// <param name="coeIndex">CANopen SDO index.</param>
		/// <param name="subIndex">CANopen SDO sub-index.</param>
		/// <param name="buffer">Buffer to receive the read data.</param>
		/// <param name="completeAccess">
		/// If true, reads the complete object (all sub-indices) in a single request.
		/// This sets bit 8 of the index offset as per the EtherCAT CoE specification.
		/// </param>
		/// <param name="cancel">Cancellation token.</param>
		/// <returns>The number of bytes read.</returns>
		public static async Task<int> EcCoEReadAsync(
			this IAdsConnection connection,
			ushort coeIndex,
			byte subIndex,
			Memory<byte> buffer,
			bool completeAccess = false,
			CancellationToken cancel = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			uint indexOffset = ((uint)coeIndex << 16) | subIndex;

			if (completeAccess)
				indexOffset |= 0x100;

			var result = await connection.ReadAsync(
				EC_ADS_IGRP_CANOPEN_SDO, indexOffset, buffer, cancel);
			result.ThrowOnError();

			return result.ReadBytes;
		}

		#endregion

		#region Private Helpers

		private static byte GetReadCommandType(EcAddressingType type)
		{
			switch (type)
			{
				case EcAddressingType.Fixed: return EC_CMD_TYPE_FPRD;
				case EcAddressingType.AutoIncrement: return EC_CMD_TYPE_APRD;
				case EcAddressingType.Broadcast: return EC_CMD_TYPE_BRD;
				default: throw new ArgumentException("Unknown addressing type: " + type, nameof(type));
			}
		}

		private static byte GetWriteCommandType(EcAddressingType type)
		{
			switch (type)
			{
				case EcAddressingType.Fixed: return EC_CMD_TYPE_FPWR;
				case EcAddressingType.AutoIncrement: return EC_CMD_TYPE_APWR;
				case EcAddressingType.Broadcast: return EC_CMD_TYPE_BWR;
				default: throw new ArgumentException("Unknown addressing type: " + type, nameof(type));
			}
		}

		/// <summary>
		/// Builds an EtherCAT command header in the given buffer.
		/// Header layout (10 bytes): cmd(1) + idx(1) + adp(2) + ado(2) + len(2) + irq(2).
		/// </summary>
		private static void BuildEcCommandHeader(byte[] buffer, byte cmdType, ushort adp, ushort ado, int dataLength)
		{
			using (var ms = new MemoryStream(buffer))
			{
				using (var bw = new BinaryWriter(ms))
				{
					bw.Write(cmdType);              // cmd  (1 byte)
					bw.Write((byte)0);              // idx  (1 byte)
					bw.Write(adp);                  // adp  (2 bytes, slave address)
					bw.Write(ado);                  // ado  (2 bytes, register offset)
					bw.Write((short)dataLength);    // len  (2 bytes)
					bw.Write((ushort)0);            // irq  (2 bytes)
				}
			}
		}

		/// <summary>
		/// Writes the EEPROM word address and sends the read command to the EEPROM control register.
		/// </summary>
		private static async Task WriteEepromAddressAndSendReadCommand(
			IAdsConnection connection,
			ushort slaveAddress,
			ushort eepromAddress,
			CancellationToken cancel)
		{
			// Write EEPROM word address to address register (0x0504)
			await connection.EcPhysicalWriteAsync(
				slaveAddress, REGISTER_EEPROM_ADDRESS,
				BitConverter.GetBytes(eepromAddress),
				EcAddressingType.Fixed, cancel);

			// Send read command: bit 8 of EEPROM control register = read
			ushort readCommand = 0x0100;
			await connection.EcPhysicalWriteAsync(
				slaveAddress, REGISTER_EEPROM_CONTROL,
				BitConverter.GetBytes(readCommand),
				EcAddressingType.Fixed, cancel);
		}

		/// <summary>
		/// Reads a single word from an EEPROM data register via physical read.
		/// </summary>
		private static async Task<ushort> ReadEepromRegisterWord(
			IAdsConnection connection,
			ushort slaveAddress,
			ushort register,
			CancellationToken cancel)
		{
			byte[] data = await connection.EcPhysicalReadAsync(
				slaveAddress, register, 2, EcAddressingType.Fixed, cancel);

			return BitConverter.ToUInt16(data, 0);
		}

		#endregion
	}
}
