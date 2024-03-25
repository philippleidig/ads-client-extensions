using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads.Extensions.TypeSystem;
using TwinCAT.Ads.TypeSystem;

namespace TwinCAT.Ads.Extensions
{
	/// <summary>
	/// Provides extension methods for working with directories on a TwinCAT.Ads target system.
	/// </summary>
	public static partial class AdsClientExtensions
    {
		/// <summary>
		/// Asynchronously creates a directory within the boot folder on the ADS target system.
		/// </summary>
		public static Task CreateDirectoryInBootFolderAsync(this IAdsConnection connection, string path, CancellationToken cancel = default)
		{
			return connection.CreateDirectoryAsync(path, AdsDirectory.BootDir, cancel);
		}

		/// <summary>
		/// Asynchronously removes a directory within the boot folder on the ADS target system.
		/// </summary>
		public static Task RemoveDirectoryInBootFolderAsync(this IAdsConnection connection, string path, bool recursive = true, CancellationToken cancel = default)
		{
			return connection.DeleteDirectoryAsync(path, recursive, AdsDirectory.BootDir, cancel);
		}

		/// <summary>
		/// Asynchronously creates a directory on the ADS target system.
		/// </summary>
		public static async Task CreateDirectoryAsync(this IAdsConnection connection, string path, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

            if (connection.Address.Port != (int)AmsPort.SystemService) 
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

            byte[] writeData = new byte[path.Length + 1];

            using (MemoryStream writeStream = new MemoryStream(writeData))
            {
                using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
                {
                    writer.Write(path.ToCharArray());
                    writer.Write('\0');

					var indexOffset = (uint)standardDirectory >> 16;

					var result = await connection.ReadWriteAsync((uint)AdsIndexGroup.SYSTEMSERVICE_MKDIR, indexOffset, Memory<byte>.Empty, writeData.AsMemory(), cancel);
                    result.ThrowOnError();
                }
            }
        }

		/// <summary>
		/// Asynchronously removes a directory on the target system.
		/// </summary>
		public static async Task DeleteDirectoryAsync(this IAdsConnection connection, string path, bool recursive = true, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

            if (connection.Address.Port != (int)AmsPort.SystemService) 
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			var exists = await connection.ExistsDirectoryAsync(path, standardDirectory, cancel);

			if (!exists)
			{
				throw new DirectoryNotFoundException(path);
			}

			if (recursive)
			{
				await connection.DeleteDirectoryContentAsync(path, recursive, standardDirectory, cancel);
			}

			await connection.DeleteFolderAsync(path, standardDirectory, cancel);	
		}

		/// <summary>
		/// Asynchronously renames a directory on the target system.
		/// </summary>
		public static async Task RenameDirectoryAsync (this IAdsConnection connection, string oldDirectory, string newDirectory, bool overwrite, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(oldDirectory)) throw new ArgumentNullException(nameof(oldDirectory));

			if (string.IsNullOrEmpty(newDirectory)) throw new ArgumentNullException(nameof(newDirectory));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService) 
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			byte[] writeData = new byte[oldDirectory.Length + 1 + newDirectory.Length + 1];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(oldDirectory.ToCharArray());
					writer.Write('\0');
					writer.Write(newDirectory.ToCharArray());
					writer.Write('\0');

					AdsFileOpenMode remoteFileMode = (overwrite ? AdsFileOpenMode.Overwrite : (~(AdsFileOpenMode.Read | AdsFileOpenMode.Write | AdsFileOpenMode.Append | AdsFileOpenMode.Plus | AdsFileOpenMode.Binary | AdsFileOpenMode.Text)));
					uint indexOffset = (uint)remoteFileMode | (uint)standardDirectory;

