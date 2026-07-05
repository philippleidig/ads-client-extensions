namespace TwinCAT.Ads.Extensions
{
	/// <summary>
	/// Helpers for manipulating paths that live on the <b>remote</b> ADS target.
	/// </summary>
	/// <remarks>
	/// Remote paths must never be processed with <see cref="System.IO.Path"/> or
	/// <see cref="System.IO.DirectoryInfo"/>: those APIs use the separator rules of
	/// the <i>client</i> operating system and resolve against the client filesystem.
	/// A client running on Linux or TwinCAT/BSD would therefore mishandle a Windows
	/// (CE) target path and vice versa. These helpers treat both '\' and '/' as
	/// separators regardless of the client platform and never touch the local disk.
	/// </remarks>
	internal static class RemotePath
	{
		private static readonly char[] Separators = { '\\', '/' };

		/// <summary>
		/// Returns the leaf (file or directory) name of a remote path.
		/// </summary>
		public static string GetFileName(string path)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			string trimmed = path.TrimEnd(Separators);
			int index = trimmed.LastIndexOfAny(Separators);
			return index < 0 ? trimmed : trimmed.Substring(index + 1);
		}

		/// <summary>
		/// Replaces the leaf name of <paramref name="path"/> with <paramref name="newName"/>,
		/// keeping the parent directory and the separator style of the original path.
		/// </summary>
		public static string ChangeName(string path, string newName)
		{
			if (string.IsNullOrEmpty(path))
				return newName;

			int index = path.TrimEnd(Separators).LastIndexOfAny(Separators);
			if (index < 0)
				return newName;

			// Keep the original separator (path[index]) so the result stays in the
			// separator style the caller/target uses.
			return path.Substring(0, index + 1) + newName;
		}
	}
}
