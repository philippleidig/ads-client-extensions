using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;

namespace TwinCAT.Ads.Extensions
{
	public static partial class AdsClientFileExtensions
	{
		private static int DefaultChunkSize = 1024 * 1024;

		public static Task UploadFileToTargetBootFolderAsync(
			this IAdsConnection connection,
			string localFile,
			string remoteFile,
			bool overwrite = false,
			bool ensureDirectory = false,
			CancellationToken cancel = default
		)
		{
			return connection.UploadFileToTargetAsync(
				localFile,
				remoteFile,
				overwrite,
				ensureDirectory,
				AdsDirectory.BootDir
			);
		}

		public static Task UploadFileToTargetBootFolderAsync(
			this IAdsConnection connection,
			Stream stream,
			string remoteFile,
			bool overwrite = false,
			bool ensureDirectory = false,
			CancellationToken cancel = default
		)
		{
			return connection.UploadFileToTargetAsync(
				stream,
				remoteFile,
				overwrite,
				ensureDirectory,
				AdsDirectory.BootDir
			);
		}

		public static async Task UploadFileToTargetAsync(
			this IAdsConnection connection,
			Stream stream,
			string fileName,
			bool overwrite = false,
			bool ensureDirectory = false,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (stream.Length == 0)
				throw new EndOfStreamException();

			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			if (!overwrite)
			{
				var destinationExists = await connection.FileExistsAsync(
					fileName,
					standardDirectory,
					cancel
				);

				if (destinationExists)
				{
					throw new IOException("File already exists. Possibly use override flag.");
				}
			}

			AdsFileOpenMode mode = AdsFileOpenMode.Write | AdsFileOpenMode.Binary;

			if (ensureDirectory)
			{
				mode |= AdsFileOpenMode.EnsureDirectory;
			}

			if (overwrite)
			{
				mode |= AdsFileOpenMode.Overwrite;
			}

			uint fileHandle = await connection.OpenFileAsync(
				fileName,
				mode,
				standardDirectory,
				cancel
			);

			if (fileHandle <= 0)
			{
				throw new AdsErrorException(
					"Could not open file. Invalid handle.",
					AdsErrorCode.DeviceNotifyHandleInvalid
				);
			}

			int bytesWritten = await connection.WriteFileAsync(fileHandle, stream, cancel);

			await connection.CloseFileAsync(fileHandle);
		}

		public static Task UploadFileToTargetAsync(
			this IAdsConnection connection,
			string localFile,
			string remoteFile,
			bool overwrite = false,
			bool ensureDirectory = false,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (string.IsNullOrEmpty(localFile))
				throw new ArgumentNullException(nameof(localFile));

			if (Path.GetExtension(localFile) != Path.GetExtension(remoteFile))
			{
				throw new IOException("Invalid file extension");
			}

			if (!File.Exists(localFile))
			{
				throw new FileNotFoundException(localFile);
			}

			FileStream fs = File.OpenRead(localFile);
			return connection.UploadFileToTargetAsync(
				fs,
				remoteFile,
				overwrite,
				ensureDirectory,
				standardDirectory,
				cancel
			);
		}

		public static async Task UploadFolderContentToTargetAsync(
			this IAdsConnection connection,
			string localPath,
			string remotePath,
			bool overwrite = false,
			bool ensureDirectory = false,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (!Directory.Exists(localPath))
			{
				throw new DirectoryNotFoundException(localPath);
			}

			var directories = Directory.EnumerateDirectories(
				localPath,
				"*",
				SearchOption.AllDirectories
			);

			foreach (var directory in directories)
			{
				var relativePath = "";
				await connection.CreateDirectoryAsync(relativePath, standardDirectory, cancel);
			}

			var files = Directory.EnumerateFiles(localPath, "*", SearchOption.AllDirectories);

			foreach (var file in files)
			{
				var relativePath = "";
				await connection.UploadFileToTargetAsync(
					localPath,
					relativePath,
					overwrite,
					ensureDirectory,
					standardDirectory,
					cancel
				);
			}
		}

		public static Task DownloadFileFromBootFolderAsync(
			this IAdsConnection connection,
			string localFile,
			string remoteFile,
			CancellationToken cancel = default
		)
		{
			return connection.DownloadFileFromTargetAsync(
				localFile,
				remoteFile,
				AdsDirectory.BootDir,
				cancel
			);
		}

