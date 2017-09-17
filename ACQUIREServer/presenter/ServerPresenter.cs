using ACQUIREServer.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading;

namespace ACQUIREServer.presenter
{
	public class TileData
	{
		public TileData(string uid, int border, CompanyType company)
		{
			Uid = uid;
			this.border = border;
			this.company = company;
		}

		public TileData(){}

		public string Uid { get; set; }
		public int border { get; set; }
		public CompanyType company { get; set; }
	}

	public class HandleResult
	{
		public Dictionary<CompanyType, int> exchange { get; set; }
		public Dictionary<CompanyType, int> sale { get; set; }
	}

	public class PlayerData
	{
		public PlayerData(int index, int money, Dictionary<CompanyType, int> share)
		{
			this.index = index;
			this.money = money;
			this.share = share;
		}

		public PlayerData(){}

		public int index { get; set; }
		public int money { get; set; }
		public Dictionary<CompanyType, int> share { get; set; }
	}

	public class BuyData
	{
		public Dictionary<CompanyType, int> companyPrice { get; set; }
		public Dictionary<CompanyType, int> companyAvailable { get; set; }
		public int playerMoney { get; set; }
	}

	public class HandleData
	{
		public CompanyType bigCompany { get; set; }
		public int bigCompanyRemain { get; set; }
		public Dictionary<CompanyType, int> smallcomPrice { get; set; }
		public Dictionary<CompanyType, int> smallcomAvailable { get; set; }
	}

	public class UpdateData
	{
		public HashSet<TileData> tiles { get; set; }
		public HashSet<PlayerData> players { get; set; }
	}
	
	public class ServerPresenter
	{
		private static ServerPresenter _singleton = new ServerPresenter();
		private GamePresenter gamePresenter;
		private MainPlayerPresenter mainPlayerPresenter = MainPlayerPresenter.getInstance();
		private GameServer server = GameServer.getInstance();

		private ServerPresenter() { }

		public static ServerPresenter getInstance()
		{
			return _singleton;
		}

		public void ClientsReady()
		{
			gamePresenter.Start();
		}

		public void HandleError()
		{
			Program.Close();
		}

		public void Init(int port, int playerCount)
		{
			gamePresenter = GamePresenter.getInstance();
			server.Port = port;
			server.PlayerCount = playerCount;
			server.Init();
		}

		public void sendGameStart()
		{
			server.broadcast(server.Empty, (byte)DataType.GAMESTART);
		}

		public void sendGameOver()
		{
			server.broadcast(server.Empty, (byte)DataType.GAMEOVER);
		}
		public void sendPlayerStart(int index)
		{
			server.ActiveIndex = index;
			server.clearState();
			server.Send(server.Empty, (byte)DataType.START);
		}
		public void sendPlayerOver()
		{
			server.broadcast(server.Empty, (byte)DataType.OVER);
		}

		public string receiveTile()
		{
			var tileStr = server.waitMessageAsync((byte)DataType.PUTTILE);
			var tile = JsonConvert.DeserializeObject<TileData>(tileStr.Result);
			return tile.Uid;
		}

		public CompanyType receiveSelect(HashSet<CompanyType> companys)
		{
			var data = JsonConvert.SerializeObject(companys);
			server.clearState();
			server.Send(data, (byte)DataType.SELECT);
			var resultStr = server.waitMessageAsync((byte)DataType.SELECTRESULT);
			return JsonConvert.DeserializeObject<CompanyType>(resultStr.Result);
		}

		public HandleResult receiveHandleShare(CompanyType bigcom, int bigcomRemain, Dictionary<CompanyType, int> smallcomPrice, Dictionary<CompanyType, int> smallcomAvailable, int playerId)
		{
			HandleData rawData = new HandleData();
			rawData.bigCompany = bigcom;
			rawData.bigCompanyRemain = bigcomRemain;
			rawData.smallcomPrice = smallcomPrice;
			rawData.smallcomAvailable = smallcomAvailable;
			var data = JsonConvert.SerializeObject(rawData);
			server.clearState();
			server.Send(data, playerId, (byte)DataType.HANDLE);
			var resultStr = server.waitMessageAsync((byte)DataType.HANDLERESULT);
			return JsonConvert.DeserializeObject<HandleResult>(resultStr.Result);
		}

		public Dictionary<CompanyType, int> receiveBuyShare(Dictionary<CompanyType, int> companyPrice, Dictionary<CompanyType, int> companyAvailable, int playerMoney)
		{
			BuyData rawData = new BuyData();
			rawData.companyPrice = companyPrice;
			rawData.companyAvailable = companyAvailable;
			rawData.playerMoney = playerMoney;
			var data = JsonConvert.SerializeObject(rawData);
			server.clearState();
			server.Send(data, (byte)DataType.BUY);
			var resultStr = server.waitMessageAsync((byte)DataType.BUYRESULT);
			return JsonConvert.DeserializeObject<Dictionary<CompanyType, int>>(resultStr.Result);
		}

		public void updateTiles(Dictionary<string, bool> tileUids, int? playerId = null)
		{
			var data = JsonConvert.SerializeObject(tileUids);
			if(playerId.HasValue)
			{
				server.Send(data, playerId.Value, (byte)DataType.UPDATETILES);
			}
			else
			{
				server.Send(data, (byte)DataType.UPDATETILES);
			}
		}

		public void updateGameInformations(HashSet<TileData> tiles, HashSet<PlayerData> players)
		{
			UpdateData rawData = new UpdateData();
			rawData.tiles = tiles;
			rawData.players = players;
			var data = JsonConvert.SerializeObject(rawData);
			server.broadcast(data, (byte)DataType.UPDATE);
		}
	}
}
