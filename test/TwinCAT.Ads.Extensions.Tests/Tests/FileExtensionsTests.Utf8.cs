using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwinCAT.Ads.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	public partial class FileExtensionsTests
	{
		[TestMethod]
		public async Task UploadFileToTargetAsync_ShouldUploadFileWithUnicodeName_WhenUtf8()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			using (TemporaryFile file = new TemporaryFile(1024))
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				string extension = Path.GetExtension(file.Path);
				string destination = Path.Combine(directory.Path, "Prüfstandsmeßdaten_äöü" + extension);

				await adsClient.UploadFileToTargetAsync(
					file.Path,
					destination,
					overwrite: false,
					ensureDirectory: false,
					AdsDirectory.Generic,
					utf8: true
				);

				Assert.IsTrue(File.Exists(destination));
			}
		}

		[TestMethod]
		public async Task DeleteFileAsync_ShouldDeleteFileWithUnicodeName_WhenUtf8()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				string path = Path.Combine(directory.Path, "Meßö_ä.txt");
				File.WriteAllText(path, "content");

				await adsClient.DeleteFileAsync(path, AdsDirectory.Generic, utf8: true);

				Assert.IsFalse(File.Exists(path));
			}
		}

		[TestMethod]
		public async Task EnumerateFilesAsync_ShouldReturnUnicodeName_WhenUtf8()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			using (AdsClient adsClient = new AdsClient())
			{
				adsClient.Connect(TargetSystem, AmsPort.SystemService);

				string name = "Größenmeßung_ä.txt";
				File.WriteAllText(Path.Combine(directory.Path, name), "content");

				var files = await adsClient.EnumerateFilesAsync(
					directory.Path,
					"*.*",
					SearchOption.TopDirectoryOnly,
					AdsDirectory.Generic,
					utf8: true
				);

				Assert.IsTrue(files.Any(f => f.EndsWith(name)));
			}
		}
	}
}
