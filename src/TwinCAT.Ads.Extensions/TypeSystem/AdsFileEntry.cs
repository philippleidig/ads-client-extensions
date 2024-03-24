using TwinCAT.Ads.Extensions.TypeSystem;

namespace TwinCAT.Ads.TypeSystem
{
	public sealed class AdsFileEntry : AdsFileSystemEntry
	{
		public long FileSize { get; private set; }

		internal AdsFileEntry(AmsFileSystemEntry entry, string path)
			: base(entry, path)
		{
			FileSize = entry.FileSize;
		}
	}
}
