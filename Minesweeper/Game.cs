using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Mathematics;
using Fusion.Development;
using Fusion.Input;
using Fusion.VideoStreaming;

namespace Minesweeper
{
    public class Game : Visualisator
    {
        /// <summary>
        /// Minesweeper constructor
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
            AddService(new Camera(this), true, false, 1, 1);

            //	add here additional services :
            AddService(new World(this), true, true, 500, 500);

            //	load configuration for each service :
            LoadConfiguration();

            //	make configuration saved on exit :
            Exiting += Game_Exiting;
		}


        /// <summary>
        /// Initializes game :
        /// </summary>
        protected override void Initialize()
        {
            //	initialize services :
            base.Initialize();

            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;
            InputDevice.KeyDown += GetService<World>().InputDevice_KeyDown;
            InputDevice.KeyUp += GetService<World>().InputDevice_KeyUp;

            //	load content & create graphics and audio resources here:
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
		void InputDevice_KeyDown(object sender, InputDevice.KeyEventArgs e)
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
			ds.Add(Color.White, "F1   - show developer console");
			ds.Add(Color.White, "F2   - start new game");
			ds.Add(Color.White, "F5   - build content and reload textures");
			ds.Add(Color.White, "F12  - make screenshot");
			ds.Add(Color.White, "ESC  - exit");

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

            //	Draw stuff here :
        }
    }
}
