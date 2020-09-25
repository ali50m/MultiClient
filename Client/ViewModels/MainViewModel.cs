using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Client.Models;
using Client.Core.Interfaces;
using Microsoft.Win32;
using Client.Core.Services;
using Client.Core.Services.Enums;
using Client.Models.Common;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Client.Core.Exceptions;
using Client.Windows;
using Client.Messaging;
using System.Windows.Threading;
using log4net;

namespace Client.ViewModels
{
	public class MainViewModel : BaseModel
	{
		private IServer Server;
		private IFileService _fileManager;
		private ILog _Logger= LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ServerFactory ServerFactory = new ServerFactory();
		private FileFactory FileFactory = new FileFactory();
		

		public MainViewModel()
		{
			ListOfItemsActive = new ObservableCollection<IItemView>();
			//ListServers = new List<OPCServerName>();
			//ListServers.AddRange(MarcomServer.GetServers());
			
		}

		#region ModBus
			private List<ClientType> _listClients;

			public List<ClientType> ListClients
			{
				get {
					var list = new List<ClientType>();
					foreach (ClientType item in Enum.GetValues(typeof(ClientType)))
						list.Add(item);
					return list;
					}
				private set { _listClients = value; OnPropertyChange(); }
			}

			private ClientType _selectedClient;

			public ClientType SelectedClient
			{
				get { return _selectedClient; }
				set { _selectedClient = value; OnPropertyChange(); }
			}

			private ItemType _modBusType;

			public ItemType ModBusType
			{
				get { return _modBusType; }
				set { _modBusType = value; OnPropertyChange(); }
			}

			private string _modbusAddress;
			public string ModbusAddress
			{
				get { return _modbusAddress; }
				set { _modbusAddress = value; OnPropertyChange(); }
			}

			/*ADD Item Modbus Click.*/
			private RelayCommand _addModbusCommand;
			public ICommand AddModbusCommand
			{
				get
				{
					if (_addModbusCommand == null)
					{
						_addModbusCommand = new RelayCommand(AddModbus);
					}
					return _addModbusCommand;
				}
			}
			public void AddModbus(object parameter)
			{
			try
			{
				if (parameter == null) { Messenger.Default.Send(new Messaging.ShowMessage("You have to insert the address.")); return; }
				if (ModBusType == ItemType.None) { Messenger.Default.Send(new Messaging.ShowMessage("You have to select a type.")); return; }
				IItem Item = Server.CreateItem(parameter.ToString(), ModBusType);
				if (Item != null)
				{
					ListOfItemsActive.Add(new ItemViewModel(Item));				
				}
				else
				{
					Messenger.Default.Send(new Messaging.ShowMessage("The item is already in the list."));
				}
			}
			catch (Exception ex)
			{
				Messenger.Default.Send(new Messaging.ShowMessage("Error trying to adding an item \n Error: "+ex.Message));
				_Logger.Error("Error trying to adding an item \n "+ ex.Message );
			}
				

			}
		
		/*ADD RADIO button event.*/
		private RelayCommand _modBusTypeRadioButtonCommand;
		public ICommand ModBusTypeRadioButtonCommand
		{
			get
			{
				if (_modBusTypeRadioButtonCommand == null)
				{
					_modBusTypeRadioButtonCommand = new RelayCommand(ModBusTypeRadioButton);
				}
				return _modBusTypeRadioButtonCommand;
			}
		}
		public void ModBusTypeRadioButton(object parameter)
		{
			if (parameter == null) return;
			switch (parameter.ToString())
			{
				case nameof(ItemType.Coils):
					ModBusType = ItemType.Coils;
					break;
				case nameof(ItemType.DiscreteInputs):
					ModBusType = ItemType.DiscreteInputs;
					break;
				case nameof(ItemType.HoldingRegister):
					ModBusType = ItemType.HoldingRegister;
					break;
				case nameof(ItemType.InputRegister):
					ModBusType = ItemType.InputRegister;
					break;
				
			}
		}

		#endregion	


		private String _IpAddress;
		public String IpAddress {
			get
			{
				return _IpAddress;
			}
			set
			{
				_IpAddress = value;
				//Server1.Ip = _IpAddress;
				OnPropertyChange();
			}

			}


	//private List<OPCServerName> _ListServers;
	//	public List<OPCServerName> ListServers
	//	{
	//		get
	//		{
	//			return _ListServers;
	//		}
	//		set
	//		{
	//			_ListServers = value;
	//			OnPropertyChange();
	//		}
	//	}

	//	private OPCServerName _ItemSelected;
	//	public OPCServerName ItemSelected
	//	{
	//		get
	//		{
	//			return _ItemSelected;
	//		}
	//		set
	//		{
	//			_ItemSelected = value;
	//			OnPropertyChange();
	//		}
	//	}

		private Dictionary<string, List<string>> _TreeListItems;
		public Dictionary<string, List<string>> TreeListItems
		{
			get { return _TreeListItems; }
			set
			{
				_TreeListItems = value;
				OnPropertyChange();
			}
		}

