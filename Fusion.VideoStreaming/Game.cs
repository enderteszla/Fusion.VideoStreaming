using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;

namespace Fusion.VideoStreaming
{
	public class Game : Fusion.Game
	{
		private DateTime nextSnapshotTime;
		private TimeSpan snapshotPeriod;
		private Wheel wheel;

		private struct Wheel
		{
			private String[] FileNames;
			private int c;
			public Wheel(int number)
			{
				FileNames = new String[number];
				String directory = Properties.Settings.Default.fakeImageDir;
				String name = Properties.Settings.Default.fakeImageName;
				String extension = Properties.Settings.Default.fakeImageExtension;
				for (var i = 0; i < FileNames.Length; i++)
				{
					FileNames[i] = String.Format(@"{0}{1}{2,3:000}.{3}", directory, name, i, extension);
				}
				c = 0;
			}
			public Wheel next()
			{
				c = (c < FileNames.Length - 1) ? c + 1 : 0;
				return this;
			}
			public String current()
			{
				return FileNames[c];
			}
		}

		public Game() : base() {
			nextSnapshotTime = DateTime.Now;
			snapshotPeriod = Properties.Settings.Default.snapshotPeriod;
			wheel = new Wheel(Properties.Settings.Default.wheelLength);
		}

		private void backBufferToBitmap(out Bitmap bitmap)
		{
			RenderTarget2D rt = GraphicsDevice.BackbufferColor;
			int w = GraphicsDevice.DisplayBounds.Width;
			int h = GraphicsDevice.DisplayBounds.Height;

			Fusion.Mathematics.Color[] colors = new Fusion.Mathematics.Color[4 * w * h];
			rt.GetData<Fusion.Mathematics.Color>(colors, 0, 4 * w * h - 1);

			bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
			BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
			var bytes = new byte[bmpData.Stride * bmpData.Height];
			for (var i = 0; i < bitmap.Height; i++)
			{
				for (var j = 0; j < bitmap.Width; j++)
				{
					bytes[i * bmpData.Stride + 4 * j] = colors[i * bmpData.Width + j].B;
					bytes[i * bmpData.Stride + 4 * j + 1] = colors[i * bmpData.Width + j].G;
					bytes[i * bmpData.Stride + 4 * j + 2] = colors[i * bmpData.Width + j].R;
					bytes[i * bmpData.Stride + 4 * j + 3] = 255;
				}
			}
			Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
			bitmap.UnlockBits(bmpData);
		}

		protected override void Draw(Fusion.GameTime gameTime, StereoEye stereoEye)
		{
			base.Draw(gameTime, stereoEye);
			if (DateTime.Now > nextSnapshotTime)
			{
				Bitmap bmp;
				backBufferToBitmap(out bmp);
				bmp.Save(wheel.current(), ImageFormat.Png);
				bmp.Dispose();
				nextSnapshotTime = nextSnapshotTime.Add(snapshotPeriod);
				wheel.next();
			}
		}
	}
}
