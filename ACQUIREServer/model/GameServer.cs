using ACQUIREServer.presenter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACQUIREServer.model
{
	public enum DataType : byte
	{
		GAMESTART = 0,
		GAMEOVER = 1,
		START = 2,
		OVER = 3,
		PUTTILE = 4,
		SELECT = 5,
		SELECTRESULT = 6,
		HANDLE = 7,
		HANDLERESULT = 8,
		BUY = 9,
		BUYRESULT = 10,
		UPDATE = 11,
		UPDATETILES = 12,
		EMPTY = 19,
		INIT = 20,
		ERROR = 40
	}

	class GameServer
	{
		private static GameServer _singleton = new GameServer();
		private IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 56000);
		private TcpListener listener;
		private Dictionary<int, Socket> clients = new Dictionary<int, Socket>();
		private Dictionary<int, Thread> receiveThreads = new Dictionary<int, Thread>();

		private ServerPresenter serverPresenter;

		private bool[] states = new bool[32];
		private string[] messages = new string[32];
		private static string empty = "{}";

		private int playerCount = 0;
		private int index = 0;
		private bool isReady = false;
		private int activeIndex = 0; 

		public static GameServer getInstance()
		{
			return _singleton;
		}

		public bool isReceiveAll(byte dataType)
		{
			switch(dataType)
			{
				case 8:
					return true;
				default:
					return false;
			}
		}

		public void handleError()
		{
			isReady = false;
			serverPresenter.HandleError();
			foreach(var t in receiveThreads)
			{
				t.Value.Abort();
			}
		}

		public void Init()
		{
			serverPresenter = ServerPresenter.getInstance();
			listener = new TcpListener(ipEndPoint);
			listener.Start();
			new Thread(async delegate ()
			{
				while (index < playerCount)
				{
					clients[index] = await listener.AcceptSocketAsync();
					Connected(index);
					index++;
				}
			}).Start();
		}

		public void clearState()
		{
			for(int i = 0; i < states.Length; i++)
			{
				states[i] = false;
			}
		}

		private void Connected(int index)
		{
			receiveThreads[index] = new Thread(delegate ()
			{
				int cIndex = index;
				byte[] typeBytes = new Byte[2];
				byte[] lengthBytes = new Byte[8];
				byte[] bytes = new Byte[4096];
				string tempStr;
				int tempInt;

				Send(cIndex.ToString(), cIndex, 20);

				while(!isReady)
				{
				}
				Console.WriteLine("Ready");
				while (isReady)
				{
					try
					{
						clients[cIndex].Receive(typeBytes, 2, SocketFlags.Partial);
						clients[cIndex].Receive(lengthBytes, 8, SocketFlags.Partial);
						tempStr = Encoding.Unicode.GetString(lengthBytes, 0, 8);
						tempInt = int.Parse(tempStr);
						tempInt *= 2;
						clients[cIndex].Receive(bytes, tempInt, SocketFlags.Partial);
						if(typeBytes[0] == (byte)DataType.ERROR)
						{
							throw new Exception();
						}
						Console.Write("from :"+ typeBytes[1].ToString() + "->" + typeBytes[0].ToString() + " ");
							if (typeBytes[1] == (byte)activeIndex || isReceiveAll(typeBytes[0]))
							{
								lock (messages)
								{
									messages[typeBytes[0]] = Encoding.Unicode.GetString(bytes, 0, tempInt);
									Console.WriteLine(messages[typeBytes[0]]);
								}
								lock (states)
								{
									states[typeBytes[0]] = true;
								}
							}
							Console.WriteLine();
					}
					catch (Exception)
					{
						if(isReady)
						{
							isReady = false;
							handleError();
						}
						return;
					};
				}
			});


			receiveThreads[index].Start();

			if (index == playerCount - 1)
			{
				isReady = true;
				Thread.Sleep(10);
				serverPresenter.ClientsReady();
			}
		}

		public void Send(string text, int index, byte textType)
		{
			var message = Encoding.Unicode.GetBytes("0" + string.Format("{0:0000}", text.Length) + text);
			message[0] = textType;
			message[1] = (byte)index;
			try
			{
				clients[index].Send(message);
			}
			catch (Exception)
			{
				isReady = false;
				serverPresenter.HandleError();
			}
		}

		public bool getState(int textType)
		{
			return states[textType];
		}

		public string getMessage(int textType)
		{
			if (states[textType])
			{
				lock(states)
				{
					states[textType] = false;
				}
				return messages[textType];
			}
			else
			{
				return null;
			}
		}

		public async Task<string> waitMessageAsync(int textType)
		{
			return await Task.Run(() => {
				while (!states[textType])
				{
					if(!isReady)
					{
						return string.Empty;
					}
					Thread.Sleep(10);
				}
				lock(states)
				{
					states[textType] = false;
				}
				return messages[textType];
			});
		}

		public void Send(string text, byte textType)
		{
			Send(text, activeIndex, textType);

		}

		public void Send(string text)
		{
			Send(text, activeIndex, 19);
		}

		public void broadcast(string text, byte textType)
		{
			var message = Encoding.Unicode.GetBytes("0" + string.Format("{0:0000}", text.Length) + text);
			message[0] = textType;
			foreach (var s in clients)
			{
				message[1] = (byte)s.Key;
				try
				{
					s.Value.Send(message);
				}
				catch (Exception)
				{
					isReady = false;
					serverPresenter.HandleError();
				}
			}
		}
		
		public int Port
		{
			get
			{
				return ipEndPoint.Port;
			}

			set
			{
				ipEndPoint.Port = value;
			}
		}

		public int PlayerCount
		{
			get
			{
				return playerCount;
			}

			set
			{
				if(value > 8)
				{
					playerCount = 8;
				}
				else
				{
					playerCount = value;
				}
			}
		}

		public int ActiveIndex
		{
			get
			{
				return activeIndex;
			}

			set
			{
				activeIndex = value;
			}
		}

		public string Empty
		{
			get
			{
				return empty;
			}
		}
	}
}