		private string _TextTable;
		public string TextTable {
			get
			{
				return _TextTable;
			}
			set
			{
				_TextTable = value;
				OnPropertyChange();
			}
}

		private ObservableCollection<IItemView> _listOfItemsActive;
		public ObservableCollection<IItemView> ListOfItemsActive
		{
			get {

				return _listOfItemsActive;
			}
			set {
				_listOfItemsActive = value;
				OnPropertyChange();
			}
		}
	
		/* Status Of Connection for change the name of the button */
		public String _TextButtonConect=StatusConnection.Connect;
		public String TextButtonConect {
			get {
				
					
					return _TextButtonConect;
				 }
				set
				{
					_TextButtonConect = value;
					OnPropertyChange();
		
				}
			}


		/* variable isConnected for Enable Or Disabled elements in the form  */
		private bool _isConnected;
		public bool isConnected
		{
			get
			{
				return _isConnected;
			}
			set
			{
				_isConnected = value;
				OnPropertyChange();
			}
		}

		/* Button Connect */
		private RelayCommand _ConnectClick;
		public ICommand ConnectClick
		{
			get
			{
				if (_ConnectClick == null)
				{
					//	_buttonClick = new RelayCommand(CellEdit(_gridItem), CanCellEdit);
					_ConnectClick = new RelayCommand(async (parameter)=> { await  ClickButtonConnect(parameter); },CanExecuteConnect );

				}
				return _ConnectClick;
			}
		}

		private bool CanExecuteConnect(object obj)
		{
			return !IsBusy;
		}

		public async Task ClickButtonConnect(object parameter)
		{
			//if(SelectedClient == ClientType.MarcomDA && ItemSelected == null) { Messenger.Default.Send(new Messaging.ShowMessage("You have to select a ServerName"));/* MessageBox.Show("You have to select a ServerName");*/ return; }
			await Task.Run(() =>
					{
						try
						{
						IsBusy = true;
						ConnectClick.CanExecute(null);
							if (Server == null)
							{

								Server = ServerFactory.CreateServer(SelectedClient);
								Server.Ip = IpAddress;
								Server.Port = 502;
								Server.Name = "Modbus";
								Server.Connect();
								TextButtonConect = StatusConnection.Disconnect;
								isConnected = true;
								TreeListItems = Server.GetListItems();
								_Logger.Info("Connected to " +IpAddress + " Server "+ SelectedClient + " successful");
							}
							else
							{
								Server.Disconnect();
								//TreeListItems = null;
								//This code need to run in the UI Thread so invoke a delegate to handle this job.
								Application.Current.Dispatcher.Invoke((System.Action)delegate
								{
									TreeListItems = new Dictionary<string, List<string>>();
									ListOfItemsActive.Clear();
								});							
								TextButtonConect = StatusConnection.Connect;
								isConnected = false;
								Server = null;
							}
						}
						catch (Exception ex)
						{
							Server = null;
							_Logger.Error("Occurred an error trying to Connect/Disconnect to the server:\n "+ ex.Message + "\n"+ex.StackTrace);
							Messenger.Default.Send(new Messaging.ShowMessage(ex.Message+"\n" + ex.StackTrace));
						}
						
							IsBusy = false;
					});
		}

		/*ADD Item Button Double Click.*/
		private RelayCommand _DoubleClick;
		public ICommand DoubleClick
		{
			get
			{
				if (_DoubleClick == null)
				{
					_DoubleClick = new RelayCommand(AddItem);
				}
				return _DoubleClick;
			}
		}
		public void AddItem(object parameter)
		{
			if(parameter == null) { return; }
			string itemName;
			if (parameter.GetType() == typeof(ItemListStruct) )
				{
					ItemListStruct item = (ItemListStruct)parameter;
					 itemName = item.ItemID;

			}
			else
			{
				itemName = parameter.ToString();
			}
			try
			{
				IItem oPCItem = Server.CreateItem(itemName);
				if (oPCItem != null)
				{
					ListOfItemsActive.Add(new ItemViewModel(oPCItem));
				}
				else
				{
					Messenger.Default.Send(new Messaging.ShowMessage("The item is already in the list."));
				}

			}
			catch (Exception ex)
			{
				_Logger.Error("Occurred an exception trying to added an item \n"+ex.Message +"\n" +ex.StackTrace);			
				Messenger.Default.Send(new Messaging.ShowMessage(ex.Message,ex));
			}

		}


		/*Button Remove Click*/
		private RelayCommand _buttonRemoveItem;
		public ICommand ButtonRemoveItem
		{
			get
			{
				if (_buttonRemoveItem == null)
				{
					_buttonRemoveItem = new RelayCommand(RemoveItem);
				}
				return _buttonRemoveItem;
			}
		}

		private void RemoveItem(object parameter){
			IItemView item = (IItemView)parameter;
			if (item != null)
			{
				switch (SelectedClient)
				{					
					case ClientType.Modbus:
						Server.RemoveItem(item.ItemID,item.ItemType);
						ListOfItemsActive.Remove(item);
						break;
					default:
						break;
				}
			}
			else
			{				
				Messenger.Default.Send(new Messaging.ShowMessage("You have to select an item"));
			}
		}

