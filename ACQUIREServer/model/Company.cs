using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACQUIREServer.model
{
	public enum CompanyState
	{
		UNESTABLISH,
		UNSAFE,
		SAFE,
		FINNISH
	}

	public enum CompanyType
	{
		WORLDWIDE,
		SACKSON,
		FESTIVAL,
		IMPERIAL,
		AMERICAN,
		CONTINENTAL,
		TOWER,
		NULL,
		SINGLE
	}

	class Company
	{
		static Dictionary<CompanyType, int> originalPrice = 
			new Dictionary<CompanyType, int> {
			{ CompanyType.WORLDWIDE, 200 },
			{ CompanyType.SACKSON, 200 },
			{ CompanyType.FESTIVAL, 300 },
			{ CompanyType.IMPERIAL, 300 },
			{ CompanyType.AMERICAN, 300 },
			{ CompanyType.CONTINENTAL, 400 },
			{ CompanyType.TOWER, 400 }};
		private CompanyType type;
		private string name;
		private int maxShare;
		private int remainShare;
		private int tileCount;

		public CompanyType Type
		{
			get
			{
				return type;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}

			set
			{
				name = value;
			}
		}

		public int MaxShare
		{
			get
			{
				return maxShare;
			}

			set
			{
				maxShare = value;
			}
		}

		public int RemainShare
		{
			get
			{
				return remainShare;
			}
		}

		public int TileCount
		{
			get
			{
				return tileCount;
			}
		}

		public CompanyState getState()
		{
			if(tileCount <= 1)
			{
				return CompanyState.UNESTABLISH;
			}
			else if(tileCount < 11)
			{
				return CompanyState.UNSAFE;
			}
			else if(tileCount < 41)
			{
				return CompanyState.SAFE;
			}
			else
			{
				return CompanyState.FINNISH;
			}
		}

		public void establish()
		{
			tileCount = 2;
		}

		public void addTile()
		{
			tileCount++;
		}

		public int getPrice(int tiles)
		{
			if (tiles < 2)
			{
				return 0;
			}
			else if (tiles <= 5)
			{
				return originalPrice[type] + (tileCount - 2) * 100;
			}
			else if (tiles <= 10)
			{
				return originalPrice[type] + 400;
			}
			else if (tiles <= 20)
			{
				return originalPrice[type] + 500;
			}
			else if (tiles <= 30)
			{
				return originalPrice[type] + 600;
			}
			else if (tiles <= 40)
			{
				return originalPrice[type] + 700;
			}
			else
			{
				return originalPrice[type] + 800;
			}
		}

		public void increaseShare(int number = 1)
		{
			remainShare += number;
		}


		public void decreaseShare(int number = 1)
		{
			remainShare -= number;
		}

		public void increaseTile(int number = 1)
		{
			tileCount += number;
		}

		public void decreaseTile(int number = 1)
		{
			tileCount -= number;
		}

		public void clearTile()
		{
			tileCount = 0;
		}

		public int getPrice()
		{
			return getPrice(tileCount);
		}

		public int getMajorityBonus()
		{
			return 10 * getPrice();
		}

		public int getMinorityBonus()
		{
			return 5 * getPrice();
		}

		public Company(CompanyType type)
		{
			this.type = type;
			Name = type.ToString();
			MaxShare = 25;
			remainShare = 25;
			tileCount = 0;
		}
	}
}
