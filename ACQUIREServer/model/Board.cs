using ACQUIREServer.presenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACQUIREServer.model
{
	public enum alphabate
	{
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I
	}

	public enum tileState
	{
		UNSET,
		SINGLE,
		SET
	}

	public enum tileResult
	{
		INDEPENDENT,
		CREATE,
		ATTACH,
		MERGER,
		TRIPLE,
		FOUR,
		ERROR
	}

	public enum mergerResult
	{
		CANNOTMERGER,
		SUCCESS,
		SELECT
	}

	class Board
	{
		private Tile[,] tiles = new Tile[12,9];
		private Dictionary<CompanyType, int> smallCompanyPrices = new Dictionary<CompanyType, int>();

		public Board()
		{
			for(int i = 0; i < 12; i++)
			{
				for(int j = 0; j < 9; j++)
				{
					tiles[i, j] = new Tile(i, j);
				}
			}
		}

		public Tile[,] Tiles
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

		public Dictionary<CompanyType, int> SmallCompanyPrices
		{
			get
			{
				return smallCompanyPrices;
			}
		}

		public HashSet<CompanyType> getBiggestCompany(Vector tilePosition)
		{
			int max = 0;
			HashSet<CompanyType> BiggestCompany = new HashSet<CompanyType>();
			int temp;
			foreach (var c in getTileNeighbourCompany(tilePosition))
			{
				temp = Game.getInstance().Companys[c].TileCount;
				if (temp >= max)
				{
					max = temp;
					BiggestCompany.Add(c);
				}
			}
			BiggestCompany.Remove(CompanyType.NULL);
			return BiggestCompany;
		}

		public mergerResult merger(Vector tilePosition)
		{
			int max = 0;
			HashSet<CompanyType> BiggestCompany = new HashSet<CompanyType>();
			HashSet<CompanyType> SmallerCompany = new HashSet<CompanyType>();
			int temp;
			foreach(var c in getTileNeighbourCompany(tilePosition))
			{
				temp = Game.getInstance().Companys[c].TileCount;
				if (temp > max)
				{
					max = temp;
					foreach(var bc in BiggestCompany)
					{
						SmallerCompany.Add(bc);
					}
					BiggestCompany.Clear();
					BiggestCompany.Add(c);
				}
				else if(temp == max)
				{
					BiggestCompany.Add(c);
				}
				else
				{
					SmallerCompany.Add(c);
				}
			}
			BiggestCompany.Remove(CompanyType.NULL);
			SmallerCompany.Remove(CompanyType.NULL);

			if (BiggestCompany.Count == 1)
			{
				CompanyType BigCompany = BiggestCompany.First();
				smallCompanyPrices.Clear();
				foreach (var c in SmallerCompany)
				{
					smallCompanyPrices[c] = Game.getInstance().Companys[c].getPrice();
					mergerCompany(BigCompany, c);
				}
				changeCompany(tilePosition, BigCompany);
				return mergerResult.SUCCESS;
			}
			else if(BiggestCompany.Count == 0)
			{
				return mergerResult.CANNOTMERGER;
			}
			else
			{
				return mergerResult.SELECT;
			}
		}

		public Tile getTile(Vector tilePosition)
		{
			return tiles[(int)tilePosition.X, (int)tilePosition.Y];
		}

		public void attch(Vector tilePosition)
		{
			var company = getTileNeighbourCompany(tilePosition);
			if(company.Count == 1)
			{
				changeCompany(tilePosition, company.First());
			}
		}

		public HashSet<CompanyType> getTileNeighbourCompany(Vector tilePosition)
		{
			int x = (int)tilePosition.X;
			int y = (int)tilePosition.Y;
			HashSet<CompanyType> nCompany = new HashSet<CompanyType>();

			Tile tile;
			tileState state;
			if (x - 1 >= 0)
			{
				tile = tiles[x - 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					nCompany.Add(tile.Company);
				}
			}

			if (x + 1 < 12)
			{
				tile = tiles[x + 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					nCompany.Add(tile.Company);
				}
			}

			if (y - 1 >= 0)
			{
				tile = tiles[x, y - 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					nCompany.Add(tile.Company);
				}
			}

			if (y + 1 < 9)
			{
				tile = tiles[x, y + 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					nCompany.Add(tile.Company);
				}
			}

			nCompany.Remove(CompanyType.NULL);
			nCompany.Remove(CompanyType.SINGLE);
			return nCompany;
		}

		public int getTileNeighbour(Vector tilePosition)
		{
			int neighbour = 0;
			int x = tilePosition.X;
			int y = tilePosition.Y;
			CompanyType com = getTile(tilePosition).Company;
			tileState state = getTile(tilePosition).State;
			if (state == tileState.UNSET)
			{
				return -1;
			}

			Tile tile;
			if (x - 1 >= 0)
			{
				tile = tiles[x - 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					if (tile.Company == com)
					{
						neighbour |= 8;
					}
				}
			}

			if (x + 1 < 12)
			{
				tile = tiles[x + 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					if (tile.Company == com)
					{
						neighbour |= 2;
					}
				}
			}

			if (y - 1 >= 0)
			{
				tile = tiles[x, y - 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					if (tile.Company == com)
					{
						neighbour |= 1;
					}
				}
			}

			if (y + 1 < 9)
			{
				tile = tiles[x, y + 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					if (tile.Company == com)
					{
						neighbour |= 4;
					}
				}
			}

			return neighbour;
		}

		public void setCompany(Vector tilePosition, CompanyType com)
		{
			Tile t = getTile(tilePosition);
			t.State = tileState.SET;
			t.Company = com;
			Game.getInstance().Companys[com].increaseTile();
		}

		public void changeCompany(Vector tilePosition, CompanyType com)
		{
			Tile t = getTile(tilePosition);
			CompanyType preCompany = t.Company;
			
			if(preCompany == com)
			{
				return;
			}
			t.State = tileState.SET;
			t.Company = com;
			Game.getInstance().Companys[com].increaseTile();
			if (preCompany != CompanyType.NULL && preCompany != CompanyType.SINGLE)
			{
				Game.getInstance().Companys[preCompany].decreaseTile();
			}

			int x = (int)tilePosition.X;
			int y = (int)tilePosition.Y;
			if (x - 1 >= 0)
			{
				t = tiles[x - 1, y];
				if (t.State != tileState.UNSET)
				{
					if ((t.Company == preCompany && preCompany != CompanyType.NULL) || t.State == tileState.SINGLE)
					{
						changeCompany(new Vector(x - 1, y), com);
					}
				}
			}

			if (x + 1 < 12)
			{
				t = tiles[x + 1, y];
				if (t.State != tileState.UNSET)
				{
					if ((t.Company == preCompany && preCompany != CompanyType.NULL) || t.State == tileState.SINGLE)
					{
						changeCompany(new Vector(x + 1, y), com);
					}
				}
			}


			if (y + 1 < 9)
			{
				t = tiles[x, y + 1];
				if (t.State != tileState.UNSET)
				{
					if ((t.Company == preCompany && preCompany != CompanyType.NULL) || t.State == tileState.SINGLE)
					{
						changeCompany(new Vector(x, y + 1), com);
					}
				}
			}

			if (y - 1 >= 0)
			{
				t = tiles[x, y - 1];
				if (t.State != tileState.UNSET)
				{
					if ((t.Company == preCompany && preCompany != CompanyType.NULL) || t.State == tileState.SINGLE)
					{
						changeCompany(new Vector(x, y - 1), com);
					}
				}
			}
		}

		public CompanyType getCompany(Vector tilePosition)
		{
			return getTile(tilePosition).Company;
		}

		public void mergerCompany(CompanyType big, CompanyType small)
		{
			if (Game.getInstance().Companys[small].getState() == CompanyState.UNSAFE)
			{
				foreach (var t in tiles)
				{
					if (t.Company == small)
					{
						changeCompany(t.Position, big);
					}
				}
			}
		}

		public bool tryPutTile(Vector tilePosition)
		{
			Tile tile;
			tile = getTile(tilePosition);
			if (tile.State != tileState.UNSET)
			{
				return false;
			}
			int neighbour = 0;
			HashSet<CompanyType> com = new HashSet<CompanyType>();
			HashSet<CompanyType> Safecom = new HashSet<CompanyType>();
			var companys = Game.getInstance().Companys;
			int x = (int)tilePosition.X;
			int y = (int)tilePosition.Y;
			tileState state;
			if (x - 1 >= 0)
			{
				tile = tiles[x - 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if (companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (x + 1 < 12)
			{
				tile = tiles[x + 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if (companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (y - 1 >= 0)
			{
				tile = tiles[x, y - 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if (companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (y + 1 < 9)
			{
				tile = tiles[x, y + 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if (companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (Safecom.Count > 1)
			{
				return false;
			}
			
			com.Remove(CompanyType.NULL);

			if (com.Count == 0)
			{
				if (neighbour == 0)
				{
					return true;
				}
				else
				{
					if (Game.getInstance().getCompanyCount() >= 7)
					{
						return false;
					}
					return true;
				}
			}
			else if (com.Count == 1)
			{
				return true;
			}
			else if (com.Count == 2)
			{
				return true;
			}
			else if (com.Count == 3)
			{
				return true;
			}
			else if (com.Count == 4)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public tileResult putTile(Vector tilePosition)
		{
			Tile tile;
			tile = getTile(tilePosition);
			if(tile.State != tileState.UNSET)
			{
				return tileResult.ERROR;
			}
			int neighbour = 0;
			HashSet<CompanyType> com = new HashSet<CompanyType>();
			HashSet<CompanyType> Safecom = new HashSet<CompanyType>();
			var companys = Game.getInstance().Companys;
			int x = (int)tilePosition.X;
			int y = (int)tilePosition.Y;
			tileState state;
			if (x - 1 >= 0)
			{
				tile = tiles[x - 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if(companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (x + 1 < 12)
			{
				tile = tiles[x + 1, y];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if (companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (y - 1 >= 0)
			{
				tile = tiles[x, y - 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if (companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (y + 1 < 9)
			{
				tile = tiles[x, y + 1];
				state = tile.State;
				if (state != tileState.UNSET)
				{
					neighbour++;
					if (state != tileState.SINGLE)
					{
						if (companys[tile.Company].getState() >= CompanyState.SAFE)
						{
							Safecom.Add(tile.Company);
						}
						com.Add(tile.Company);
					}
				}
			}

			if (Safecom.Count > 1)
			{
				return tileResult.ERROR;
			}

			getTile(tilePosition).State = tileState.SINGLE;
			getTile(tilePosition).Company = CompanyType.SINGLE;
			com.Remove(CompanyType.NULL);
			
			if (com.Count == 0)
			{
				if(neighbour == 0)
				{
					return tileResult.INDEPENDENT;
				}
				else
				{
					if(Game.getInstance().getCompanyCount() >= 7)
					{
						getTile(tilePosition).State = tileState.UNSET;
						getTile(tilePosition).Company = CompanyType.NULL;
						return tileResult.ERROR;
					}
					return tileResult.CREATE;
				}
			}
			else if(com.Count == 1)
			{
				return tileResult.ATTACH;
			}
			else if(com.Count == 2)
			{
				return tileResult.MERGER;
			}
			else if(com.Count == 3)
			{
				return tileResult.TRIPLE;
			}
			else if(com.Count == 4)
			{
				return tileResult.FOUR;
			}
			else
			{
				getTile(tilePosition).State = tileState.UNSET;
				getTile(tilePosition).Company = CompanyType.NULL;
				return tileResult.ERROR;
			}
		}
	}
}
