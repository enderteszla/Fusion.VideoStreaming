using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fusion;
using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;
using Fusion.Mathematics;

namespace GraphVis
{
	class OrbitCamera : Camera
	{
        GameTime GameTime;

		float latitude;
		float longitude;
		float altitude;

		float angularVelocity;
		float latVelocity;
		float lonVelocity;
		float upDownVelocity;

		float zeroRadius;

		const float PI180		= (float)Math.PI / 180;

		public float Altitude
		{
			get { return altitude; }
		}
		public float Latitude
		{
			get { return latitude; }
		}
		public float Longitude
		{
			get { return longitude; }
		}


		public OrbitCamera( Game game ) : base(game)
		{
			
		}


		public override void Initialize()
		{

			base.Initialize();
			
			var rndr = Game.GetService<ParticleSystem>();

			zeroRadius = 0.0f;
			latitude	= 0;
			longitude	= 0;
			altitude	= 150.0f;

			angularVelocity = 0.005f;
			latVelocity		= 0.005f;
			lonVelocity		= 0.005f;
			upDownVelocity	= 1;

            Game.InputDevice.KeyDown += InputDevice_KeyDown;
		}


		public override void Update(GameTime gameTime)
		{		
			Config.FreeCamEnabled	= true;

			var rndr	= Game.GetService<ParticleSystem>();
			var ds		= Game.GetService<DebugStrings>();

			angularVelocity	= 0.25f;
			upDownVelocity = 0.7f;

			

			latVelocity = angularVelocity;
			lonVelocity = angularVelocity;

            if ( altitude < 0.1f ) {
				altitude = 0.1f;
			}

			var input = Game.InputDevice;
			input.IsMouseHidden = false;
			input.IsMouseCentered = false;
			input.IsMouseClipped = true;

			Vector3 cameraLocation = anglesToCoords( latitude, longitude, (zeroRadius + altitude) );
//			base.SetPose( cameraLocation, 0, 0, 0 );
//			base.LookAt( cameraLocation, new Vector3(0, 0, 0), new Vector3( 0, 1, 0) );
			base.SetupCamera( cameraLocation, new Vector3(0, 0, 0), new Vector3( 0, 1, 0), new Vector3(0, 0, 0),
				65.0f, base.Config.FreeCamZNear,base.Config.FreeCamZFar, 0, 0 );
			//base.

			//ds.Add( "Altitude = " + altitude + " m" );
			//ds.Add( "Longitude = " + longitude );
			//ds.Add( "Latitude = " + latitude );

            GameTime = gameTime;

			base.Update(gameTime);
		}


		Vector3 anglesToCoords( float lat, float lon, float radius )
		{
			lat = lat * PI180;
			lon = lon * PI180;
			float rSmall	= radius * (float)( Math.Cos( lat ) );
			float X = (float)( rSmall * Math.Sin( lon ) );
 			float Z = (float)( rSmall * Math.Cos( lon ) );
			float Y = radius * (float)( Math.Sin( lat ) );
			return new Vector3( X, Y, Z );
		}

        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.LeftShift)
            {
                angularVelocity *= 3;
                upDownVelocity *= 3;
            }

            if (e.Key == Keys.W)
            {
                latitude += latitude > 85 ? 0 : latVelocity * GameTime.Elapsed.Milliseconds;

            }
            if (e.Key == Keys.S)
            {
                latitude -= latitude < -85 ? 0 : latVelocity * GameTime.Elapsed.Milliseconds;
            }
            if (e.Key == Keys.A)
            {
                longitude -= lonVelocity * GameTime.Elapsed.Milliseconds;
            }
            if (e.Key == Keys.D)
            {
                longitude += lonVelocity * GameTime.Elapsed.Milliseconds;
            }

            if (e.Key == Keys.Space)
            {
                altitude += upDownVelocity * GameTime.Elapsed.Milliseconds / 3;
            }

            if (e.Key == Keys.C)
            {
                altitude -= upDownVelocity * GameTime.Elapsed.Milliseconds / 3;
            }
        }
	}
}
