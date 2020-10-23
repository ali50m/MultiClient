
using Client.Core.Services.Enums;
using System.Runtime.InteropServices;


namespace Client.Models
{
	public interface IItemView
	{
		string ItemID { get;}
		object Value { get; }
		VarEnum DataType { get; }
		ItemType ItemType { get; }
	}
}