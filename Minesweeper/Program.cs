using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Development;
using Fusion.VideoStreaming;

namespace Minesweeper
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			using (var ss = new StreamingServer())
			using (var game = new Game(ss))
			{
				if (Fusion.Development.DevCon.Prepare(game, @"..\..\..\Content\Content.xml", "Content"))
				{
					new Thread(() =>
					{
						ss.Assign(game).Start();
					}).Start();
					game.Run(args);
				}
			}
		}
	}
}
