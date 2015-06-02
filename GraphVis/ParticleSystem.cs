using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Globalization;

using Fusion;
using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;
using Fusion.Mathematics;



namespace GraphVis
{

    public enum IntegratorType
    {
        EULER = 0x8,
        RUNGE_KUTTA = 0x8 << 1
    }

    public class ParticleConfig
    {

        float maxParticleMass;
        float minParticleMass;
        float rotation;
        IntegratorType iType;
        int bankNodes;
        int customerNodes;
		


        [Category("Particle mass")]
        [Description("Largest particle mass")]
        public float Max_mass { get { return maxParticleMass; } set { maxParticleMass = value; } }

        [Category("Particle mass")]
        [Description("Smallest particle mass")]
        public float Min_mass { get { return minParticleMass; } set { minParticleMass = value; } }

        [Category("Integrator type")]
        [Description("Integrator type")]
        public IntegratorType IType { get { return iType; } set { iType = value; } }

        [Category("Initial rotation")]
        [Description("Rate of initial rotation")]
        public float Rotation { get { return rotation; } set { rotation = value; } }

        [Category("Number of banks")]
        [Description("Number of banks")]
        public int BankNodes { get { return bankNodes; } set { bankNodes = value; } }

        [Category("Number of customers")]
        [Description("Number of customers")]
        public int CustomerNodes { get { return customerNodes; } set { customerNodes = value; } }

        public ParticleConfig()
        {
            minParticleMass = 0.001f;
            maxParticleMass = 0.001f;
            rotation = 2.6f;
            iType = IntegratorType.RUNGE_KUTTA;
            bankNodes = 100;
            customerNodes = 3000;
        }
    }

	public enum State
	{
		RUN,
		PAUSE
	}

    public class ParticleSystem : GameService
    {


        [Config]
        public ParticleConfig cfg { get; set; }

        Texture2D texture;
		Texture2D legend;
		Texture2D line;

        Ubershader shader;
		SpriteFont font1;

        public State state;

        const int BlockSize = 512;

        const int MaxInjectingParticles = 3100;
        const int MaxSimulatedParticles = MaxInjectingParticles;

        float MaxParticleMass;
        float MinParticleMass;
        float spinRate;
        float linkSize;

        int[,] linkArr;

		bool[] bankrupts;

        int injectionCount = 0;
        Particle3d[] injectionBufferCPU;

        StructuredBuffer simulationBufferSrc;

		//List<PlotDataList> dataPlot1;
		//List<PlotDataList> dataPlot2;
		//List<PlotDataList> dataPlot3;
		//List<PlotDataList> dataPlot4;
		//List<PlotDataList> dataPlot5;

		PlotDataList Plot1;
		PlotDataList Plot2;
		PlotDataList Plot3;
		PlotDataList Plot4;
		PlotDataList Plot5;

		float[] maxArr;

        StructuredBuffer simulationBufferDst;
        StructuredBuffer linksPtrBuffer;
        LinkId[] linksPtrBufferCPU;

        StructuredBuffer linksBuffer;
        Link[] linksBufferCPU;

        int[] linkCount;

		bool drawLinks;
		//bool drawGrid;

        ConstantBuffer paramsCB;
        List<List<int>> linkPtrLists;

        List<Link> linkList;
        public List<Particle3d> ParticleList;


		int currentParticle;

        string[] dstrings;
        public static int[,] data;
        public double[,] dataSt;


		//float[] arrayData1;
		//float[] arrayData2;
		//float[] arrayData3;
		//float[] arrayData4;
		//float[] arrayData5;


		//int plotCounter;
		//float prevcoord = 0;

        // Particle in 3d space:
        [StructLayout(LayoutKind.Explicit)]
        public struct Particle3d
        {
            [FieldOffset(0)]
            public Vector3 Position;
            [FieldOffset(12)]
            public Vector3 Velocity;
            [FieldOffset(24)]
            public Vector4 Color0;
            [FieldOffset(40)]
            public float Size0;
            [FieldOffset(44)]
            public float TotalLifeTime;
            [FieldOffset(48)]
            public float LifeTime;
            [FieldOffset(52)]
            public int linksPtr;
            [FieldOffset(56)]
            public int linksCount;
            [FieldOffset(60)]
            public Vector3 Acceleration;
            [FieldOffset(72)]
            public float Mass;
            [FieldOffset(76)]
            public float Charge;
            [FieldOffset(80)]
            public int Id;
			[FieldOffset(84)]
			public int Type;
			[FieldOffset(88)]
			public float Assets;
			[FieldOffset(92)]
			public int Links;

