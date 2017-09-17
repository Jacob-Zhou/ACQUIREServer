using ACQUIREServer.presenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACQUIREServer.model
{



	class Game
	{
		static private Game _singleton = new Game();
		private Board gameBoard;
		private Dictionary<CompanyType, Company> companys;
		private Queue<string> tilesQueue = new Queue<string>();
		private Vector[,] tiles = new Vector[12, 9];
		private int playerCount;
		private int NowIndex;
		private Player[] players;
		private int[] roundIndex;

		static public Game getInstance()
		{
			
			return _singleton;
		}
		

		public int PlayerCount
		{
			get
			{
				return playerCount;
			}

			set
			{
				playerCount = value;
			}
		}

		public Player getNowPlayer()
		{
			return players[roundIndex[NowIndex]];
		}

		internal Dictionary<CompanyType, Company> Companys
		{
			get
			{
				return companys;
			}
		}

		public bool exchangeShare(CompanyType destination, int destinationShareCount, Dictionary<CompanyType, int> source)
		{
			if (companys[destination].RemainShare >= destinationShareCount)
			{
				companys[destination].decreaseShare(destinationShareCount);
				foreach(var s in source)
				{
					companys[s.Key].increaseShare(s.Value);
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool giveShare(CompanyType com)
		{
			if(companys[com].RemainShare >= 1)
			{
				companys[com].decreaseShare();
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool buyShare(CompanyType com, int count, int playerRemainMoney)
		{
			if (count > 3)
			{
				return false;
			}
			else
			{
				if(companys[com].RemainShare >= count && playerRemainMoney >= (companys[com].getPrice() * count))
				{
					companys[com].decreaseShare(count);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public void saleShare(CompanyType com, int count)
		{
			companys[com].increaseShare(count);
		}

		public bool isOver()
		{
			int count = 0;
			foreach (var c in Companys)
			{
				if (c.Value.getState() == CompanyState.SAFE)
				{
					count++;
				}
				else if(c.Value.getState() == CompanyState.FINNISH)
				{
					return true;
				}
			}
			if(count >= 7)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		internal Board GameBoard
		{
			get
			{
				return gameBoard;
			}

			set
			{
				gameBoard = value;
			}
		}

		internal Player[] Players
		{
			get
			{
				return players;
			}
		}

		public Vector[,] Tiles
		{
			get
			{
				return tiles;
			}
		}

		public Game(int pCount = 8)
		{
			reset(pCount);
		}

		public HashSet<CompanyType> getUnEstablishCompany()
		{
			HashSet<CompanyType> com = new HashSet<CompanyType>();
			foreach(var c in Companys)
			{
				if(c.Value.getState() == CompanyState.UNESTABLISH)
				{
					com.Add(c.Value.Type);
				}
			}
			return com;
		}

		public int getCompanyCount()
		{
			int count = 0;
			foreach (var c in Companys)
			{
				if (c.Value.getState() != CompanyState.UNESTABLISH)
				{
					count++;
				}
			}
			return count;
		}

		public Vector UidToVector(string Uid)
		{
			int id = int.Parse(Uid);
			return Tiles[id / 10, id % 10];
		}

		public Vector getNewTile()
		{
			return UidToVector(tilesQueue.Dequeue());
		}

		public HashSet<CompanyType> getBuyableCompany()
		{
			HashSet<CompanyType> com = new HashSet<CompanyType>();
			foreach (var c in Companys)
			{
				if (c.Value.getState() > CompanyState.UNESTABLISH && c.Value.RemainShare > 0)
				{
					com.Add(c.Value.Type);
				}
			}
			return com;
		}
		
		public int getNowPlayerIndex()
		{
			return roundIndex[NowIndex];
		}

		public bool nextPlayer()
		{
			NowIndex++;
			if(NowIndex == playerCount)
			{
				NowIndex = 0;
				mixIndex();
				return false;
			}
			else
			{
				return true;
			}
		}

		public void mixIndex()
		{
			Random ram = new Random();
			int k = 0;
			int value = 0;
			int max = roundIndex.Length;
			for (int i = 0; i < max; i++)
			{
				k = ram.Next(0, max);
				if (k != i)
				{
					value = roundIndex[i];
					roundIndex[i] = roundIndex[k];
					roundIndex[k] = value;
				}
			}
		}

		public void reset(int pCount = 8)
		{
			NowIndex = 0;
			gameBoard = new Board();
			companys = new Dictionary<CompanyType, Company>();
			playerCount = pCount;

			for (int i = 0; i < 7; i++)
			{
				Companys[(CompanyType)i] = new Company((CompanyType)i);
			}

			roundIndex = new int[playerCount];
			for (int i = 0; i < playerCount; i++)
			{
				roundIndex[i] = i;
			}

			players = new Player[pCount];
			for (int i = 0; i < pCount; i++)
			{
				players[i] = new Player(true);
			}

			Vector temp;
			Vector[] tempTiles = new Vector[108];
			int index = 0;
			for (int i = 0; i < 12; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					temp = new Vector(i, j);
					tiles[i, j] = temp;
					tempTiles[index] = temp;
					index++;
				}
			}

			Random ran = new Random();
			int k;
			Vector tempVector;
			for (int i = 0; i < 108; i++)
			{
				k = ran.Next(0, 108);
				tempVector = tempTiles[i];
				tempTiles[i] = tempTiles[k];
				tempTiles[k] = tempVector;
			}

			foreach (var v in tempTiles)
			{
				tilesQueue.Enqueue(v.ToString());
			}

		}
	}
}
