using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	[TestClass]
	public partial class DirectoryExtensionsTests 
	{
		private readonly IEnumerable<string> WorkingDirectory;
		public DirectoryExtensionsTests()
		{
			WorkingDirectory = new List<string>
			{
				"File1.txt",
				"File2.txt",
				"File3.xml",
				"SubFolder_1",
				"SubFolder_2",
				"SubFolder_2/File1.txt",
				"SubFolder_2/File2.txt",
				"SubFolder_2/File3.xml",
				"SubFolder_2/SubSubFolder_1",
				"SubFolder_2/SubSubFolder_1/File1.txt",
				"SubFolder_2/SubSubFolder_1/File2.xml",
				"SubFolder_2/SubSubFolder_2",
				"SubFolder_2/SubSubFolder_2/File1.txt",
				"SubFolder_2/SubSubFolder_3",
			};
		}
	}
}