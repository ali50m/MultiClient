using System;
using System.Collections.Generic;
using Client.Core.Exceptions;
using Client.Core.Interfaces;
using Client.Core.Models;
using Client.Core.Services;
using Marcom.OPC.DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientOPC.Test
{
	[TestClass]
	public class FileManagerTest
	{
		/*
		Function SaveItems() 
		*/

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveItemsInXML_NullArguments_ThrowAnException()
		{
			// Arrange

			IFileService fileService = new XmlServices();
			// Act

			fileService.SaveItems(null, null);
			//Assert

			//don't use asserts for exception.
			
		}

		[TestMethod]
		[ExpectedException(typeof(FileException))]
		public void SaveItemsInXML_ListItemsEmpty_ThrowAnException()
		{
			//Arrange
			IFileService fileService = new XmlServices();
			//Act
			bool result=fileService.SaveItems(new List<IItem>());
			//Assert
			//Assert.IsTrue(result);
		}

		[TestMethod]		
		public void SaveItemsInXML_ListItems_ReturnTrue()
		{
			//Arrange
			IFileService fileService = new XmlServices();
			//Act
			IList<IItem> items = new List<IItem>()
			{
				new ModBusItem(),
				new ModBusItem()
			};
			bool result = fileService.SaveItems(items);
			//Assert
			Assert.IsTrue(result);
		}

		/*
			Function ReadItems() 
		*/


		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ReadItemsInXML_NullArgument_ThrowAnException()
		{
			//arrange
			IFileService fileService = new XmlServices();
			//Act
			IList<IItem> result=fileService.ReadItems(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ReadItemsInXML_EmptyArgument_ThrowAnException()
		{
			//arrange
			IFileService fileService = new XmlServices();
			//Act
			IList<IItem> result = fileService.ReadItems("");
		}

		[TestMethod]
		[ExpectedException(typeof(FileException))]
		public void ReadItemsInXML_PathFileNotXML_ThrowAnException()
		{
			//arrange
			IFileService fileService = new XmlServices();
			//Act
			IList<IItem> result = fileService.ReadItems("   ");

			
		}
	}
}
