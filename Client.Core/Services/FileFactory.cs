using Client.Core.Interfaces;
using Client.Core.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core.Services
{
	public class FileFactory
	{
		/// <summary>
		/// Factory method for the File Manager
		/// </summary>
		/// <param name="fileType"> Type for the manager</param>
		/// <returns> return instance of Manager</returns>
		public IFileService GetManager(FileType fileType)
		{
			switch (fileType)
			{
				case FileType.XML:
					return new XmlServices();
				case FileType.JSON:
					return new XmlServices();
				default:
					return null;

			}

		}
	}
}
