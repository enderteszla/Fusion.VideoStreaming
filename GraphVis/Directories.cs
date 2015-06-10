using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;


using Fusion;
using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;


namespace GraphVis
{
	public class Directories : GameService
	{

		public class Configure
		{

			string baseDataDir;
			string edgeDataDir;
			string stabilityDataDir;
			string edgesToRemoveDir;
			string assetsDirectory;
			string bunkruptsDirectory;


			[Category("Base Directory")]
			[Description("Path to edge list data")]
			public string BaseDirectory
			{
				get
				{
					baseDataDir = @"e:\011\";
					edgeDataDir = baseDataDir + @"edgeLists";
					//stabilityDataDir	= baseDataDir + @"stab";
					edgesToRemoveDir = baseDataDir + @"remove";
					assetsDirectory = baseDataDir + @"assets";
					bunkruptsDirectory = baseDataDir + @"bankrupts";
					return baseDataDir;
				}
				set
				{
					baseDataDir = value;
					edgeDataDir = baseDataDir + @"edgeLists";
					//stabilityDataDir	= baseDataDir + @"stab";
					edgesToRemoveDir = baseDataDir + @"remove";
					assetsDirectory = baseDataDir + @"assets";
					bunkruptsDirectory = baseDataDir + @"bankrupts";
				}
			}

			[Category("Directories")]
			[Description("Path to edge list data")]
			public string EdgeListDirectory { get { return edgeDataDir; } }

			[Category("Directories")]
			[Description("Path to edge list data")]
			public string StabilityListDirectory { get { return stabilityDataDir; } }

			[Category("Directories")]
			[Description("Path to removing edges")]
			public string EdgesToRemoveDirectory { get { return edgesToRemoveDir; } }

			[Category("Directories")]
			[Description("Path to Assets")]
			public string AssetsDirectory { get { return assetsDirectory; } }

			[Category("Directories")]
			[Description("Path to Bunkrupts")]
			public string BunkruptsDirectory { get { return bunkruptsDirectory; } }




			public Configure()
			{
				//baseDataDir = @"d:\Data11\";
				edgeDataDir = baseDataDir + @"edgeLists";
				//stabilityDataDir	= baseDataDir + @"stab";
				edgesToRemoveDir = baseDataDir + @"remove";
				assetsDirectory = baseDataDir + @"assets";
				bunkruptsDirectory = baseDataDir + @"bankrupts";

			}

		}


		[Config]
		public Configure cfg { get; set; }

		public Directories(Game game)
			: base(game)
		{
			cfg = new Configure();
		}



		public override void Initialize()
		{
			base.Initialize();
		}

	}
}