			//Type = 1 BANK
			//Type = 2 CUSTOMER
			//Type = 3 SLEEPING CUSTOMER
			//Type = 4 BANKRUPT'S NEIGHBOUR
			//Type = 5 BANKRUPT
			//Type = 6 PLOT


            public override string ToString()
            {
                return string.Format("life time = {0}/{1}", LifeTime, TotalLifeTime);
            }

        }


        // link between 2 particles:
        [StructLayout(LayoutKind.Explicit)]
        struct Link
        {
            [FieldOffset(0)]
            public int par1;
            [FieldOffset(4)]
            public int par2;
            [FieldOffset(8)]
            public float length;
            [FieldOffset(12)]
            public float force2;
            [FieldOffset(16)]
            public Vector3 orientation;
			[FieldOffset(28)]
			public float weight;
			[FieldOffset(32)]
			public int linkType;
        }


        [StructLayout(LayoutKind.Explicit)]
        struct LinkId
        {
            [FieldOffset(0)]
            public int id;
        }

        enum Flags
        {
            // for compute shader: 
            INJECTION = 0x1,
            SIMULATION = 0x1 << 1,
            MOVE = 0x1 << 2,
            EULER = 0x1 << 3,
            RUNGE_KUTTA = 0x1 << 4,
            // for geometry shader:
            POINT = 0x1 << 5,
            LINE = 0x1 << 6,
            COLOR = 0x1 << 7
        }



        [StructLayout(LayoutKind.Explicit, Size=160)]
        struct Params
        {
            [FieldOffset(0)]
            public Matrix View;
            [FieldOffset(64)]
            public Matrix Projection;
            [FieldOffset(128)]
            public int MaxParticles;
            [FieldOffset(132)]
            public float DeltaTime;
            [FieldOffset(136)]
            public float LinkSize;
            [FieldOffset(140)]
            public float MouseX;
            [FieldOffset(144)]
            public float MouseY;
        }

        Params param = new Params();

        Random rand = new Random();

        public bool linkptr;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        public ParticleSystem(Game game)
            : base(game)
        {
            cfg = new ParticleConfig();
        }



		StateFactory	factory;


        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            texture		= Game.Content.Load<Texture2D>("smaller3");
			legend = Game.Content.Load<Texture2D>("111");
			line = Game.Content.Load<Texture2D>("line");

			var Dirs = Game.GetService<Directories>();

			Plot1 = new PlotDataList(Dirs.cfg.BaseDirectory + "changedBank");
			Plot2 = new PlotDataList(Dirs.cfg.BaseDirectory + "lostFunds");
			Plot3 = new PlotDataList(Dirs.cfg.BaseDirectory + "probabilisticStability");
			Plot4 = new PlotDataList(Dirs.cfg.BaseDirectory + "averageDegree");
			Plot5 = new PlotDataList(Dirs.cfg.BaseDirectory + "bankruptsCnt");

			shader = Game.Content.Load<Ubershader>("shaders");
			//PlotDataList.texplot = Game.Content.Load<Texture2D>("new2");
			font1 = Game.Content.Load<SpriteFont>("calibri");


			maxArr = new float[5];
			maxArr[0] = Plot1.maxValue;
			maxArr[1] = Plot2.maxValue;
			Console.WriteLine(Plot2.maxValue);
			maxArr[2] = Plot3.maxValue;
			maxArr[3] = Plot4.maxValue;
			maxArr[4] = Plot5.maxValue;

			//PlotDataList.font1 = font1;
			//PlotDataList.font2 = Game.Content.Load<SpriteFont>("calibrismall");

			PlotData.texplot = Game.Content.Load<Texture2D>("new2");
			PlotData.font1 = font1;
			PlotData.font2 = Game.Content.Load<SpriteFont>("calibrismall");

			PlotDataList.font1 = PlotData.font1;
			PlotDataList.font2 = PlotData.font2;
			PlotDataList.texplot = PlotData.texplot;

			factory	= new StateFactory( shader, typeof(Flags), (ps,i) => Enum( ps, (Flags)i ) );

            var inpdevice = Game.InputDevice;
			//dataPlot1 = new List<PlotDataList>();
			//dataPlot2 = new List<PlotDataList>();
			//dataPlot3 = new List<PlotDataList>();
			//dataPlot4 = new List<PlotDataList>();
			//dataPlot5 = new List<PlotDataList>();

            inpdevice.KeyDown += InputDevice_KeyDown;


            paramsCB = new ConstantBuffer(Game.GraphicsDevice, typeof(Params));

