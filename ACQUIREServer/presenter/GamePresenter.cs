using ACQUIREServer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACQUIREServer.presenter
{
	class GamePresenter
	{
		private static GamePresenter _singleton = new GamePresenter();
		private ServerPresenter sPresenter = ServerPresenter.getInstance();
		private MainPlayerPresenter mainPlayerPresenter = MainPlayerPresenter.getInstance();
		private Game game = Game.getInstance();
		private Vector nowTile;
		
		static public GamePresenter getInstance()
		{
			return _singleton;
		}

		private GamePresenter()
		{
		}

		public void Init(int playerCount = 8)
		{
			Game.getInstance().reset(playerCount);
		}

		public string getInfomation(string Uid)
		{
			return getInfomation(game.UidToVector(Uid));
		}

		public string getInfomation(Vector tilePosition)
		{
			Tile tile = game.GameBoard.getTile(tilePosition);
			if(tile.Company == CompanyType.NULL || tile.Company == CompanyType.SINGLE)
			{
				return "";
			}
			else
			{
				return tile.Company.ToString() + "\n" + game.Companys[tile.Company].TileCount.ToString() + "\n" + game.Companys[tile.Company].getPrice().ToString();
			}
	
		}

		public bool tryPutTile(string Uid)
		{
			return tryPutTile(game.UidToVector(Uid));
		}

		public bool tryPutTile(Vector tilePosition)
		{
			return game.GameBoard.tryPutTile(tilePosition);
		}

		public bool putTile(string Uid)
		{
			return putTile(game.UidToVector(Uid));
		}

		public bool putTile(Vector tile)
		{
			nowTile = tile;
			switch(game.GameBoard.putTile(tile))
			{
				case tileResult.INDEPENDENT:
					break;
				case tileResult.CREATE:
					handleCreate(sPresenter.receiveSelect(game.getUnEstablishCompany()));
					//mainWindow.setCompanyClickable(game.getUnEstablishCompany());
					break;
				case tileResult.ATTACH:
					game.GameBoard.attch(tile);
					break;
				case tileResult.MERGER:
				case tileResult.TRIPLE:
				case tileResult.FOUR:
					switch (game.GameBoard.merger(tile))
					{
						case mergerResult.CANNOTMERGER:
							return false;
						case mergerResult.SELECT:
							//mainWindow.setCompanyClickable(game.GameBoard.getBiggestCompany(tile));
							game.GameBoard.changeCompany(nowTile, sPresenter.receiveSelect(game.GameBoard.getBiggestCompany(tile)));
							game.GameBoard.merger(nowTile);
							//handleShare
							break;
						case mergerResult.SUCCESS:
							break;
					}
					CompanyType biggestCompany = game.GameBoard.getBiggestCompany(tile).FirstOrDefault();
					Handle(biggestCompany, game.Companys[biggestCompany].RemainShare, game.GameBoard.SmallCompanyPrices);
					break;
				case tileResult.ERROR:
					return false;
				default:
					break;
			}
			game.getNowPlayer().Tiles.Remove(tile);
			return true;
		}

		private void Handle(CompanyType biggestCompany, int remainShare, Dictionary<CompanyType, int> smallCompanyPrices)
		{
			int id = 0;
			foreach(var p in game.Players)
			{
				var result = sPresenter.receiveHandleShare(biggestCompany, remainShare, smallCompanyPrices, mainPlayerPresenter.getAvailable(smallCompanyPrices, id), id);
				handleExchange(biggestCompany, result.exchange, id);
				handleSale(smallCompanyPrices, result.sale, id);
				id++;
			}
			//TODO
		}



		public void handleExchange(CompanyType biggest, Dictionary<CompanyType, int> result, int playerId)
		{
			var player = game.Players[playerId];
			int destinationShareCount = 0;
			foreach (var r in result)
			{
				destinationShareCount += r.Value;              //加验证
			}
			if (Game.getInstance().exchangeShare(biggest, destinationShareCount, result))
			{
				foreach (var r in result)
				{
					player.Share[r.Key] -= r.Value;
				}
				player.Share[biggest] += destinationShareCount;
			}

		}

		public void handleSale(Dictionary<CompanyType, int> companyPrices, Dictionary<CompanyType, int> result, int playerId)
		{
			foreach (var r in result)
			{
				if (r.Value > 0)
				{
					saleShare(r.Key, r.Value, companyPrices[r.Key], playerId);
				}
			}
		}



		public bool saleShare(CompanyType com, int count, int price, int playerId)
		{
			if (game.Players[playerId].saleShare(com, count, price))
			{
				Game.getInstance().saleShare(com, count);
				return true;
			}
			return false;
		}

		public void Start()
		{
			sPresenter.sendGameStart();
			for(int i = 0; i < game.PlayerCount; i++)
			{
				sPresenter.updateTiles(mainPlayerPresenter.initTiles(i), i);
			}
			GameOn();
			buyShare();
		}

		private void GameOn()
		{
			while(!game.isOver())
			{
				RoundStart();
				RoundOn();
				RoundEnd();
			}
			Over();
		}

		private void RoundStart()
		{
			sPresenter.sendPlayerStart(game.getNowPlayerIndex());
		}

		private void RoundOn()
		{
			var tile = sPresenter.receiveTile();
			putTile(tile);
			sPresenter.updateTiles(mainPlayerPresenter.getNewTile());
			buyShare();
			updateGame();
		}

		private void RoundEnd()
		{
			sPresenter.sendPlayerOver();
			if (!game.nextPlayer())
			{
				game.mixIndex();
			}
		}

		private void updateGame()
		{
			Vector position = new Vector();
			int border;
			var gameBoard = Game.getInstance().GameBoard;
			HashSet<TileData> tiles = new HashSet<TileData>();
			HashSet<PlayerData> players = new HashSet<PlayerData>();
			for (int i = 0; i < 12; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					position.X = i;
					position.Y = j;
					border = gameBoard.getTileNeighbour(position);
					if (border != -1)
					{
						tiles.Add(new TileData(position.X.ToString() + position.Y.ToString(), border, gameBoard.getCompany(position)));
					}
				}
			}
			int index = 0;
			foreach(var p in Game.getInstance().Players)
			{
				players.Add(new PlayerData(index, p.Money, p.Share));
				index++;
			}

			sPresenter.updateGameInformations(tiles, players);
		}

		private void handleCreate(CompanyType com)
		{
			game.GameBoard.changeCompany(nowTile, com);
			mainPlayerPresenter.giveShare(com);
		}

		private void buyShare()
		{
			Dictionary<CompanyType, int> companysPrice = new Dictionary<CompanyType, int>();
			Dictionary<CompanyType, int> companysAvailable = new Dictionary<CompanyType, int>();
			foreach (var c in game.getBuyableCompany())
			{
				companysPrice[c] = game.Companys[c].getPrice();
			}
			foreach(var c in companysPrice)
			{
				companysAvailable[c.Key] = game.Companys[c.Key].RemainShare;
			}
			var result = sPresenter.receiveBuyShare(companysPrice, companysAvailable, game.getNowPlayer().Money);
			foreach(var c in result)
			{
				mainPlayerPresenter.buyShare(c.Key, c.Value);
			}
			//mainWindow.putTileOver(game.getBuyableCompany());
		}

		public string getCompanysInfomation()
		{
			string info = "";
			foreach(var c in game.Companys)
			{
				info += c.Value.Type.ToString();
				info += ": ";
				info += c.Value.TileCount;
				info += "\n";
			}
			return info;
		}

		public void Over()
		{
			sPresenter.sendGameOver();
			Program.Close();
		}
	}
}