		public static Task DownloadFileFromTargetAsync(
			this IAdsConnection connection,
			string localFile,
			string remoteFile,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (File.Exists(localFile))
			{
				throw new IOException("Local file already exists.");
			}

			FileStream fs = File.OpenWrite(localFile);
			return connection.DownloadFileFromTargetAsync(
				fs,
				remoteFile,
				standardDirectory,
				cancel
			);
		}

		public static async Task DownloadFileFromTargetAsync(
			this IAdsConnection connection,
			Stream stream,
			string remoteFile,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(remoteFile))
				throw new ArgumentNullException(nameof(remoteFile));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			uint handle = await connection.OpenFileAsync(
				remoteFile,
				AdsFileOpenMode.Read | AdsFileOpenMode.Binary,
				standardDirectory,
				cancel
			);

			if (handle <= 0)
			{
				throw new AdsErrorException(
					"Could not open file. Invalid handle.",
					AdsErrorCode.DeviceNotifyHandleInvalid
				);
			}

			var bytesRead = await connection.ReadFileAsync(handle, stream, cancel);

			await connection.CloseFileAsync(handle, cancel);
		}

		public static async Task<bool> FileExistsAsync(
			this IAdsConnection connection,
			string fileName,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			try
			{
				uint handle = await connection.OpenFileAsync(
					fileName,
					AdsFileOpenMode.Read | AdsFileOpenMode.Binary,
					standardDirectory,
					cancel
				);

				if (handle > 0)
				{
					await connection.CloseFileAsync(handle, cancel);
					return true;
				}
			}
			catch (AdsErrorException ex)
			{
				if (ex.ErrorCode == AdsErrorCode.DeviceNotFound)
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

		public static Task<bool> FileExistsInBootFolder(
			this IAdsConnection connection,
			string filePath,
			CancellationToken cancel = default
		)
		{
			return connection.FileExistsAsync(filePath, AdsDirectory.BootDir, cancel);
		}

		public static async Task DeleteFileAsync(
			this IAdsConnection connection,
			string fileName,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			byte[] writeData = new byte[fileName.Length + 1];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(fileName.ToCharArray());
					writer.Write('\0');

					uint indexOffset = (uint)standardDirectory >> 16;

					ResultReadWrite result = await connection.ReadWriteAsync(
						(int)AdsIndexGroup.SYSTEMSERVICE_FDELETE,
						indexOffset,
						Memory<byte>.Empty,
						writeData.AsMemory(),
						cancel
					);
					result.ThrowOnError();
				}
			}
		}

		public static async Task RenameFileAsync(
			this IAdsConnection connection,
			string oldName,
			string newName,
			bool overwrite = false,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(oldName))
				throw new ArgumentNullException(nameof(oldName));

			if (string.IsNullOrEmpty(newName))
				throw new ArgumentNullException(nameof(newName));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			if (Path.GetExtension(oldName) != Path.GetExtension(newName))
			{
				throw new IOException("Invalid file extension");
			}

			string newPath = Path.Combine(
				Path.GetDirectoryName(oldName),
				Path.GetFileName(newName)
			);

			byte[] writeData = new byte[oldName.Length + 1 + newPath.Length + 1];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(oldName.ToCharArray());
					writer.Write('\0');
					writer.Write(newPath.ToCharArray());
					writer.Write('\0');

					AdsFileOpenMode remoteFileMode = (
						overwrite
							? AdsFileOpenMode.Overwrite
							: (
								~(
									AdsFileOpenMode.Read
									| AdsFileOpenMode.Write
									| AdsFileOpenMode.Append
									| AdsFileOpenMode.Plus
									| AdsFileOpenMode.Binary
									| AdsFileOpenMode.Text
								)
							)
					);
					uint indexOffset = (uint)(remoteFileMode | (AdsFileOpenMode)standardDirectory);

