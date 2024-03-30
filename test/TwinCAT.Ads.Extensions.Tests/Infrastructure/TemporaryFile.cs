using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinCAT.Ads.Extensions.Tests
{
	public class TemporaryFile : IDisposable
	{
		private bool _isDisposed;

		public bool Keep { get; set; }
		public string Path { get; private set; }

		public TemporaryFile() 
			: this(false, 0)
		{
		}

		public TemporaryFile(long size)
			: this(false, size)
		{
		}

		public TemporaryFile(bool shortLived, long size)
		{
			this.Path = CreateTemporaryFile(shortLived);

			if(size > 0)
			{
				AppendData(size);
			}
		}

		~TemporaryFile()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(false);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				_isDisposed = true;

				if (!this.Keep)
				{
					TryDelete();
				}
			}
		}

		private void TryDelete()
		{
			try
			{
				File.Delete(this.Path);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		public static string CreateTemporaryFile(bool shortLived)
		{
			string temporaryFile = System.IO.Path.GetTempFileName();

			if (shortLived)
			{
				File.SetAttributes(temporaryFile,
					File.GetAttributes(temporaryFile) | FileAttributes.Temporary);
			}

			return temporaryFile;
		}

		private void AppendData(long size)
		{
			FileStream fs = new FileStream(Path, FileMode.Open);
			fs.Seek(size, SeekOrigin.Begin);
			fs.WriteByte(0);
			fs.Close();
		}
	}
}
