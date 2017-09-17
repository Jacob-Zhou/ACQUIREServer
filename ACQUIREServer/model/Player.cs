using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACQUIREServer.model
{
	class Player
	{
		private int money;
		private Dictionary<CompanyType, int> share;
		private HashSet<Vector> tiles;
		private bool inGame;

		public int Money
		{
			get
			{
				return money;
			}

			set
			{
				money = value;
			}
		}

		public Dictionary<CompanyType, int> Share
		{
			get
			{
				return share;
			}

			set
			{
				share = value;
			}
		}

		public HashSet<Vector> Tiles
		{
			get
			{
				return tiles;
			}

			set
			{
				tiles = value;
			}
		}

		public bool InGame
		{
			get
			{
				return inGame;
			}

			set
			{
				inGame = value;
			}
		}

		public void giveShare(CompanyType com)
		{
			share[com]++;
		}

		public void buyShare(CompanyType com, int count, int price)
		{
			money -= price * count;
			share[com] += count;
		}

		public bool saleShare(CompanyType com, int count, int price)
		{
			if(share[com] >= count)
			{
				share[com] -= count;
				money += price * count;
				return true;
			}
			return false;
		}

		public int getProperty()
		{
			int shareValue = 0;
			foreach(var c in share)
			{
				shareValue += Game.getInstance().Companys[c.Key].getPrice() * c.Value;
			}
			return money + shareValue;
		}

		public Player(bool inGame)
		{
			this.Money = 6000;
			this.Share = new Dictionary<CompanyType, int> {
			{CompanyType.AMERICAN, 0 },
			{CompanyType.CONTINENTAL, 0 },
			{CompanyType.FESTIVAL, 0 },
			{CompanyType.IMPERIAL, 0 },
			{CompanyType.SACKSON, 0 },
			{CompanyType.TOWER, 0 },
			{CompanyType.WORLDWIDE, 0 }};
			this.Tiles = new HashSet<Vector>();
			this.inGame = inGame;
		}
	}
}
