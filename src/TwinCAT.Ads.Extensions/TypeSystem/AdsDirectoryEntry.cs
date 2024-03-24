using System;
using System.Collections.Generic;
using System.Text;
using TwinCAT.Ads.Extensions.TypeSystem;

namespace TwinCAT.Ads.TypeSystem
{
	public sealed class AdsDirectoryEntry : AdsFileSystemEntry
	{
		internal AdsDirectoryEntry(AmsFileSystemEntry entry, string path)
			: base(entry, path)
		{

		}
	}
}
