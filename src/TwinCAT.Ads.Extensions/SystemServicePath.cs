using System;
using System.Text;

namespace TwinCAT.Ads.Extensions
{
	/// <summary>
	/// Encodes paths for the TwinCAT system service and provides the UTF-8 flag
	/// bits used in the ADS index offset.
	/// </summary>
	/// <remarks>
	/// The system service interprets a path either in the target's default 8-bit
	/// code page or, when the request opts in, as UTF-8. This mirrors the behaviour
	/// of the TcXaeMgmt implementation (RemoteFile / RemoteIO): the UTF-8 flag is
	/// signalled in the index offset and the path bytes are UTF-8 encoded. UTF-8 is
	/// the only deterministic, platform-independent choice for non-ASCII names on
	/// Windows, Windows CE, TwinCAT/BSD and Linux.
	/// </remarks>
	internal static class SystemServicePath
	{
		private static readonly Encoding Utf8Encoding = new UTF8Encoding(false);

		/// <summary>
		/// UTF-8 flag for functions whose index offset carries the standard directory
		/// in the high 16 bits (FOPEN, FDELETE, FRENAME): <c>RemoteFileMode.Utf8</c>.
		/// </summary>
		public const uint FileFlagUtf8 = 0x1000;

		/// <summary>
		/// UTF-8 flag for functions whose index offset carries the standard directory
		/// in the low 16 bits (FFILEFIND, MKDIR, RMDIR): bit 16.
		/// </summary>
		public const uint DirectoryFlagUtf8 = 0x10000;

		public static Encoding GetEncoding(bool utf8)
		{
			return utf8 ? Utf8Encoding : Encoding.ASCII;
		}

		/// <summary>
		/// Encodes <paramref name="path"/> into a NUL-terminated buffer. The buffer is
		/// sized from the encoded byte count (not the character count) so multi-byte
		/// UTF-8 names are not truncated.
		/// </summary>
		public static byte[] ToBytes(string path, bool utf8)
		{
			Encoding encoding = GetEncoding(utf8);
			int count = encoding.GetByteCount(path);
			byte[] buffer = new byte[count + 1]; // single 0x00 terminator for ASCII and UTF-8
			encoding.GetBytes(path, 0, path.Length, buffer, 0);
			return buffer;
		}

		/// <summary>
		/// Encodes two paths into a single buffer, each NUL-terminated. Used by the
		/// rename services (FRENAME) which expect "source\0target\0".
		/// </summary>
		public static byte[] ToBytes(string first, string second, bool utf8)
		{
			byte[] a = ToBytes(first, utf8);
			byte[] b = ToBytes(second, utf8);
			byte[] buffer = new byte[a.Length + b.Length];
			Array.Copy(a, 0, buffer, 0, a.Length);
			Array.Copy(b, 0, buffer, a.Length, b.Length);
			return buffer;
		}
	}
}
