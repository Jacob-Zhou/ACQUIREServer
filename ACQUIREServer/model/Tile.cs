using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACQUIREServer.model
{
	class Tile
	{
		private CompanyType company = CompanyType.NULL;
		private tileState state = tileState.UNSET;
		private Vector position = new Vector(-1, -1);

		public Tile(int x, int y)
		{
			position = new Vector(x, y);
		}

		public Tile()
		{
		}

		public CompanyType Company
		{
			get
			{
				return company;
			}

			set
			{
				company = value;
			}
		}

		public tileState State
		{
			get
			{
				return state;
			}

			set
			{
				state = value;
			}
		}

		public Vector Position
		{
			get
			{
				return position;
			}

			set
			{
				position = value;
			}
		}
	}
}
