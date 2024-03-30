using System;
using System.Collections.Generic;
using System.IO;

namespace TwinCAT.Ads.Extensions.Tests
{
	public class TemporaryDirectory : IDisposable
	{
		private string _path;

		public TemporaryDirectory(string rootDirectory = "")
		{
			if (string.IsNullOrEmpty(rootDirectory))
			{
				_path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
			}
			else
			{
				if (!Directory.Exists(rootDirectory)){
					throw new IOException("Invalid root directory. Directory does not exist.");
				}

				_path = System.IO.Path.Combine(rootDirectory, Guid.NewGuid().ToString());
			}

			Directory.CreateDirectory(_path);
		}

		public TemporaryDirectory(IEnumerable<string> structure, string rootDirectory = "")
		{
			if (string.IsNullOrEmpty(rootDirectory))
			{
				_path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
			}
			else
			{
				if (!Directory.Exists(rootDirectory)){
					throw new IOException("Invalid root directory. Directory does not exist.");
				}

				_path = System.IO.Path.Combine(rootDirectory, Guid.NewGuid().ToString());
			}

			Directory.CreateDirectory(_path);
			CreateStructure(structure, _path);
		}

		private void CreateStructure(IEnumerable<string> structure, string currentPath)
		{
			foreach (string item in structure)
			{
				string itemPath = System.IO.Path.Combine(currentPath, item);
				if (System.IO.Path.HasExtension(itemPath)) 
				{
					File.Create(itemPath).Dispose();
				}
				else 
				{
					Directory.CreateDirectory(itemPath);
				}
			}
		}

		public string Path => _path;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Directory.Exists(_path))
				{
					Directory.Delete(_path, true);
				}
			}
		}
	}
}
