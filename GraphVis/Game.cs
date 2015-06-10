using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;
using Fusion.VideoStreaming;

namespace GraphVis
{
    public class Game : Visualisator
    {
                /// <summary>
        /// GraphVis constructor
        /// 
        /// </summary>
        public Game()
            : base()
        {
            //	enable object tracking :
            Parameters.TrackObjects = true;

            //	uncomment to enable debug graphics device:
            //	(MS Platform SDK must be installed)
            //	Parameters.UseDebugDevice	=	true;

            //	add services :
            AddService(new SpriteBatch(this), false, false, 0, 0);
            AddService(new DebugStrings(this), true, true, 9999, 9999);
            AddService(new DebugRender(this), true, true, 9998, 9998);
            AddService(new OrbitCamera(this), true, false, 9997, 9997);
			//AddService(new PlotData(this), true, false, 9997, 9997);

            //	add here additional services :
            AddService(new ParticleSystem(this), true, true, 9996, 9996);
            AddService(new Directories(this), true, false, 9997, 9997);


            //	load configuration for each service :
            LoadConfiguration();

            //	make configuration saved on exit :
            Exiting += Game_Exiting;
        }


        string[] edgeDataDirectory;
        string[] stabilityDataDirectory;
        string[] removingEdgesDirectory;
		string[] assetsDirectory;
		string[] bunkruptsDirectory;


        int iterationTimer;
        bool startSimulation;
        int iterationCounter;
        int simulationSpeed;





        /// <summary>q
        /// Initializes game :
        /// </summary>
        protected override void Initialize()
        {
            
			//	initialize services :
            base.Initialize();

            InputDevice.MouseScroll		+= InputDevice_MouseScroll;
			InputDevice.IsMouseHidden	= true;

            var cam = GetService<OrbitCamera>();
			cam.Config.FreeCamEnabled = true;

            iterationTimer		= -1000;
            startSimulation		= false;
            iterationCounter	= 0;
            simulationSpeed		= 4;

            var Dirs				= GetService<Directories>();

            edgeDataDirectory		= Directory.GetFiles(Dirs.cfg.EdgeListDirectory);
            //stabilityDataDirectory	= Directory.GetFiles(Dirs.cfg.StabilityListDirectory);
            removingEdgesDirectory	= Directory.GetFiles(Dirs.cfg.EdgesToRemoveDirectory);
			assetsDirectory			= Directory.GetFiles(Dirs.cfg.AssetsDirectory);
			bunkruptsDirectory		= Directory.GetFiles(Dirs.cfg.BunkruptsDirectory);
          
            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;

            //	load content & create graphics and audio resources here:

        }

        void InputDevice_MouseScroll(object sender, InputDevice.MouseScrollEventArgs e)
        {
            Log.Message("...mouse scroll event : {0}", e.WheelDelta);
            //scrollValue += e.WheelDelta;
        }


        /// <summary>
        /// Disposes game
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //	dispose disposable stuff here
                //	Do NOT dispose objects loaded using ContentManager.
            }
            base.Dispose(disposing);
        }



        /// <summary>
        /// Handle keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.F1)
            {
                DevCon.Show(this);
            }

            if (e.Key == Keys.F5)
            {
                Reload();
            }

            if (e.Key == Keys.F12)
            {
                GraphicsDevice.Screenshot();
            }

            if (e.Key == Keys.Escape)
            {
                Exit();
            }


            if (e.Key == Keys.K)
            {
                if (simulationSpeed < 100)
                {
                    simulationSpeed = simulationSpeed + 5;
                }

                iterationTimer = 0;
            }

            if (e.Key == Keys.L)
            {
                if (simulationSpeed > 10)
                {
                    simulationSpeed = simulationSpeed - 5;
                }

                iterationTimer = 0;
            }

            // if (e.Key == Keys.I)
            if (e.Key == Keys.LeftButton)
            {
                GetService<ParticleSystem>().AddMaxParticles();
                iterationTimer = simulationSpeed - 10;
                startSimulation = true;
            }

            if (e.Key == Keys.Q)
            {
                iterationTimer = 0;
            }

            if (e.Key == Keys.B)
            {
                iterationTimer = 0;
            }

        }



        /// <summary>
        /// Saves configuration on exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Game_Exiting(object sender, EventArgs e)
        {
            SaveConfiguration();
        }



        /// <summary>
        /// Updates game
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            var ds = GetService<DebugStrings>();

            ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);

			var cam = GetService<OrbitCamera>();
            var debRen	= GetService<DebugRender>();

            int w = GraphicsDevice.DisplayBounds.Width;
            int h = GraphicsDevice.DisplayBounds.Height;

            var partSys = GetService<ParticleSystem>();

            iterationTimer++;
			//Console.WriteLine(iterationTimer++);

			if (startSimulation == true && iterationCounter < edgeDataDirectory.Length && iterationTimer == simulationSpeed && partSys.state == State.RUN)
			{

				partSys.createLinksFromFile(edgeDataDirectory[iterationCounter]);
				partSys.setAssets(assetsDirectory[iterationCounter]);
				partSys.setBunkrupts(bunkruptsDirectory[iterationCounter]);
				partSys.removeLinksFromFile(removingEdgesDirectory[iterationCounter]);
				partSys.drawPlot(iterationCounter);

				Console.WriteLine(edgeDataDirectory[iterationCounter] + "   " + iterationCounter);

				iterationTimer = 0;
				iterationCounter = iterationCounter + 1;

			}
           	

            base.Update(gameTime);

            //	Update stuff here :


        }



        /// <summary>
        /// Draws game
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        protected override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
            base.Draw(gameTime, stereoEye);
			
            var sb = GetService<SpriteBatch>();
            sb.Begin();

            sb.End();


            //	Draw stuff here :
        }
    }
}
