using System;
using System.IO;
using TwinCAT.Ads.Extensions.TypeSystem;

namespace TwinCAT.Ads.TypeSystem
{
	public abstract class AdsFileSystemEntry
	{
		public FileAttributes Attributes { get; private set; }
		public DateTime CreationTime { get; private set; }
		public DateTime CreationTimeUtc => CreationTime.ToUniversalTime();
		public DateTime LastAccessTime { get; private set; }
		public DateTime LastAccessTimeUtc => LastAccessTime.ToUniversalTime();
		public DateTime LastWriteTime { get; private set; }
		public DateTime LastWriteTimeUtc => LastWriteTime.ToUniversalTime();
		public string Name { get; private set; }
		public string FullName { get; private set; }
		public bool IsDirectory => (Attributes & FileAttributes.Directory) == FileAttributes.Directory;

		internal AdsFileSystemEntry(AmsFileSystemEntry entry, string path)
		{
			Name = entry.FileName;
			FullName = Path.Combine(path, Name);
			Attributes = (FileAttributes)entry.FileAttributes;
			CreationTime = DateTime.FromFileTime(entry.CreationTime);
			LastAccessTime = DateTime.FromFileTime(entry.LastAccessTime);
			LastWriteTime = DateTime.FromFileTime(entry.LastWriteTime);			
		}
	}
}
