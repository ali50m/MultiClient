using Client.Core.Exceptions;
using Client.Core.Interfaces;
using Client.Core.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TitaniumAS.Opc.Client.Da;

namespace Client.Core.Models
{
	public class OPCDAItem : IItem

	{
		public OPCDAItem() { }

		public OPCDAItem(OpcDaItemResult item)
		{
			this.ItemID = item.Item.ItemId;
			this.Parent = item.Item.Group;
			this.DataType = item.Item.CanonicalDataType;			
			this.ItemType = ItemType.DA;
			if (item.Item.AccessRights == OpcDaAccessRights.Read)
				this.AccessRights = AccessRights.READABLE;
			if (item.Item.AccessRights == OpcDaAccessRights.ReadWrite)
				this.AccessRights = AccessRights.READWRITEABLE;

		}
		private OpcDaGroup Parent;

		public string ItemID { get; private set; }

		public object Value { get; private set; }

		public DateTime LastUpdate { get; private set; }

		public AccessRights AccessRights { get; private set; }

		public Type DataType  { get; private set; }

		public ItemType ItemType { get; set; }

		public event UpdatedEventHandler Updated;

		public void OnUpdated(object newValue) { this.LastUpdate = DateTime.Now; this.Value = newValue; this.Updated?.Invoke(this, new ItemEventArgs { oldValue = this.Value, newValue = newValue }); }

		public void Read()
		{
			if (Parent == null)
				throw new ServerException("The parent is null");
			if (string.IsNullOrEmpty(this.ItemID))
				throw new ServerException("The itemID is null or empty");
			var result=Parent.Read(Parent.Items.Where(x => x.ItemId == this.ItemID).ToList(), OpcDaDataSource.Cache);
			OnUpdated(result[0].Value);
		}

		public async Task ReadAsync()
		{
			await Task.Run(() =>
			{
				Read();
			}

			);
		}

		public void Write(object value)
		{

		
		}

		public async Task WriteAsync(object value)
		{
			await Task.Run(() =>
			{
				Write(value);
			}

				);
		}
	}
}