            MaxParticleMass = cfg.Max_mass;
            MinParticleMass = cfg.Min_mass;
            spinRate = cfg.Rotation;
            linkSize = 1.0f;
			drawLinks = true;

			//drawGrid = false; 
			linkptr = true;
			currentParticle = -1;
            linkList		= new List<Link>();
            ParticleList	= new List<Particle3d>();
            linkPtrLists	= new List<List<int>>();
            linkArr			= new int[MaxInjectingParticles, MaxInjectingParticles];
            linkCount		= new int[MaxInjectingParticles];
			bankrupts		= new bool[cfg.BankNodes];
			//plotCounter = 0;
            state = State.RUN;

            base.Initialize();
        }

		public float[] setArticleData(string path) 
		
		{
			string[] plotData = File.ReadAllLines(path);
			float[] arrayData = plotData.Select(float.Parse).ToArray<float>();
			return arrayData;
		}

		int bunkruptcounter = 0;

		public void killBankruptLinks()
		{
			//if (bunkruptcounter == 4)
			{

				for (int i = 0; i < cfg.BankNodes; i++)
				{
					if (ParticleList[i].Type == 4)
					{

						Particle3d newPrt1 = ParticleList[i];
						newPrt1.Type = 1;
						ParticleList[i] = newPrt1;
					}

				}

				//for (int i = cfg.BankNodes; i < ParticleList.Count; i++)
				//{
				//	if (ParticleList[i].Type == 4)
				//	{

				//		Particle3d newPrt1 = ParticleList[i];
				//		newPrt1.Type = 2;
				//		ParticleList[i] = newPrt1;
				//	}

				//}


				for (int i = 0; i < linkList.Count; i++)
				{

					Link newLink = linkList[i];
					newLink.linkType = 0;
					linkList[i] = newLink;

				}

				//bunkruptcounter = 0;
			}

			//bunkruptcounter++;
		}

		public void findTheBankruptNeighbors(int BankruptsId)
		{

			for (int i = 0; i < linkList.Count; i++)
			{
				if (linkList[i].par1 == BankruptsId || linkList[i].par2 == BankruptsId)
				{

					Link newLink = linkList[i];
					newLink.linkType = 1;
					linkList[i] = newLink;

					//int end1 = linkList[i].par1;
					//int end2 = linkList[i].par2;

					//Particle3d newPrt1 = ParticleList[end1];
					//Particle3d newPrt2 = ParticleList[end2];

					//newPrt1.Type = 4;
					//newPrt2.Type = 4;

					//ParticleList[end1] = newPrt1;
					//ParticleList[end2] = newPrt2;

				}

			}
		}

		void Enum ( PipelineState ps, Flags flag )
		{

			ps.Primitive			=	Primitive.PointList;
			ps.RasterizerState		=	RasterizerState.CullNone;
			if (flag.HasFlag(Flags.LINE))
			{
				ps.BlendState = BlendState.AlphaBlend;
				ps.DepthStencilState = DepthStencilState.Readonly;
			}
			else
			{
				ps.BlendState = BlendState.AlphaBlend;
				ps.DepthStencilState = DepthStencilState.Readonly;
			}	
		}



        public void Pause()
        {
            if (state == State.RUN)
            {
                state = State.PAUSE;
            }
            else
            {
                state = State.RUN;
            }
        }


		public void StopDrawindLinks()
		{
			if (drawLinks == true)
			{
				drawLinks = false;
			}
			else
			{
				drawLinks = true;
			}
		}



        /// <summary>
        /// Returns random radial vector
        /// </summary>
        /// <returns></returns>
        Vector3 RadialRandomVector()
        {
            Vector3 r;
            do
            {
                r = rand.NextVector3(-Vector3.One, Vector3.One);
            } while (r.Length() > 1);

            r.Normalize();

            return r;
        }


        public void AddMaxParticles(int N = MaxInjectingParticles)
        {
            ParticleList.Clear();
            linkList.Clear();
            linkPtrLists.Clear();
            addChain(N);

            setBuffers();

        }


		public void drawPlot(int count)
		{

			Plot1.AddDot(count);
			Plot2.AddDot(count);
			Plot3.AddDot(count);
			Plot4.AddDot(count);
			Plot5.AddDot(count);

		}


		void addParticle(Vector3 pos, float lifeTime, float size0, int type, int id, float colorBoost = 1, float charge = 0.05f)
        {
            float ParticleMass = rand.NextFloat(MinParticleMass, MaxParticleMass); //mass

            ParticleList.Add(new Particle3d
            {
                Position = pos,
                Velocity = Vector3.Zero,
                Color0 = rand.NextVector4(Vector4.Zero, Vector4.One) * colorBoost,
                Size0 = size0,
                TotalLifeTime = lifeTime,
                LifeTime = 0,
                Acceleration = Vector3.Zero,
                Mass = ParticleMass,
                Charge = charge,
				Id = id,
				Type = type,
				Assets = 100,
				Links = 0
            }
            );
            linkPtrLists.Add(new List<int>());


        }


