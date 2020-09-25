using Client.Core.Exceptions;
using Client.Core.Interfaces;
using Client.Core.Services;
using Client.Core.Services.Enums;
using System;
using System.Runtime.InteropServices;

using System.Threading.Tasks;
namespace Client.Core.Models
{
	public class ModBusItem :IItem
	{
		public ModBusItem() { }
		public ModBusItem(string ItemID,ItemType type, ModBusServer parent) {
			this.ItemID = ItemID;
			this.Parent = parent;
			this.ItemType = type;
			if (type == ItemType.Coils || type == ItemType.DiscreteInputs)
				this.DataType = VarEnum.VT_BOOL;
			if (type == ItemType.InputRegister || type == ItemType.HoldingRegister)
				this.DataType = VarEnum.VT_INT;
			if (type == ItemType.HoldingRegister || type == ItemType.Coils)
				this.AccessRights = AccessRights.READWRITEABLE;
			if (type == ItemType.DiscreteInputs || type == ItemType.InputRegister)
				this.AccessRights = AccessRights.READABLE;
		}


		public delegate void UpdatedEventHandler(object sender, ItemEventArgs args);
		public string ItemID { get; set; }
		public object Value { get; private set; }
		public readonly ModBusServer Parent;

		public event Interfaces.UpdatedEventHandler Updated;

		public DateTime LastUpdate { get; private set; }
		public VarEnum DataType { get; }
		public AccessRights AccessRights { get; }
		public ItemType ItemType { get; set; }
		public void OnUpdated(object newValue) { this.LastUpdate = DateTime.Now; this.Value = newValue; this.Updated?.Invoke(this, new ItemEventArgs { oldValue = this.Value, newValue = newValue }); }

		public void Read()
		{
			try
			{
			switch (this.DataType)
			{
				case VarEnum.VT_BOOL:					
					bool result;
					if (this.AccessRights == AccessRights.READWRITEABLE)
					{
						// read the coil like B0
						result = Parent.ReadCoil(int.Parse(this.ItemID));
					}
					else
					{
						// read the coil like b1
						result = Parent.ReadDiscreteInputs(int.Parse(this.ItemID));
					}

					if (this.Value == null)
					{
						OnUpdated(result);

					}
					else
					{
						if (result != (bool)this.Value)
							OnUpdated(result);
					}
					break;
				case VarEnum.VT_INT:
					int result2;
					if (this.AccessRights == AccessRights.READWRITEABLE)
					{
						try
						{
						result2 = Parent.ReadHoldingRegister(int.Parse(this.ItemID));

						}
						catch (Exception ex)
						{
							return;
						//throw;
						}
						//var x = Parent.modbusClient.ReadHoldingRegisters(0, 10);
					}
					else
					{
						 result2 = Parent.ReadInputRegister(int.Parse(this.ItemID));
					}

						if(this.Value == null)
							OnUpdated(result2);

						int.TryParse(this.Value.ToString(), out int actual);
						if (result2 != actual)
							OnUpdated(result2);
					break;
				default:
					break;
			}

			}
			catch (EasyModbus.Exceptions.StartingAddressInvalidException ex)
			{
				//Return an exception when the variable not exist in the server
				throw new ServerException("Starting address invalid", ex);
			}
			catch (Exception ex)
			{
				//other types of exceptions don't handle (mostly they are that the reading time ends)
				return;
			}
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
			Parent.WriteItem(ItemID, value);
		}

		public async Task WriteAsync(object value)
		{
			await Task.Run(() =>
			{
				Write(value);
			});
		}
	}


}
