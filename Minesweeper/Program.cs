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
			using (var vs = new Fusion.VideoStreaming.StreamingProcess())
			using (var game = new Game())
			{
				if (Fusion.Development.DevCon.Prepare(game, @"..\..\..\Content\Content.xml", "Content"))
				{
					new Thread(() =>
					{
						game.Run(args);
					}).Start();
					new Thread(() =>
					{
						vs.Start();
					}).Start();
				}
			}
		}
	}
}