        public void addLink(int end1, int end2, int linkWeight)
        {

				if (linkArr[end1, end2] == 0)
				{
					int linkNumber = linkList.Count;

					linkList.Add(new Link
					{
						par1 = end1,
						par2 = end2,
						length = linkSize,
						force2 = 0,
						orientation = Vector3.Zero,
						weight = linkWeight,
						linkType = 0,
					}
					);

					linkArr[end1, end2] = 1;
					linkArr[end2, end1] = 1;


					if (linkPtrLists.ElementAtOrDefault(end1) == null)
					{
						linkPtrLists.Insert(end1, new List<int>());
					}
					linkPtrLists[end1].Add(linkNumber);

					if (linkPtrLists.ElementAtOrDefault(end2) == null)
					{
						linkPtrLists.Insert(end1, new List<int>());
					}
					linkPtrLists[end2].Add(linkNumber);


					Particle3d newPrt1 = ParticleList[end1];
					Particle3d newPrt2 = ParticleList[end2];
					newPrt1.linksCount += 1;
					newPrt2.linksCount += 1;

					if (newPrt1.Type == 3)
					{
						newPrt1.Charge = 0.02f;
						newPrt1.Type = 2;
					}

					if (newPrt2.Type == 3)
					{
						newPrt2.Charge = 0.02f;
						newPrt2.Type = 2;
					}

					ParticleList[end1] = newPrt1;
					ParticleList[end2] = newPrt2;

				}
			
        }

