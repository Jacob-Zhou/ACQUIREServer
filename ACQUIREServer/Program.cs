using ACQUIREServer.presenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACQUIREServer
{
	class Program
	{
		private static bool isEnd = false;

		static void Main(string[] args)
		{
			try
			{
				int port = int.Parse(args[0]);
				int count = int.Parse(args[1]);
				Console.WriteLine(args[0] + " " + args[1]);
				ServerPresenter.getInstance().Init(port, count);
				GamePresenter.getInstance().Init(count);
				while (!isEnd)
				{
					Thread.Sleep(100);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static void Close()
		{
			isEnd = true;
		}
	}
}
