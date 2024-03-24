using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TwinCAT.Ads.Extensions.TypeSystem
{
	[StructLayout(LayoutKind.Sequential, Pack = 0)]
	internal struct AmsFileSystemEntry
	{
		public ushort Handle;
		public ushort Reserved;

		public uint FileAttributes;
		public long CreationTime;
		public long LastAccessTime;
		public long LastWriteTime;
		public long FileSize;
		public uint Reserved0;
		public uint Reserved1;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string FileName;
		public uint Reserved2;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		public string AlternateFileName;
		public ushort Reserved3;
	}
}
