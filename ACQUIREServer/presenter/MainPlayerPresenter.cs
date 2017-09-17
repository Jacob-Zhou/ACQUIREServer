using ACQUIREServer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACQUIREServer.presenter
{
	class MainPlayerPresenter
	{
		private static MainPlayerPresenter _singleton = new MainPlayerPresenter();
		private Game game = Game.getInstance();

		public static MainPlayerPresenter getInstance()
		{
			return _singleton;
		}

		public int getPlayerMoney()
		{
			return game.getNowPlayer().Money;
		}

		public string getShareInfomations()
		{
			string str = "";
			foreach(var s in game.getNowPlayer().Share)
			{
				str += s.Key.ToString() + ": " + s.Value.ToString() + "\n	Value: " + (s.Value * Game.getInstance().Companys[s.Key].getPrice()) + "\n";
			}
			return str;
		}

		public int getProperty()
		{
			return game.getNowPlayer().getProperty();
		}

		public void giveShare(CompanyType com)
		{
			if(Game.getInstance().giveShare(com))
			{
				game.getNowPlayer().giveShare(com);
			}
		}

		public bool buyShare(CompanyType com, int count)
		{
			if(Game.getInstance().buyShare(com, count, game.getNowPlayer().Money))
			{
				game.getNowPlayer().buyShare(com, count, Game.getInstance().Companys[com].getPrice());
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool saleShare(CompanyType com, int count, int playerId)
		{
			return GamePresenter.getInstance().saleShare(com, count, Game.getInstance().Companys[com].getPrice(), game.getNowPlayerIndex());
		}

		public Dictionary<CompanyType, int> getAvailable(Dictionary<CompanyType, int> companyPrices)
		{
			return getAvailable(companyPrices, game.getNowPlayerIndex());
		}

		public Dictionary<CompanyType, int> getAvailable(Dictionary<CompanyType, int> companyPrices, int playerId)
		{
			var player = game.Players[playerId];
			Dictionary<CompanyType, int> available = new Dictionary<CompanyType, int>();
			foreach (var r in companyPrices)
			{
				available[r.Key] = player.Share[r.Key];
			}
			return available;
		}

		public Dictionary<string, bool> initTiles(int playerId)
		{
			var player = game.Players[playerId];
			Dictionary<string, bool> Uids = new Dictionary<string, bool>();
			for (int i = 0; i < 6; i++)
			{
				player.Tiles.Add(game.getNewTile());
			}
			foreach (var t in player.Tiles)
			{
				Uids[t.X.ToString() + t.Y.ToString()] = game.GameBoard.tryPutTile(t);
			}
			return Uids;
		}

		public Dictionary<string, bool> getNewTile(int playerId)
		{
			var player = game.Players[playerId];
			player.Tiles.Add(game.getNewTile());
			Dictionary<string, bool> Uids = new Dictionary<string, bool>();
			foreach (var t in player.Tiles)
			{
				Uids[t.X.ToString() + t.Y.ToString()] = game.GameBoard.tryPutTile(t);
			}
			return Uids;
		}

		public Dictionary<string, bool> getNewTile()
		{
			return getNewTile(game.getNowPlayerIndex());
		}
	}
}
