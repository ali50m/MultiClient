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
using System.Reflection;

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

			//When the file type change update the fileManager
			this.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName== nameof(FileType))
				{
					_fileManager = FileFactory.GetManager(fileType);
				}
			};
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
				Messenger.Default.Send(new Messaging.ShowMessage(ex.Message));
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


	
		#region OPCDA

		private ObservableCollection<string> _browseList;		

		public ObservableCollection<string> BrowseList
		{
			get { return _browseList; }
			set { _browseList = value; OnPropertyChange(); }
		}

		private List<string> _listServers;

		public List<string> ListServers
		{
			get { return _listServers = OPCDAServer.getAvaliableServers(); }
			set { _listServers = value; OnPropertyChange(); }
		}

		private string _serverselected;

		public string ServerSelected
		{
			get { return _serverselected; }
			set { _serverselected = value; OnPropertyChange(); }
		}

		private List<string> ListItemsInServer;


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
			try
			{
				if(!(parameter is string)) { return; }
				IItem oPCItem = Server.CreateItem(parameter.ToString());
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
				Messenger.Default.Send(new Messaging.ShowMessage(ex.Message));
			}

		}
		
		
		/* FIND ITEMS Browse ON TEXT CHANGE */
		private RelayCommand _TextChangeFilterItemsCommand;
		public ICommand TextChangeFilterItemsCommand
		{
			get
			{
				if (_TextChangeFilterItemsCommand == null)
				{
					_TextChangeFilterItemsCommand = new RelayCommand(FilterItems);
				}
				return _TextChangeFilterItemsCommand;
			}
		}
		private void FilterItems(object parameter)
		{

			//TODO
			TreeListItems =  SorterItems(ListItemsInServer
													.Where(
		  												x => x.Contains(parameter.ToString())
														
															)
													.ToList());
		}
		#endregion



		#region Common

		private String _IpAddress;
		public String IpAddress
		{
			get
			{
				return _IpAddress;
			}
			set
			{
				_IpAddress = value;
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
			if(SelectedClient == ClientType.OPCDA && ServerSelected == null) { Messenger.Default.Send(new Messaging.ShowMessage("You have to select a ServerName"));/* MessageBox.Show("You have to select a ServerName");*/ return; }
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
								if(SelectedClient == ClientType.Modbus)
									Server.Name = "Modbus";
								if (SelectedClient == ClientType.OPCDA)
									Server.Name = ServerSelected;
								Server.Connect();
								TextButtonConect = StatusConnection.Disconnect;
								isConnected = true;
								BrowseList = new ObservableCollection<string>(Server.BrowseItems());
								ListItemsInServer = Server.BrowseItems();
								TreeListItems = SorterItems(ListItemsInServer);
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
									BrowseList.Clear();
								});
								IpAddress = string.Empty;
								ModbusAddress = string.Empty;
								ModBusType = ItemType.None;
								TextButtonConect = StatusConnection.Connect;
								isConnected = false;
								Server = null;
							}
						}
						catch (Exception ex)
						{
							Server = null;
							_Logger.Error("Occurred an error trying to Connect/Disconnect to the server:\n "+ ex.Message + "\n"+ex.StackTrace);
							Messenger.Default.Send(new Messaging.ShowMessage(ex.Message));
						}
						
							IsBusy = false;
					});
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
						Server.RemoveItem(item.ItemID,item.ItemType);
						ListOfItemsActive.Remove(item);									
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
					Server.RemoveAll();
					ListOfItemsActive.Clear();
					
		}

		/* FIND ITEM ACTIVE ON TEXT CHANGE*/
		private RelayCommand _TextChangeItemsActive;
		public ICommand TextChangeItemsActive
		{
			get
			{
				if (_TextChangeItemsActive == null)
				{
					_TextChangeItemsActive = new RelayCommand(FindItemAct);
				}
				return _TextChangeItemsActive;
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
				if (pad.ShowDialog() == true)
				{

					switch (SelectedClient)
					{
						case ClientType.Modbus:
							if (item.ItemType == ItemType.HoldingRegister || item.ItemType == ItemType.Coils)
								Server.WriteItem(item.ItemID, pad.textBox.Text);
							break;
						case ClientType.OPCDA:
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



		/* button SAVE in file */
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
			if (FileType == FileType.None) { Messenger.Default.Send(new Messaging.ShowMessage("Select a file type")); return; }
			SaveFileDialog SaveFileDialog = new SaveFileDialog();
			switch (FileType)	
			{
				case FileType.XML:
					SaveFileDialog.Filter = "XML file (*.xml; *.xml) | *.xml;";
					break;
				case FileType.JSON:
					SaveFileDialog.Filter = "JSON file (*.json; *.json) | *.json;";
					break;
			}
			if (SaveFileDialog.ShowDialog() == true)
			{
				if (_fileManager != null)
				{
					if (_fileManager.SaveItems(Server.Items, SaveFileDialog.FileName))
						Messenger.Default.Send(new Messaging.ShowMessage("Items Saved"));

				}
			}
		}

		/* button LOAD from file  */
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
			if (FileType == FileType.None) { Messenger.Default.Send(new Messaging.ShowMessage("Select a file type")); return; }
			OpenFileDialog OpenFileDialog = new OpenFileDialog();
			switch (FileType)
			{
				case FileType.XML:
					OpenFileDialog.Filter = "XML file (*.xml; *.xml) | *.xml;";
					break;
				case FileType.JSON:
					OpenFileDialog.Filter = "JSON file (*.json; *.json) | *.json;";
					break;
			}
			if (OpenFileDialog.ShowDialog() == true)
			{
				try
				{
					if (_fileManager != null)
					{
						var items = _fileManager.ReadItems(OpenFileDialog.FileName);

					foreach (var item in items)
					{
						IItem itemCreated = Server.CreateItem(item.ItemID,item.ItemType);
						if (itemCreated != null)
							ListOfItemsActive.Add(new ItemViewModel(itemCreated));					
					}
					}
				}
				catch (Exception ex)
				{
					_Logger.Error("Occurred an exception trying to load the items \n" + ex.Message + "\n" + ex.StackTrace);
					Messenger.Default.Send(new Messaging.ShowMessage(ex.Message));
					
				}
			}
		}
		
		/* File Type change*/

		private FileType fileType;

		public FileType FileType
		{
			get { return fileType; }
			set { fileType = value; OnPropertyChange(); }
		}

		private RelayCommand _fileTypeCommand;

		public ICommand FileTypeCommand
		{
			get
			{
				if (_fileTypeCommand == null)
				{
					_fileTypeCommand = new RelayCommand(FileTypeChange);
				}
				return _fileTypeCommand;
			}

		}

		public void FileTypeChange(object parameter)
		{
			if (parameter == null) return;
			switch (parameter.ToString())
			{
				case nameof(FileType.XML):
					FileType = FileType.XML;
					break;
				case nameof(FileType.JSON):
					FileType = FileType.JSON;
					break;
				default:
					FileType = FileType.None;
					break;
			}
		}

		private Dictionary<string,List<string>> SorterItems(List<string> items)
		{
				var dic = new Dictionary<string, List<string>>();
			if(items!= null && items.Count() > 0)
			{
				foreach (var item in items)
				{
					string path = item;
					string parent="";
					do
					{
						var pos=path.IndexOf(".");
						parent = parent + path.Substring(0, pos+1);
						path =path.Substring(pos+1);
					
					} while (path.Contains("."));

					if (dic.ContainsKey(parent))
					{
						dic[parent].Add(item);
					}
					else
					{
						dic.Add(parent, new List<string>());
							dic[parent].Add(item);
					}
				}
			}
			return dic;
		}
		#endregion



	}
	/* Status of connection for change the name of the button */
	public static class StatusConnection
	{
		public const String Connect= "Connect";
		public const String Disconnect = "Disconnect";		
	}

}