					var result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FRENAME, indexOffset, Memory<byte>.Empty, writeData.AsMemory(), cancel);
					result.ThrowOnError();
				}
			}
		}

		/// <summary>
		/// Cleans up the boot folder on the target system.
		/// </summary>
		public static Task CleanUpBootFolder (this IAdsConnection connection, CancellationToken cancel = default)
		{
			return connection.DeleteDirectoryAsync("", true, AdsDirectory.BootDir, cancel);
		}

		/// <summary>
		/// Checks if a directory exists on the target system.
		/// </summary>
		public static async Task<bool> ExistsDirectoryAsync (this IAdsConnection connection, string directory, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException(nameof(directory));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			try
			{
				(AdsFileSystemEntry entry, ushort handle) = await connection.FindFileSystemEntryAsync(directory, "*.*", 0, standardDirectory, cancel);

				if (entry == null)
				{
					return false;
				}

				return entry.IsDirectory;
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
		}

		/// <summary>
		/// Enumerates files in a directory on the target system.
		/// </summary>
		public static async Task<IEnumerable<string>> EnumerateFilesAsync(this IAdsConnection connection, string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			var fileSystemEntries = await connection.EnumerateFileSystemEntriesAsync(path, searchPattern, searchOption, standardDirectory, cancel);

			return fileSystemEntries.Where(fileSystemEntry => !fileSystemEntry.IsDirectory)
									.Select(fileSystemEntry => fileSystemEntry.FullName)
									.ToList();
		}

		/// <summary>
		/// Enumerates file entries in a directory on the target system.
		/// </summary>
		public static async Task<IEnumerable<AdsFileEntry>> EnumerateFileEntriesAsync(this IAdsConnection connection, string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			var fileSystemEntries = await connection.EnumerateFileSystemEntriesAsync(path, searchPattern, searchOption, standardDirectory, cancel);
			return fileSystemEntries.Where(fileSystemEntry => !fileSystemEntry.IsDirectory).Cast<AdsFileEntry>().ToList();
		}

		/// <summary>
		/// Enumerates directories in a directory on the target system.
		/// </summary>
		public static async Task<IEnumerable<string>> EnumerateDirectoriesAsync(this IAdsConnection connection, string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			var fileSystemEntries = await connection.EnumerateFileSystemEntriesAsync(path, searchPattern, searchOption, standardDirectory, cancel);

			return fileSystemEntries.Where(fileSystemEntry => fileSystemEntry.IsDirectory)
									.Select(fileSystemEntry => fileSystemEntry.FullName)
									.ToList();
		}

		/// <summary>
		/// Enumerates directory entries in a directory on the target system.
		/// </summary>
		public static async Task<IEnumerable<AdsDirectoryEntry>> EnumerateDirectoryEntriesAsync(this IAdsConnection connection, string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			var fileSystemEntries = await connection.EnumerateFileSystemEntriesAsync(path, searchPattern, searchOption, standardDirectory, cancel);
			return fileSystemEntries.Where(fileSystemEntry => fileSystemEntry.IsDirectory).Cast<AdsDirectoryEntry>().ToList();
		}

		/// <summary>
		/// Enumerates files and directories in a directory on the target system.
		/// </summary>
		public static async Task<IEnumerable<AdsFileSystemEntry>> EnumerateFileSystemEntriesAsync (this IAdsConnection connection, string path, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			if (connection.Address.Port != (int)AmsPort.SystemService)
				throw new AdsErrorException("Invalid AMS Port. Connect to port 10000.", AdsErrorCode.InvalidAmsPort);

			if (string.IsNullOrEmpty(path))
			{
				path = "";
			}

			if (string.IsNullOrEmpty(searchPattern))
			{
				searchPattern = "*.*";
			}

			List<AdsFileSystemEntry> fileSystemEntries = new List<AdsFileSystemEntry>();

			bool isBusy = true;
			ushort handle = 0;
			AdsFileSystemEntry fileSystemEntry;

			do
			{
				try
				{
					(fileSystemEntry, handle) = await connection.FindFileSystemEntryAsync(path, searchPattern, handle, standardDirectory, cancel);

					if (fileSystemEntry.Name.Equals(".") || fileSystemEntry.Name.Equals(".."))
					{
						continue;
					}

					fileSystemEntries.Add(fileSystemEntry);

					if(searchOption == SearchOption.AllDirectories)
					{
						if (fileSystemEntry.IsDirectory)
						{
							string test = Path.Combine(path, fileSystemEntry.Name);
							IEnumerable<AdsFileSystemEntry> entries = await connection.EnumerateFileSystemEntriesAsync(test, searchPattern, searchOption, standardDirectory, cancel);

							fileSystemEntries.AddRange(entries);
						}
					}

				}
				catch (AdsErrorException ex) 
				{
					if(ex.ErrorCode == AdsErrorCode.DeviceNotFound)
					{
						isBusy = false;
					}
					else
					{
						throw ex;
					}
				}

			}
			while (isBusy);

			await connection.CloseHandleAsync(handle, cancel);

			return fileSystemEntries;
		}

		private static async Task<Tuple<AdsFileSystemEntry, ushort>> FindFileSystemEntryAsync(this IAdsConnection connection, string path, string searchPattern, ushort previousHandle = 0, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			AmsFileSystemEntry dataEntry = new AmsFileSystemEntry();
			dataEntry.Handle = previousHandle;

			byte[] readData = new byte[Marshal.SizeOf(dataEntry)];
			string search = Path.Combine(path, searchPattern);

			if (previousHandle == 0)
			{
				byte[] writeData = new byte[search.Length + 1];

				using (MemoryStream writeStream = new MemoryStream(writeData))
				{
					using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
					{
						writer.Write(search.ToCharArray());
						writer.Write('\0');

						uint indexOffset = (uint)standardDirectory >> 16;

						ResultReadWrite res = await connection.ReadWriteAsync((uint)AdsIndexGroup.SYSTEMSERVICE_FFILEFIND, indexOffset, readData.AsMemory(), writeData.AsMemory(), cancel);
						res.ThrowOnError();
					}
				}
			}
			else
			{
				ResultReadWrite res = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_FFILEFIND, previousHandle, readData.AsMemory(), Memory<byte>.Empty, cancel);
				res.ThrowOnError();
			}

			dataEntry = (AmsFileSystemEntry)connection.ByteArrayToStruct(readData, typeof(AmsFileSystemEntry));

			return Tuple.Create(AdsFileSystemEntryFactory.Create(dataEntry, path), dataEntry.Handle);
		}

		private static async Task DeleteFolderAsync(this IAdsConnection connection, string path, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			byte[] readData = new byte[0];
			byte[] writeData = new byte[path.Length + 1];

			using (MemoryStream writeStream = new MemoryStream(writeData))
			{
				using (BinaryWriter writer = new BinaryWriter(writeStream, Encoding.ASCII))
				{
					writer.Write(path.ToCharArray());
					writer.Write('\0');

					uint indexOffset = (uint)standardDirectory >> 16;

					ResultReadWrite result = await connection.ReadWriteAsync((int)AdsIndexGroup.SYSTEMSERVICE_RMDIR, indexOffset , readData.AsMemory(), writeData.AsMemory(), cancel);
					result.ThrowOnError();
				}
			}
		}

		private static async Task DeleteDirectoryContentAsync(this IAdsConnection connection, string path, bool recursive = true, AdsDirectory standardDirectory = AdsDirectory.Generic, CancellationToken cancel = default)
		{
			IEnumerable<AdsFileSystemEntry> fileSystemEntries = await connection.EnumerateFileSystemEntriesAsync(path, "*.*", SearchOption.TopDirectoryOnly, standardDirectory, cancel);

			if (fileSystemEntries == null)
			{
				return;
			}

			var isDirectoryEmpty = fileSystemEntries.Count() == 0;

			if (!recursive && !isDirectoryEmpty)
			{
				throw new IOException("Directory is not empty");
			}

			if (recursive && !isDirectoryEmpty)
			{
				foreach (var fileSystemEntry in fileSystemEntries)
				{
					if (fileSystemEntry.IsDirectory)
					{
						var subFolder = Path.Combine(path, fileSystemEntry.Name);

						if (recursive)
						{
							await connection.DeleteDirectoryContentAsync(subFolder, recursive, standardDirectory, cancel);
						}

						await connection.DeleteFolderAsync(subFolder, standardDirectory, cancel);
					}
					else
					{
						await connection.DeleteFileAsync(fileSystemEntry.FullName, standardDirectory, cancel);
					}
				}
			}
		}

		private static async Task CloseHandleAsync(this IAdsConnection connection, uint handle, CancellationToken cancel = default)
		{
			byte[] writeData = BitConverter.GetBytes(handle);

			ResultWrite result = await connection.WriteAsync((uint)AdsIndexGroup.SYSTEMSERVICE_CLOSEHANDLE, 0x0, writeData.AsMemory(), cancel);
			result.ThrowOnError();
		}

		private static object ByteArrayToStruct(this IAdsConnection connection, byte[] array, Type structType)
		{
			int offset = 0;
			if (structType.StructLayoutAttribute.Value != LayoutKind.Sequential)
				throw new ArgumentException("structType ist keine Struktur oder nicht Sequentiell.");

			int size = Marshal.SizeOf(structType);
			if (array.Length < offset + size)
				throw new ArgumentException("Byte-Array hat die falsche Länge.");

			byte[] tmp = new byte[size];
			Array.Copy(array, offset, tmp, 0, size);

			GCHandle structHandle = GCHandle.Alloc(tmp, GCHandleType.Pinned);
			object structure = Marshal.PtrToStructure(structHandle.AddrOfPinnedObject(), structType);
			structHandle.Free();

			return structure;
		}
	}
}
