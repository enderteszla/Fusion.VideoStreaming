﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Mathematics;

namespace Fusion.VideoStreaming
{
	public class Visualisator : Fusion.Game
	{
		private DateTime nextSnapshotTime;
		private TimeSpan snapshotPeriod;
		private Wheel Wheel;

		public Visualisator()
			: base()
		{
			nextSnapshotTime = DateTime.Now;
			snapshotPeriod = Properties.Settings.Default.snapshotPeriod;
			Wheel = new Wheel(Properties.Settings.Default.wheelLength);
		}

		public void SetStartInstance(DateTime Instance) {
			Wheel.Set((int)((DateTime.Now.Subtract(Instance).Ticks / Properties.Settings.Default.snapshotPeriod.Ticks) % Properties.Settings.Default.wheelLength));
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

		/// <summary>
		/// Loads configuration for each subsystem
		/// </summary>
		/// <param name="path"></param>
		[UICommand("Load Configuration", 1)]
		new public void LoadConfiguration()
		{
			string ConfigPath = String.Format(@"..\Config\{0}", this.GetType().Namespace);
			Log.Message("Loading configuration...");

			foreach (var svc in GetServiceList())
			{
				string name = System.IO.Path.Combine(ConfigPath, string.Format("Config.{0}.xml", svc.GetType().Name));
				var cfgProp = Inherited.GetConfigProperty(svc.GetType());

				if (cfgProp == null)
				{
					continue;
				}

				Log.Message("Loading : {0}", name);

				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(name));

				try
				{
					var cfg = Inherited.LoadFromXml(cfgProp.PropertyType, name);
					cfgProp.SetValue(svc, cfg);
				}
				catch (Exception ex)
				{
					Log.Warning("Failed to load configuration:");
					Log.Warning(ex.Message);
				}
			}
		}

		/// <summary>
		/// Saves configuration to XML file	for each subsystem
		/// </summary>
		/// <param name="path"></param>
		[UICommand("Save Configuration", 1)]
		new public void SaveConfiguration()
		{
			string ConfigPath = String.Format(@"..\Config\{0}", this.GetType().Namespace);
			Log.Message("Saving configuration...");

			foreach (var svc in GetServiceList())
			{
				string name = System.IO.Path.Combine(ConfigPath, string.Format("Config.{0}.xml", svc.GetType().Name));
				var cfgProp = Inherited.GetConfigProperty(svc.GetType());

				if (cfgProp == null)
				{
					continue;
				}

				Log.Message("Saving : {0}", name);

				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(name));

				try
				{
					Inherited.SaveToXml(cfgProp.GetValue(svc), cfgProp.PropertyType, name);
				}
				catch (Exception ex)
				{
					Log.Error("Failed to save configuration:");
					Log.Error(ex.Message);
				}
			}
		}

		protected override void Draw(Fusion.GameTime gameTime, StereoEye stereoEye)
		{
			base.Draw(gameTime, stereoEye);
			if (DateTime.Now > nextSnapshotTime)
			{
				Bitmap bmp;
				backBufferToBitmap(out bmp);
				bmp.Save(Wheel.Current(), ImageFormat.Png);
				bmp.Dispose();
				nextSnapshotTime = nextSnapshotTime.Add(snapshotPeriod);
				Wheel.Next();
			}
		}

		public void KeyUp(Keys Key, Vector2 MousePosition)
		{
			InputDevice.RemoveVirtuallyPressedKey(Key);
			if ((int)MousePosition.X != -1)
			{
				InputDevice.GlobalMouseOffset = MousePosition;
			}
		}

		public void KeyDown(Keys Key, Vector2 MousePosition)
		{
			InputDevice.AddVirtuallyPressedKey(Key);
			if ((int)MousePosition.X != -1)
			{
				InputDevice.GlobalMouseOffset = MousePosition;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) {
			}
			base.Dispose(disposing);
		}
	}
}
