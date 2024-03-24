using System.IO;
using TwinCAT.Ads.TypeSystem;

namespace TwinCAT.Ads.Extensions.TypeSystem
{
	internal class AdsFileSystemEntryFactory
	{
		internal static AdsFileSystemEntry Create(AmsFileSystemEntry entry, string path)
		{
			FileAttributes fileAttributes = (FileAttributes)entry.FileAttributes;

			if (fileAttributes.HasFlag(FileAttributes.Directory))
			{
				return new AdsDirectoryEntry(entry, path);
			}
			return new AdsFileEntry(entry, path);
		}
	}
}
