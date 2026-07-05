using System;
using System.Text;

namespace TwinCAT.Ads.Extensions.TypeSystem
{
	/// <summary>
	/// Managed representation of the WIN32_FIND_DATA payload returned by the
	/// TwinCAT system service (SYSTEMSERVICE_FFILEFIND).
	/// </summary>
	/// <remarks>
	/// The payload is parsed manually instead of using
	/// <see cref="System.Runtime.InteropServices.Marshal.PtrToStructure(System.IntPtr, System.Type)"/>
	/// so the behaviour is identical on Windows, Windows CE, TwinCAT/BSD and Linux
	/// and does not depend on processor alignment or the platform ANSI code page.
	/// The layout mirrors the implementation shipped in TcXaeMgmt.
	/// </remarks>
	internal sealed class AmsFileSystemEntry
	{
		/// <summary>
		/// Size of the payload in bytes: 4 (handle) + 320 (WIN32_FIND_DATA) + 4 (trailing).
		/// </summary>
		public const int MarshalSize = 328;

		public uint Handle;
		public uint FileAttributes;
		public long CreationTime;
		public long LastAccessTime;
		public long LastWriteTime;
		public long FileSize;
		public string FileName = string.Empty;
		public string AlternateFileName = string.Empty;

		public static AmsFileSystemEntry FromBytes(byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			if (data.Length < MarshalSize)
				throw new ArgumentException("Invalid file find data length.", nameof(data));

			uint fileSizeHigh = BitConverter.ToUInt32(data, 32);
			uint fileSizeLow = BitConverter.ToUInt32(data, 36);

			return new AmsFileSystemEntry
			{
				Handle = BitConverter.ToUInt32(data, 0),
				FileAttributes = BitConverter.ToUInt32(data, 4),
				CreationTime = BitConverter.ToInt64(data, 8),
				LastAccessTime = BitConverter.ToInt64(data, 16),
				LastWriteTime = BitConverter.ToInt64(data, 24),
				FileSize = ((long)fileSizeHigh << 32) | fileSizeLow,
				FileName = ReadString(data, 48, 260),
				AlternateFileName = ReadString(data, 308, 14),
			};
		}

		private static string ReadString(byte[] data, int offset, int maxLength)
		{
			int length = 0;
			while (length < maxLength && data[offset + length] != 0)
			{
				length++;
			}

			return Encoding.UTF8.GetString(data, offset, length);
		}
	}
}