					ResultReadWrite result = await connection.ReadWriteAsync(
						(int)AdsIndexGroup.SYSTEMSERVICE_FRENAME,
						indexOffset,
						Memory<byte>.Empty,
						writeData.AsMemory(),
						cancel
					);
					result.ThrowOnError();
				}
			}
		}

		public static async Task CreateFileAsync(
			this IAdsConnection connection,
			string path,
			bool overwrite = false,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
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

			uint fileHandle = await connection.OpenFileAsync(
				path,
				AdsFileOpenMode.Write,
				standardDirectory,
				cancel
			);

			if (fileHandle <= 0)
			{
				throw new AdsErrorException(
					"Could not open file. Invalid handle.",
					AdsErrorCode.DeviceNotifyHandleInvalid
				);
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task WriteAllTextToFileAsync(
			this IAdsConnection connection,
			string path,
			string text,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			uint fileHandle = await connection.OpenFileAsync(
				path,
				AdsFileOpenMode.Write | AdsFileOpenMode.Text,
				standardDirectory,
				cancel
			);

			if (fileHandle <= 0)
			{
				throw new AdsErrorException(
					"Could not open file. Invalid handle.",
					AdsErrorCode.DeviceNotifyHandleInvalid
				);
			}

			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text ?? "")))
			{
				int bytesWritten = await connection.WriteFileAsync(fileHandle, stream, cancel);

				if (bytesWritten != stream.Length)
				{
					throw new Exception();
				}
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task WriteAllBytesToFileAsync(
			this IAdsConnection connection,
			string path,
			Memory<byte> bytes,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			if (bytes.IsEmpty)
				throw new ArgumentNullException(nameof(bytes));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			uint fileHandle = await connection.OpenFileAsync(
				path,
				AdsFileOpenMode.Write | AdsFileOpenMode.Binary,
				standardDirectory,
				cancel
			);

			if (fileHandle <= 0)
			{
				throw new AdsErrorException(
					"Could not open file. Invalid handle.",
					AdsErrorCode.DeviceNotifyHandleInvalid
				);
			}

			int bytesWritten = await connection.WriteFileAsync(fileHandle, bytes, cancel);

			if (bytesWritten != bytes.Length)
			{
				throw new Exception();
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task AppendAllTextToFileAsync(
			this IAdsConnection connection,
			string path,
			string text,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			uint fileHandle = await connection.OpenFileAsync(
				path,
				AdsFileOpenMode.Append | AdsFileOpenMode.Text,
				standardDirectory,
				cancel
			);

			if (fileHandle <= 0)
			{
				throw new AdsErrorException(
					"Could not open file. Invalid handle.",
					AdsErrorCode.DeviceNotifyHandleInvalid
				);
			}

			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text ?? "")))
			{
				int bytesWritten = await connection.WriteFileAsync(fileHandle, stream, cancel);

				if (bytesWritten != stream.Length)
				{
					throw new Exception();
				}
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task AppendAllBytesToFileAsync(
			this IAdsConnection connection,
			string path,
			Memory<byte> bytes,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			if (bytes.IsEmpty)
				throw new ArgumentNullException(nameof(bytes));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			uint fileHandle = await connection.OpenFileAsync(
				path,
				AdsFileOpenMode.Append | AdsFileOpenMode.Binary,
				standardDirectory,
				cancel
			);

			if (fileHandle <= 0)
			{
				throw new AdsErrorException(
					"Could not open file. Invalid handle.",
					AdsErrorCode.DeviceNotifyHandleInvalid
				);
			}

			int bytesWritten = await connection.WriteFileAsync(fileHandle, bytes, cancel);

			if (bytesWritten != bytes.Length)
			{
				throw new Exception();
			}

			await connection.CloseFileAsync(fileHandle, cancel);
		}

		public static async Task CopyFileAsync(
			this IAdsConnection connection,
			string source,
			string destination,
			bool overwrite = false,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(source))
				throw new ArgumentNullException(nameof(source));

			if (string.IsNullOrEmpty(destination))
				throw new ArgumentNullException(nameof(destination));

			if (!connection.IsConnected)
				throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException(
					"Invalid AMS Port. Connect to port 10000.",
					AdsErrorCode.InvalidAmsPort
				);

			if (Path.GetExtension(source) != Path.GetExtension(destination))
			{
				throw new IOException("Invalid file extension");
			}

			var sourceExists = await connection.FileExistsAsync(source, standardDirectory, cancel);

			if (!sourceExists)
			{
				throw new FileNotFoundException(source);
			}

			if (!overwrite)
			{
				var destinationExists = await connection.FileExistsAsync(
					destination,
					standardDirectory,
					cancel
				);

				if (destinationExists)
				{
					throw new IOException(
						"Destination file already exists. Possibly use override flag."
					);
				}
			}

			uint sourceFileHandle = await connection.OpenFileAsync(
				source,
				AdsFileOpenMode.Read | AdsFileOpenMode.Binary,
				standardDirectory,
				cancel
			);
			uint destinationFileHandle = await connection.OpenFileAsync(
				destination,
				AdsFileOpenMode.Write | AdsFileOpenMode.Binary,
				standardDirectory,
				cancel
			);

			try
			{
				int bytesRead = 0;
				byte[] readData = new byte[DefaultChunkSize];

				do
				{
					bytesRead = await connection.ReadFileAsync(
						sourceFileHandle,
						readData.AsMemory(),
						cancel
					);

					if (bytesRead > 0)
					{
						await connection.WriteFileAsync(
							destinationFileHandle,
							readData.AsMemory(),
							cancel
						);
					}
				} while (bytesRead > 0);
			}
			finally
			{
				await connection.CloseFileAsync(sourceFileHandle, cancel);
				await connection.CloseFileAsync(destinationFileHandle, cancel);
			}
		}

		public static async Task MoveFileAsync(
			this IAdsConnection connection,
			string source,
			string destination,
			bool overwrite = false,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
		{
			await connection.CopyFileAsync(
				source,
				destination,
				overwrite,
				standardDirectory,
				cancel
			);
			await connection.DeleteFileAsync(source, standardDirectory, cancel);
		}

		private static async Task<uint> OpenFileAsync(
			this IAdsConnection connection,
			string fileName,
			AdsFileOpenMode mode,
			AdsDirectory standardDirectory = AdsDirectory.Generic,
			CancellationToken cancel = default
		)
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
					ResultReadWrite result = await connection.ReadWriteAsync(
						(int)AdsIndexGroup.SYSTEMSERVICE_FOPEN,
						indexOffset,
						readData.AsMemory(),
						writeData.AsMemory(),
						cancel
					);
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

		private static async Task CloseFileAsync(
			this IAdsConnection connection,
			uint fileHandle,
			CancellationToken cancel = default
		)
		{
			ResultReadWrite result = await connection.ReadWriteAsync(
				(int)AdsIndexGroup.SYSTEMSERVICE_FCLOSE,
				fileHandle,
				Memory<byte>.Empty,
				Memory<byte>.Empty,
				cancel
			);
			result.ThrowOnError();
		}

		private static async Task<int> ReadFileAsync(
			this IAdsConnection connection,
			uint fileHandle,
			Memory<byte> readData,
			CancellationToken cancel = default
		)
		{
			ResultReadWrite result = await connection.ReadWriteAsync(
				(int)AdsIndexGroup.SYSTEMSERVICE_FREAD,
				fileHandle,
				readData,
				Memory<byte>.Empty,
				cancel
			);
			result.ThrowOnError();

			return result.ReadBytes;
		}

		private static async Task<int> ReadFileAsync(
			this IAdsConnection connection,
			uint fileHandle,
			Stream destination,
			CancellationToken cancel = default
		)
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
						reader.Write(readData, bytesRead, length);
						bytesRead += length;
					}
				} while (length > 0);
			}

			return bytesRead;
		}

		private static async Task<int> WriteFileAsync(
			this IAdsConnection connection,
			uint fileHandle,
			Stream source,
			CancellationToken cancel = default
		)
		{
			int bytesWritten = 0;

			using (BinaryReader reader = new BinaryReader(source, Encoding.Default))
			{
				do
				{
					byte[] writeData = reader.ReadBytes(DefaultChunkSize);
					int length = await connection.WriteFileAsync(
						fileHandle,
						writeData.AsMemory(),
						cancel
					);

					bytesWritten += length;
				} while (source.Length == bytesWritten);
			}

			return bytesWritten;
		}

		private static async Task<int> WriteFileAsync(
			this IAdsConnection connection,
			uint fileHandle,
			Memory<byte> writeData,
			CancellationToken cancel = default
		)
		{
			var result = await connection.ReadWriteAsync(
				(int)AdsIndexGroup.SYSTEMSERVICE_FWRITE,
				fileHandle,
				Memory<byte>.Empty,
				writeData,
				cancel
			);
			result.ThrowOnError();

			return writeData.Length;
		}
	}
}
