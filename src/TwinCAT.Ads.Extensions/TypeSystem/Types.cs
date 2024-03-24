using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TwinCAT.Ads.TypeSystem
{
	internal enum AdsFileOpenMode : uint
	{
		Read = 1U,
		Write = 2U,
		Append = 4U,
		Plus = 8U,
		Binary = 16U,
		Text = 32U,
		EnsureDirectory = 64U,
		EnableDirectory = 128U,
		Overwrite = 256U,
		Mask_Directory = 4294901760U,
		All = 63U,
		None = 4294967232U
	}

	public enum AdsDirectory : uint
	{
		Generic = 65536U,
		BootProject = 131072U,
		BootData = 196608U,
		BootDir = 262144U,
		TargetDir = 327680U,
		ConfigDir = 393216U,
		InstallDir = 458752U,
		None = 65535U,
		All = 4294901760U
	}

	internal enum AdsIndexGroup : int
	{
		SYSTEMSERVICE_OPENCREATE = 100,
		SYSTEMSERVICE_OPENREAD = 101,
		SYSTEMSERVICE_OPENWRITE = 102,
		SYSTEMSERVICE_CREATEFILE = 110,
		SYSTEMSERVICE_CLOSEHANDLE = 111,
		SYSTEMSERVICE_FOPEN = 120,
		SYSTEMSERVICE_FCLOSE = 121,
		SYSTEMSERVICE_FREAD = 122,
		SYSTEMSERVICE_FWRITE = 123,
		SYSTEMSERVICE_FEOF = 130,
		SYSTEMSERVICE_FDELETE = 131,
		SYSTEMSERVICE_FRENAME = 132,

		SYSTEMSERVICE_FFILEFIND = 133,

		SYSTEMSERVICE_MKDIR = 138,
		SYSTEMSERVICE_RMDIR = 139,
	}
}
