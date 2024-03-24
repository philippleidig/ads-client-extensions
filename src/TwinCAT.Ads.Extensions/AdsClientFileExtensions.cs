using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace TwinCAT.Ads.Extensions
{
	public static partial class AdsClientFileExtensions
    {

        private static int DefaultChunkSize = 1024 * 1024;

        public static Task UploadFileFromBootFolderAsync(this IAdsConnection connection, string localFile, string remoteFile, CancellationToken cancel = default)
        {
            return connection.UploadFileAsync(localFile, remoteFile, AdsDirectory.BootDir);
        }

        public static Task UploadFileFromBootFolderAsync(this IAdsConnection connection, Stream stream, string remoteFile, CancellationToken cancel = default)
        {
            return connection.UploadFileAsync(stream, remoteFile, AdsDirectory.BootDir);
        }

        public static async Task UploadFileAsync (this IAdsConnection connection, Stream stream, string fileName, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            if(connection == null) throw new ArgumentNullException(nameof(connection));

            if(string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

            if (connection.Address.Port != (int)AmsPort.SystemService) 
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

            uint fileHandle = await connection.OpenFileAsync(fileName, AdsFileOpenMode.Read | AdsFileOpenMode.Binary, standardDirectory, cancel);

            if (fileHandle <= 0)
            {
                throw new AdsErrorException("Could not open file. Invalid handle.", AdsErrorCode.DeviceNotifyHandleInvalid);
            }
                
			int bytesRead = await connection.ReadFileAsync(fileHandle, stream, cancel);

			//stream.Seek(0L, SeekOrigin.Begin);
			//stream.Flush();

            await connection.CloseFileAsync(fileHandle);
        }

        public static Task UploadFileAsync(this IAdsConnection connection, string localFile, string remoteFile, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            FileStream fs = File.OpenWrite(localFile);
            return connection.UploadFileAsync(fs, remoteFile, standardDirectory, cancel);
        }

        public static Task DownloadFileToBootFolderAsync(this IAdsConnection connection, string localFile, string remoteFile, CancellationToken cancel = default)
        {
            return connection.DownloadFileAsync(localFile, remoteFile, AdsDirectory.BootDir, cancel);
        }

        public static Task DownloadFileAsync(this IAdsConnection connection, string localFile, string remoteFile, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            FileStream fs = File.OpenWrite(localFile);
            return connection.DownloadFileAsync(fs, remoteFile, standardDirectory, cancel);
        }

        public static async Task DownloadFileAsync(this IAdsConnection connection, Stream stream, string remoteFile, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(remoteFile)) throw new ArgumentNullException(nameof(remoteFile));

            if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

            if (connection.Address.Port != (int)AmsPort.SystemService) throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

            uint handle = await connection.OpenFileAsync(remoteFile, AdsFileOpenMode.Write | AdsFileOpenMode.Binary, standardDirectory, cancel);

            if (handle <= 0)
            {
				throw new AdsErrorException("Could not open file. Invalid handle.", AdsErrorCode.DeviceNotifyHandleInvalid);
			}

			var bytesWritten = await connection.WriteFileAsync(handle, stream, cancel);

			if(bytesWritten != stream.Length)
			{
				throw new Exception();
			}

			await connection.CloseFileAsync(handle, cancel);
        }

        public static async Task<bool> FileExistsAsync(this IAdsConnection connection, string fileName, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

            if (connection.Address.Port != (int)AmsPort.SystemService) throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			try
            {
                uint handle = await connection.OpenFileAsync(fileName, AdsFileOpenMode.Read | AdsFileOpenMode.Binary, standardDirectory, cancel);

                if (handle > 0)
                {
					await connection.CloseFileAsync(handle, cancel);
					return true;
                }            
            }
            catch (AdsErrorException ex)
            {
                if(ex.ErrorCode == AdsErrorCode.DeviceNotFound)
				{
					return false;
				}
				else
				{
					throw ex;
				}
            }

            return false;
        }

        public static Task<bool> FileExistsInBootFolder(this IAdsConnection connection, string filePath, CancellationToken cancel = default)
        {
            return connection.FileExistsAsync(filePath, AdsDirectory.BootDir, cancel);
        }

        public static async Task DeleteFileAsync(this IAdsConnection connection, string fileName, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

            if (connection.Address.Port != (int)AmsPort.SystemService) throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			byte[] readData = new byte[0];
            byte[] writeData = new byte[fileName.Length + 1];

            using (MemoryStream writeStream = new MemoryStream(writeData))
            {
                using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
                {
                    writer.Write(fileName.ToCharArray());
                    writer.Write('\0');

					ResultReadWrite result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FDELETE, (uint)standardDirectory << 16, readData.AsMemory(), writeData.AsMemory(), cancel);
                    result.ThrowOnError();
                }
            }
        }

        public static async Task RenameFileAsync(this IAdsConnection connection, string oldName, string newName, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(oldName)) throw new ArgumentNullException(nameof(oldName));

            if (string.IsNullOrEmpty(newName)) throw new ArgumentNullException(nameof(newName));

            if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

            if (connection.Address.Port != (int)AmsPort.SystemService) 
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			if(Path.GetExtension(oldName) != Path.GetExtension(newName))
			{
				throw new IOException("Invalid file extension");
			}

			string newPath = Path.Combine(Path.GetDirectoryName(oldName), newName);

            byte[] writeData = new byte[oldName.Length + 1 + newPath.Length + 1];

            using (MemoryStream writeStream = new MemoryStream(writeData))
            {
                using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
                {
                    writer.Write(oldName.ToCharArray());
                    writer.Write('\0');
                    writer.Write(newPath.ToCharArray());
                    writer.Write('\0');

					uint indexOffset = (uint)standardDirectory << 16;

					ResultReadWrite result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FRENAME, indexOffset, Memory<byte>.Empty, writeData.AsMemory(), cancel);
                    result.ThrowOnError();
                }
            }
        }

		public static async Task CreateFileAsync(this IAdsConnection connection, string path, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			uint fileHandle = await connection.OpenFileAsync(path, AdsFileOpenMode.Write, standardDirectory, cancel);

			if(fileHandle  == 0)
			{
				throw new IOException();
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task WriteAllTextToFileAsync(this IAdsConnection connection, string path, string text, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

			if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			uint fileHandle = await connection.OpenFileAsync(path, AdsFileOpenMode.Write | AdsFileOpenMode.Text, standardDirectory, cancel);

			using( MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text ?? "")))
			{
				int bytesWritten = await connection.WriteFileAsync(fileHandle, stream, cancel);
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task WriteAllBytesToFileAsync(this IAdsConnection connection, string path, Memory<byte> bytes, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

			if (bytes.IsEmpty) throw new ArgumentNullException(nameof(bytes));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			uint fileHandle = await connection.OpenFileAsync(path, AdsFileOpenMode.Write | AdsFileOpenMode.Binary, standardDirectory, cancel);

			int bytesWritten = await connection.WriteFileAsync(fileHandle, bytes, cancel);

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task AppendAllTextToFileAsync(this IAdsConnection connection, string path, string text, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

			if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			uint fileHandle = await connection.OpenFileAsync(path, AdsFileOpenMode.Append | AdsFileOpenMode.Text, standardDirectory, cancel);

			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text ?? "")))
			{
				int bytesWritten = await connection.WriteFileAsync(fileHandle, stream, cancel);
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task AppendAllBytesToFileAsync(this IAdsConnection connection, string path, Memory<byte> bytes, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

			if (bytes.IsEmpty) throw new ArgumentNullException(nameof(bytes));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			uint fileHandle = await connection.OpenFileAsync(path, AdsFileOpenMode.Append | AdsFileOpenMode.Binary, standardDirectory, cancel);

			int bytesWritten = await connection.WriteFileAsync(fileHandle, bytes, cancel);

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task CopyFileAsync(this IAdsConnection connection, string source, string destination, bool overwrite = false, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));

			if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException(nameof(destination));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			uint sourceFileHandle = await connection.OpenFileAsync(source, AdsFileOpenMode.Read | AdsFileOpenMode.Binary, standardDirectory, cancel);
			uint destinationFileHandle = await connection.OpenFileAsync(destination, AdsFileOpenMode.Write | AdsFileOpenMode.Binary, standardDirectory, cancel);


			await connection.CloseFileAsync(sourceFileHandle, cancel);
			await connection.CloseFileAsync(destinationFileHandle, cancel);
		}

		public static async Task MoveFileAsync(this IAdsConnection connection, string source, string destination, bool overwrite = false, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));

			if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException(nameof(destination));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			uint sourceFileHandle = await connection.OpenFileAsync(source, AdsFileOpenMode.Read | AdsFileOpenMode.Binary, standardDirectory, cancel);
			uint destinationFileHandle = await connection.OpenFileAsync(destination, AdsFileOpenMode.Write | AdsFileOpenMode.Binary, standardDirectory, cancel);

			await connection.CloseFileAsync(sourceFileHandle, cancel);
			await connection.CloseFileAsync(destinationFileHandle, cancel);
		}

		private static async Task<uint> OpenFileAsync(this IAdsConnection connection, string fileName, AdsFileOpenMode mode, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			byte[] readData = new byte[sizeof(uint)];
			byte[] writeData = new byte[fileName.Length + 1];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(fileName.ToCharArray());
					writer.Write('\0');

					uint indexOffset = (uint)(mode | (AdsFileOpenMode)standardDirectory);
					ResultReadWrite result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FOPEN, indexOffset, readData.AsMemory(), writeData.AsMemory(), cancel);
					result.ThrowOnError();

					if (result.ReadBytes < sizeof(uint))
					{
						throw new Exception();
					}

					using (MemoryStream readStream = new MemoryStream(readData))
					{
						using (BinaryReader reader = new BinaryReader(readStream, Encoding.Default))
						{
							return reader.ReadUInt32();
						}
					}
				}
			}
		}

		private static async Task CloseFileAsync(this IAdsConnection connection, uint fileHandle, CancellationToken cancel = default)
		{
			ResultReadWrite result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FCLOSE, fileHandle, Memory<byte>.Empty, Memory<byte>.Empty, cancel);
			result.ThrowOnError();
		}

		private static async Task<int> ReadFileAsync(this IAdsConnection connection, uint fileHandle, Memory<byte> readData, CancellationToken cancel = default)
		{
			ResultReadWrite result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FREAD, fileHandle, readData, Memory<byte>.Empty, cancel);
			result.ThrowOnError();

			return result.ReadBytes;
		}

		private static async Task<int> ReadFileAsync(this IAdsConnection connection, uint fileHandle, Stream destination, CancellationToken cancel = default)
		{
			byte[] readData = new byte[DefaultChunkSize];
			int bytesRead = 0;
			int length = 0;

			using (BinaryWriter reader = new BinaryWriter(destination, Encoding.Default))
			{
				do
				{
					length = await connection.ReadFileAsync(fileHandle, readData, cancel);

					if (length > 0)
					{
						reader.Write(readData);
						bytesRead += length;
					}
				}
				while (length > 0);
			}

			return bytesRead;
		}

		private static async Task<int> WriteFileAsync(this IAdsConnection connection, uint fileHandle, Stream source, CancellationToken cancel = default)
		{
			int bytesWritten = 0;

			using (BinaryReader reader = new BinaryReader(source, Encoding.Default))
			{
				do
				{
					byte[] writeData = reader.ReadBytes(DefaultChunkSize);
					int length = await connection.WriteFileAsync(fileHandle, writeData.AsMemory(), cancel);

					bytesWritten += length;
				}
				while (source.Length == bytesWritten);
			}

			return bytesWritten;
		}

		private static async Task<int> WriteFileAsync(this IAdsConnection connection, uint fileHandle, Memory<byte> writeData, CancellationToken cancel = default)
		{
			var result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FWRITE, fileHandle, Memory<byte>.Empty, writeData, cancel);
			result.ThrowOnError();

			return writeData.Length;
		}
	}
}