		public void changelayout()
		{

			var cam = Game.GetService<OrbitCamera>();

				StereoEye stereoEye = Fusion.Graphics.StereoEye.Mono;

				var viewMtx = cam.GetViewMatrix(stereoEye);
				var projMtx = cam.GetProjectionMatrix(stereoEye);


				var inp = Game.InputDevice;


				int w = Game.GraphicsDevice.DisplayBounds.Width;
				int h = Game.GraphicsDevice.DisplayBounds.Height;

				param.MouseX = 2.0f * (float)inp.MousePosition.X / (float)w - 1.0f;
				param.MouseY = -2.0f * (float)inp.MousePosition.Y / (float)h + 1.0f;
			
				for (int i = 0; i < ParticleList.Count; i++)
				{

					var element = ParticleList[i];
					//element.Position = injectionBufferCPU[i].Position;
					element.Position.Y = element.Assets / 50;
					//element.Position.X += element.Assets / 100;
					ParticleList[i] = element;
					//drawLinks = false;

				}

				injectionBufferCPU = new Particle3d[ParticleList.Count];
				int iter = 0;

				if (simulationBufferSrc != null)
				{
					simulationBufferSrc.GetData(injectionBufferCPU);
					simulationBufferSrc.Dispose();

					foreach (var p in ParticleList)
					{
						injectionBufferCPU[iter].linksCount = p.linksCount;
						injectionBufferCPU[iter].Charge = p.Charge;
						injectionBufferCPU[iter].Type = p.Type;
						injectionBufferCPU[iter].Assets = p.Assets;
						injectionBufferCPU[iter].Position.Y	= p.Position.Y;


						++iter;
					}
				}
				else
				{

					foreach (var p in ParticleList)
					{
						injectionBufferCPU[iter] = p;
						++iter;
					}
				}
				linksBufferCPU = new Link[linkList.Count];
				iter = 0;
				foreach (var l in linkList)
				{
					linksBufferCPU[iter] = l;
					linksBufferCPU[iter].weight = l.weight;
					linksBufferCPU[iter].linkType = l.linkType;
					++iter;
				}
				if (linkptr == true)
				{
					linksPtrBufferCPU = new LinkId[linkList.Count * 2];
					iter = 0;
					int lpIter = 0;
					foreach (var ptrList in linkPtrLists)
					{

						int blockSize = 0;
						injectionBufferCPU[iter].linksPtr = lpIter;
						if (ptrList != null)
						{
							foreach (var linkPtr in ptrList)
							{
								linksPtrBufferCPU[lpIter] = new LinkId { id = linkPtr };
								++lpIter;
								++blockSize;
							}
						}
						injectionBufferCPU[iter].linksCount = blockSize;
						++iter;
					}
				}


				if (linksBuffer != null)
				{
					linksBuffer.Dispose();
				}

				if (linksPtrBuffer != null)
				{
					linksPtrBuffer.Dispose();
				}


				if (injectionBufferCPU.Length != 0)
				{
					simulationBufferSrc = new StructuredBuffer(Game.GraphicsDevice, typeof(Particle3d), injectionBufferCPU.Length, StructuredBufferFlags.Counter);
					simulationBufferSrc.SetData(injectionBufferCPU);
				}
				if (linksBufferCPU.Length != 0)
				{
					linksBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(Link), linksBufferCPU.Length, StructuredBufferFlags.Counter);
					linksBuffer.SetData(linksBufferCPU);
				}
				if (linksPtrBufferCPU.Length != 0)
				{
					linksPtrBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(LinkId), linksPtrBufferCPU.Length, StructuredBufferFlags.Counter);
					linksPtrBuffer.SetData(linksPtrBufferCPU);
				}

				StopDrawindLinks();
				//StopDrawindGrid();
			}			
		

		public void setAssets(string path) 
		
		{

			var Dirs = Game.GetService<Directories>();
			dstrings = File.ReadAllLines(path);

			if (dstrings.Length > 0)
			{

				for (int i = 0; i < cfg.BankNodes ; i++)
				{

					string[] split;
					split = dstrings[i].Split(new Char[] { '\t', ' ', ',' });

					var element = ParticleList.ElementAt(i);
					element.Assets = float.Parse(split[1]);
					//element.Size0 = 10 + element.Assets / 500;
					//if (element.Size0 > 40) {
					//	element.Size0 = 40;
					//}
					//element.Size0 = (float)(10 + Math.Log(element.Assets));

					//if (element.Assets == 0)
					//{
					//	element.Charge = 0;

					//}

					ParticleList[i] = element;

				}
			}
		
		}

		public void setBunkrupts(string path)
		{
			killBankruptLinks();
			var Dirs = Game.GetService<Directories>();
			dstrings = File.ReadAllLines(path);

			if (dstrings.Length > 0)
			{

				for (int i = 0; i < dstrings.Length; i++)
				{
					int id = int.Parse(dstrings[i]);

					var element = ParticleList.ElementAt(id);

					//if (element.linksCount == 0)
					{
						element.Type = 5;
						element.Charge = 0.005f;
					}

					ParticleList[id] = element;
					if (bankrupts[id] == false)
					{
						findTheBankruptNeighbors(id);
						bankrupts[id] = true;
					}
				}
			}

		}


        void addChain(int N)
        {
      //      Vector3 pos = new Vector3(0, 0, -200);
			Vector3 pos = new Vector3(70, 0, -240);

			for (int i = 0; i < cfg.BankNodes; ++i)
			{
				addParticle(pos, 9999, 10.0f, 1, i, 1.0f, 0.035f);
				pos += RadialRandomVector() * linkSize;
			}
			for (int i = cfg.BankNodes; i <= N; ++i)
			{
				addParticle(pos, 9999, 4.0f, 3, i, 1.0f, 0);
				pos += RadialRandomVector() * linkSize;
			}

        }

		public void clearBuffers()
		{
			linkList.Clear();

			foreach (var list in linkPtrLists)
			{
				list.Clear();
			}
		}

        public void createLinksFromFile(string fName)
        {
			
			dstrings = File.ReadAllLines(fName);

            if (dstrings.Length > 0)
            {
               // data = new int[dstrings.Length, dstrings[0].Length / 2 - 1];

                for (int i = 0; i < dstrings.Length; i++)
                {
                    string[] split;
                    split = dstrings[i].Split(new Char[] { ' ', ',' });

					addLink(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));

                }
            }

            setBuffers();
        }


        public void removeLinksFromFile(string fName)
        {
            dstrings = File.ReadAllLines(fName);

            if (dstrings.Length > 0)
            {
                data = new int[dstrings.Length, dstrings[0].Length / 2 - 1];

                for (int i = 0; i < dstrings.Length; i++)
                {
                    string[] split;
                    split = dstrings[i].Split(new Char[] { ' ', ',' });

                    int end1 = int.Parse(split[0]);
                    int end2 = int.Parse(split[1]);

        //                linkList.RemoveAll(list => list.par1 == end1 && list.par2 == end2);
						int index = linkList.FindIndex(list => list.par1 == end1 && list.par2 == end2);
						if (index >= 0)
						{
							var link = linkList[index];
							link.par1 = link.par2;
							linkList[index] = link;

							if (linkPtrLists[end1] != null)
								//    linkPtrLists[end1].Clear();
								linkPtrLists[end1].RemoveAll(ind => ind == index);
							if (linkPtrLists[end2] != null)
								//     linkPtrLists[end2].Clear();
								linkPtrLists[end2].RemoveAll(ind => ind == index);

							linkArr[end1, end2] = 0;
							linkArr[end2, end1] = 0;

							Particle3d newPrt1 = ParticleList[end1];
							Particle3d newPrt2 = ParticleList[end2];
							newPrt1.linksCount -= 1;
							newPrt2.linksCount -= 1;

							if (newPrt1.linksCount == 0)
							{
								newPrt1.Charge	= 0.04f;
								newPrt1.Type	= 3;
							}

							if (newPrt2.linksCount == 0)
							{
								newPrt2.Charge = 0.04f;
								newPrt2.Type = 3;
							}

							ParticleList[end1] = newPrt1;
							ParticleList[end2] = newPrt2;


						}
				

                }
            }

            setBuffers();
        }

        void setBuffers()
        {
            injectionBufferCPU = new Particle3d[ParticleList.Count];
            int iter = 0;

            if (simulationBufferSrc != null)
            {
                simulationBufferSrc.GetData(injectionBufferCPU);
                simulationBufferSrc.Dispose();

                foreach (var p in ParticleList)
                {
					injectionBufferCPU[iter].linksCount = p.linksCount;
					injectionBufferCPU[iter].Charge = p.Charge;
					injectionBufferCPU[iter].Type = p.Type;
					injectionBufferCPU[iter].Assets = p.Assets;
					injectionBufferCPU[iter].Size0 = p.Size0;
					//injectionBufferCPU[iter].Position.Y	= p.Position.Y;


                    ++iter;
                }
            }
            else
            {

                foreach (var p in ParticleList)
                {
                    injectionBufferCPU[iter] = p;
                    ++iter;
                }
            }
            linksBufferCPU = new Link[linkList.Count];
            iter = 0;
            foreach (var l in linkList)
            {
                linksBufferCPU[iter] = l;
				linksBufferCPU[iter].weight = l.weight;
				linksBufferCPU[iter].linkType = l.linkType;
                ++iter;	
            }
            if (linkptr == true)
            {
                linksPtrBufferCPU = new LinkId[linkList.Count * 2];
                iter = 0;
                int lpIter = 0;
                foreach (var ptrList in linkPtrLists)
                {

                    int blockSize = 0;
                    injectionBufferCPU[iter].linksPtr = lpIter;
                    if (ptrList != null)
                    {
                        foreach (var linkPtr in ptrList)
                        {
                            linksPtrBufferCPU[lpIter] = new LinkId { id = linkPtr };
                            ++lpIter;
                            ++blockSize;
                        }
                    }
                    injectionBufferCPU[iter].linksCount = blockSize;
                    ++iter;
                }
            }


            if (linksBuffer != null)
            {
                linksBuffer.Dispose();
            }

            if (linksPtrBuffer != null)
            {
                linksPtrBuffer.Dispose();
            }


            if (injectionBufferCPU.Length != 0)
            {
                simulationBufferSrc = new StructuredBuffer(Game.GraphicsDevice, typeof(Particle3d), injectionBufferCPU.Length, StructuredBufferFlags.Counter);
                simulationBufferSrc.SetData(injectionBufferCPU);
            }
            if (linksBufferCPU.Length != 0)
            {
                linksBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(Link), linksBufferCPU.Length, StructuredBufferFlags.Counter);
                linksBuffer.SetData(linksBufferCPU);
            }
            if (linksPtrBufferCPU.Length != 0)
            {
                linksPtrBuffer = new StructuredBuffer(Game.GraphicsDevice, typeof(LinkId), linksPtrBufferCPU.Length, StructuredBufferFlags.Counter);
                linksPtrBuffer.SetData(linksPtrBufferCPU);
            }

        }


        /// <summary>
        /// Makes all particles wittingly dead
        /// </summary>
        void ClearParticleBuffer()
        {
            for (int i = 0; i < MaxInjectingParticles; i++)
            {
                injectionBufferCPU[i].TotalLifeTime = -999999;

            }
            injectionCount = 0;
        }

		



        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                paramsCB.Dispose();
                if (simulationBufferSrc != null)
                {
                    simulationBufferSrc.Dispose();
                }
                if (linksBuffer != null)
                {
                    linksBuffer.Dispose();
                }
                if (linksPtrBuffer != null)
                {
                    linksPtrBuffer.Dispose();
                }
            }
            base.Dispose(disposing);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			//if (drawGrid == true)
			//{
			//	var cam = Game.GetService<GeoCamera>();
			//	var dr = Game.GetService<DebugRender>();
			//	dr.View = cam.GetViewMatrix(StereoEye.Mono);
			//	dr.Projection = cam.GetProjectionMatrix(StereoEye.Mono);


			//	dr.DrawGrid(10);
			//}
			
        }

        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
			if (Game.InputDevice.IsKeyDown(Keys.LeftButton))
			{
				Console.WriteLine("LEFT BUTTON");

				var cam = Game.GetService<OrbitCamera>();

				StereoEye stereoEye = Fusion.Graphics.StereoEye.Mono;

				var viewMtx = cam.GetViewMatrix(stereoEye);
				var projMtx = cam.GetProjectionMatrix(stereoEye);


				var inp = Game.InputDevice;


				int w = Game.GraphicsDevice.DisplayBounds.Width;
				int h = Game.GraphicsDevice.DisplayBounds.Height;

				param.MouseX = 2.0f * (float)inp.MousePosition.X / (float)w - 1.0f;
				param.MouseY = -2.0f * (float)inp.MousePosition.Y / (float)h + 1.0f;


				//foreach (var part in injectionBufferCPU)
				for (int i = 0; i < cfg.BankNodes; i++)
				{
					var part = injectionBufferCPU[i];
					var worldPos = new Vector4(part.Position, 1);
					var viewPos = Vector4.Transform(worldPos, viewMtx);
					var projPos = Vector4.Transform(viewPos, projMtx);

					if ((Math.Abs(projPos.X / projPos.Z - param.MouseX) < 0.02f) && (Math.Abs(projPos.Y / projPos.Z - param.MouseY) < 0.2f))
					{
						Console.WriteLine("Matched!");
						currentParticle = part.Id;
						Console.WriteLine(currentParticle);
					}
				}

			}

            if (Game.InputDevice.IsKeyDown(Keys.Q))
            {
                Pause();
            }
			
			if (Game.InputDevice.IsKeyDown(Keys.B))
            {
				changelayout();
				Console.WriteLine("B");
				Pause();
            }

			if (Game.InputDevice.IsKeyDown(Keys.V))
			{
				StopDrawindLinks();
				Console.WriteLine("V");
			}

			if (Game.InputDevice.IsKeyDown(Keys.M))
			{
				drawPlot(1);
			}
			
        }


        /// <summary>
        /// 
        /// </summary>
        void SwapParticleBuffers()
        {
            var temp = simulationBufferDst;
            simulationBufferDst = simulationBufferSrc;
            simulationBufferSrc = temp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        public override void Draw(GameTime gameTime, Fusion.Graphics.StereoEye stereoEye)
        {
			Game.GraphicsDevice.ClearBackbuffer(Color.White, 1, 0);
			var device	= Game.GraphicsDevice;
			var cam = Game.GetService<OrbitCamera>();
            var inp		= Game.InputDevice;

            int w = Game.GraphicsDevice.DisplayBounds.Width;
            int h = Game.GraphicsDevice.DisplayBounds.Height;

            param.View			= cam.GetViewMatrix(stereoEye);
            param.Projection	= cam.GetProjectionMatrix(stereoEye);
            param.MaxParticles	= 0;
            param.DeltaTime		= gameTime.ElapsedSec;
            param.LinkSize		= linkSize;



            device.ComputeShaderConstants[0]	= paramsCB;
            device.VertexShaderConstants[0]		= paramsCB;
            device.GeometryShaderConstants[0]	= paramsCB;
            device.PixelShaderConstants[0]		= paramsCB;


            device.PixelShaderSamplers[0]		= SamplerState.LinearWrap;


            //	Simulate : ------------------------------------------------------------------------
            //

            param.MaxParticles = injectionCount;
            paramsCB.SetData(param);


            device.ComputeShaderConstants[0] = paramsCB;



				if (state == State.RUN)
				{
					for (int i = 0; i < 10; i++)
					{

					// calculate accelerations: ---------------------------------------------------
					device.SetCSRWBuffer(0, simulationBufferSrc, MaxSimulatedParticles);

					device.ComputeShaderResources[2] = linksPtrBuffer;
					device.ComputeShaderResources[3] = linksBuffer;

					param.MaxParticles = MaxSimulatedParticles;
					paramsCB.SetData(param);

					device.ComputeShaderConstants[0] = paramsCB;

					device.PipelineState = factory[(int)Flags.SIMULATION | (int)cfg.IType];


					device.Dispatch(MathUtil.IntDivUp(MaxSimulatedParticles, BlockSize));




					// move particles: ------------------------------------------------------------
					device.SetCSRWBuffer(0, simulationBufferSrc, MaxSimulatedParticles);
					device.ComputeShaderConstants[0] = paramsCB;



					device.PipelineState = factory[(int)Flags.MOVE | (int)cfg.IType];
					device.Dispatch(MathUtil.IntDivUp(MaxSimulatedParticles, BlockSize));//*/




				}
			}
            // ------------------------------------------------------------------------------------


            //	Render: ---------------------------------------------------------------------------
            //

			device.SetCSRWBuffer(0, null);

			// draw lines: --------------------------------------------------------------------------

			if (drawLinks == true)
			{
				device.PipelineState = factory[(int)Flags.LINE];


				device.GeometryShaderResources[1] = simulationBufferSrc;
				device.GeometryShaderResources[3] = linksBuffer;


				device.Draw(linkList.Count, 0);

			}


            // draw points: ------------------------------------------------------------------------


            device.PipelineState = factory[(int)Flags.POINT];

			device.PixelShaderSamplers[0] = SamplerState.AnisotropicWrap;
            device.PixelShaderResources[0] = texture;

            device.GeometryShaderResources[1] = simulationBufferSrc;

  //          device.Draw(MaxSimulatedParticles, 0);


            device.Draw(ParticleList.Count, 0);

			

			// --------------------------------------------------------------------------------------

           


            var debStr = Game.GetService<DebugStrings>();

			//debStr.Add("Press I to start simulation");
			//debStr.Add("Press L to speed up simulation");
			//debStr.Add("Press K to slow down simulation");
			//debStr.Add("Press Q to pause/unpause");

			//if (currentParticle != -1)
			//{
			//	if (currentParticle >= 100)
			//	{
			//		debStr.Add(Color.Yellow, "CUSTOMER");
			//		debStr.Add(Color.Yellow, "links Count " + ParticleList[currentParticle].linksCount);
			//	}
			//	else {
			//		debStr.Add(Color.Yellow, "Asset " + ParticleList[currentParticle].Assets);
			//		debStr.Add(Color.Yellow, "links Count " + ParticleList[currentParticle].linksCount);
			//		debStr.Add(Color.Yellow, "Id " + ParticleList[currentParticle].Id);
			//	}

			//}
			var sb = Game.GetService<SpriteBatch>();
			//int w = Game.GraphicsDevice.DisplayBounds.Width;
			//int h = Game.GraphicsDevice.DisplayBounds.Height;
			sb.Begin();
			//sb.DrawSprite(line, w - 50, h - 50, line.Width, line.Height / 3, 0, Color.White);

			Color col = Color.Red;
			col.G += 100;
			col.B += 100;
			sb.DrawSprite(texture, w - 170, h - 125, 40, 40, 0, Color.Green);
			sb.DrawSprite(texture, w - 170, h - 95, 40, 40, 0, Color.Blue);
			sb.DrawSprite(texture, w - 170, h - 65, 40, 40, 0, col);

			font1.DrawString(sb, "Bank", w - 150, h - 122, Color.Black);
			font1.DrawString(sb, "Customer", w - 150, h - 91, Color.Black);
			font1.DrawString(sb, "Bankrupt", w - 150, h - 61, Color.Black);

			foreach (var plot in Plot1.dataPlot)
			{
				plot.Draw(sb, h, 1, w, Plot1.scale, maxArr);
			}

			foreach (var plot in Plot2.dataPlot)
			{
				plot.Draw(sb, h, 2, w, Plot2.scale, maxArr);
			}

			foreach (var plot in Plot3.dataPlot)
			{
				plot.Draw(sb, h, 3, w, Plot3.scale, maxArr);
			}

			foreach (var plot in Plot4.dataPlot)
			{
				plot.Draw(sb, h, 4, w, Plot4.scale, maxArr);
			}

			foreach (var plot in Plot5.dataPlot)
			{
				plot.Draw(sb, h, 5, w, Plot5.scale, maxArr);
			}


			Plot1.Draw(sb, h, 5, w, Plot5.scale, Plot5.maxValue);
			//bool a = true;
			//if (locki == true)
			//{

			//	for (int i = 0; i < 100; i++)
			//	{
			//		foreach (var plot in dataPlot1)
			//		{
			//			plot.x -= 0.01f;
			//		}
			//	}
			//}



		//	sb.Restart();
			//font1.DrawString(sb, "Lenna Soderberg", 130, 230, Color.Plum);
			sb.End();



			//debStr.Add(Color.Yellow, "drawing " + ParticleList.Count + " points");
			//debStr.Add(Color.Yellow, "drawing " + linkList.Count + " lines");


            base.Draw(gameTime, stereoEye);
        }

    }

}