		/* FIND ITEM ON TEXT CHANGE*/
		private RelayCommand _TxtChangeCommand;
		public ICommand TxtChangeCommand
		{
			get
			{
				if (_TxtChangeCommand == null)
				{
					_TxtChangeCommand = new RelayCommand(FindItem);
				}
				return _TxtChangeCommand;
			}
		}
		private void FindItem(object parameter){

			TreeListItems = Server.GetListItems(parameter.ToString());
		}

		/* Button Remove All*/
		private RelayCommand _buttonRemoveAll;
		public ICommand ButtonRemoveAll
		{
			get
			{
				if (_buttonRemoveAll == null)
				{
					_buttonRemoveAll = new RelayCommand(RemoveAll);
				}
				return _buttonRemoveAll;
			}
		}
		private void RemoveAll(object obj)
		{
			switch (SelectedClient)
			{
				case ClientType.Modbus:
					Server.RemoveAll();
					ListOfItemsActive.Clear();
					break;
				default:
					break;
			}		
		}
		/* button SAVE */
		public RelayCommand _ButtonSave;
		public ICommand ButtonSave
		{
			get
			{
				if (_ButtonSave == null)
				{
					_ButtonSave = new RelayCommand(SaveItems);
				}
				return _ButtonSave;
			}
		}
		public void SaveItems(object parameter)
		{
			SaveFileDialog SaveFileDialog = new SaveFileDialog();
			SaveFileDialog.Filter = "XML file (*.xml; *.xml) | *.xml;";
			if (SaveFileDialog.ShowDialog() == true)
			{
			_fileManager = FileFactory.GetManager(FileType.XML);
			if (_fileManager.SaveItems(Server.Items, SaveFileDialog.FileName))
					//MessageBox.Show("Items Saved");
				Messenger.Default.Send(new Messaging.ShowMessage("Items Saved"));
			}
		}

		/* button LOAD */
		public RelayCommand _ButtonLoad;
		public ICommand ButtonLoad
		{
			get
			{
				if (_ButtonLoad == null)
				{
					_ButtonLoad = new RelayCommand(LoadItems);
				}
				return _ButtonLoad;
			}
		}
		public void LoadItems(object parameter)
		{

			OpenFileDialog OpenFileDialog = new OpenFileDialog();
			OpenFileDialog.Filter = "XML file (*.xml; *.xml) | *.xml;";
			if (OpenFileDialog.ShowDialog() == true)
			{
				try
				{

					var items = _fileManager.ReadItems(OpenFileDialog.FileName);

					foreach (var item in items)
					{
						IItem itemCreated = Server.CreateItem(item.ItemID);
						if (itemCreated != null)
							ListOfItemsActive.Add(new ItemViewModel(itemCreated));					
					}
				}
				catch (Exception ex)
				{
					_Logger.Error("Occurred an exception trying to load the items \n" + ex.Message + "\n" + ex.StackTrace);
					Messenger.Default.Send(new Messaging.ShowMessage(ex.Message));
					
				}
			}
		}


		/* FIND ITEM ACTIVE ON TEXT CHANGE*/
		private RelayCommand _TxtChangeCommand2;
		public ICommand TxtChangeCommand2
		{
			get
			{
				if (_TxtChangeCommand2 == null)
				{
					_TxtChangeCommand2 = new RelayCommand(FindItemAct);
				}
				return _TxtChangeCommand2;
			}
		}
		private void FindItemAct(object parameter){
			TextTable = parameter.ToString();
			ListOfItemsActive = new ObservableCollection<IItemView>(Server.Items.Where(x=> x.ItemID.Contains(parameter.ToString())).Select(y => new ItemViewModel(y)).ToList());
		}


		/* UPDATE ITEM*/
		private RelayCommand _UpdateElement;
		public ICommand UpdateElement
		{
			get
			{
				if (_UpdateElement == null)
				{
					_UpdateElement = new RelayCommand(UpdateOPC);
				}
				return _UpdateElement;
			}
		}
		private void UpdateOPC(object parameter)
		{

			try
			{
				IItemView item = (IItemView)parameter;
				WindowUpdate pad = new WindowUpdate(item);
				//OPCItem item = (OPCItem)parameter;
				if (pad.ShowDialog() == true)
				{

					switch (SelectedClient)
					{
						case ClientType.Modbus:
							if (item.ItemType == ItemType.HoldingRegister || item.ItemType == ItemType.Coils)
								Server.WriteItem(item.ItemID, pad.textBox.Text);
							break;
						default:
							break;
					}

				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Error trying to updated item \n" + ex.Message + "\n" + ex.StackTrace);
				Messenger.Default.Send(new ShowMessage("Error trying to updated item",ex));
			}

		
				
					
		}	

	}



	/* */
	struct ItemListStruct
	{
		public String ItemID { get; set; }
		
	}


	/* Status of connection for change the name of the button */
	public static class StatusConnection
	{
		public const String Connect= "Connect";
		public const String Disconnect = "Disconnect";		
	}

}
