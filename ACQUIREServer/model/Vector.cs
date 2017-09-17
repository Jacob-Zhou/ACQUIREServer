using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACQUIREServer.model
{
	public class Vector
	{
		public Vector(int x, int y)
		{
			X = x;
			Y = y;
		}

		public Vector()
		{
			X = 0;
			Y = 0;
		}

		public override string ToString()
		{
			return X.ToString() + Y.ToString();
		}

		public int X { get; set; }
		public int Y { get; set; }
	}
}
