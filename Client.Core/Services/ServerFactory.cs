using Client.Core.Interfaces;
using Client.Core.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core.Services
{
	public class ServerFactory
	{
		public IServer CreateServer(ClientType clientType)
		{
			switch (clientType)
			{				
				case ClientType.Modbus:
					return new ModBusServer();
				case ClientType.OPCDA:
						return new OPCDAServer();
				default:
					return null;

			}
			
		}
	}
}
