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
	class PlotDataList //: GameService
	{
		public static SpriteFont font1;
		public static SpriteFont font2;

		public static Texture2D texplot
		{
			get;
			set;
		}

		float a = 0;

		public List<PlotData> dataPlot;
		float[] arrayData;
		public float maxValue;
		public float scale;

		static float graphSize = 170;

		public PlotDataList(string path)
		{
			dataPlot = new List<PlotData>();
			setArticleData(path);
		}

		public void setArticleData(string path)
		{
			string[] plotData = File.ReadAllLines(path);
			arrayData = plotData.Select(float.Parse).ToArray<float>();
			maxValue = arrayData.Max<float>();
			scale = maxValue / graphSize;
		}

		
		public void AddDot(int count)
		{
			PlotData dot;
			float coord;
			coord = arrayData[count];
			dot = new PlotData(10 + count * 0.5f, 100, coord);
			dataPlot.Add(dot);
		}

		public void Draw(SpriteBatch sb, int w, int num, int h, float scale, float max)
		{
			//font1.DrawString(sb, "Number of customers changed bank", 10, w - (graphSize + 10), Color.Black);
			//font2.DrawString(sb, max.ToString(), 2, w - (graphSize), Color.Black);
			//font2.DrawString(sb, "66", 2, w - (graphSize / 2), Color.Black);
			//font2.DrawString(sb, "0", 2, w - 5, Color.Black);



			//font1.DrawString(sb, "Lost funds", 10, w - 2 * (graphSize + 15), Color.Black);
			//font2.DrawString(sb, "84", 6, w - (2 * graphSize + 20), Color.Black);
			//font2.DrawString(sb, "42", 6, w - 2 * ((graphSize + 25 ) / 2), Color.Black);
			//font2.DrawString(sb, "0", 8, w - 115, Color.Black);


			//font1.DrawString(sb, "General probalistic stability coefficient", 10, w - 3 * (graphSize + 15), Color.Black);
			//font2.DrawString(sb, "7.85", 2, w - (3 * graphSize + 20), Color.Black);
			//font2.DrawString(sb, "3.5", 4, w - 275, Color.Black);
			//font2.DrawString(sb, "0", 8, w - 225, Color.Black);

			//font1.DrawString(sb, "Average degree", 10, w - 440, Color.Black);
			//font2.DrawString(sb, "0", 8, w - 335, Color.Black);
			//font2.DrawString(sb, "0.98", 2, w - 385, Color.Black);
			//font2.DrawString(sb, "1.96", 2, w - 425, Color.Black);

			//font1.DrawString(sb, "Bankrupts count", 10, w - 550, Color.Black);
			//font2.DrawString(sb, "0", 8, w - 455, Color.Black);
			//font2.DrawString(sb, "20", 6, w - 495, Color.Black);
			//font2.DrawString(sb, "40", 6, w - 540, Color.Black);


			//sb.DrawSprite(texplot, 180, w - 2 - 1 / 2, 350, 1, a, Color.Black);
			//font1.DrawString(sb, "Time, iterations", 380, w - 20, Color.Black);
			//font2.DrawString(sb, "599", 315, w - 10, Color.Black);
			//font2.DrawString(sb, "300", 165, w - 10, Color.Black);
		}

	}

	class PlotData
	{


		public static Texture2D texplot
		{
			get;
			set;
		}

		public static SpriteFont font1;
		public static SpriteFont font2;
		static float graphSize = 170;
		
		public PlotData(float plotX, float plotY, float value)
		{
			x = plotX;
			y = plotY;
			val = value;

			if (val > 1000)
				height = 0.001f * val;
			else
			height = 0.1f * val;


			if (height < 1)
				height = 1;

			if (height > 100)
				height = 100;
		}


				int shift = 200;
				public float x;
				public float y;
				public float a = 0;
				public float val;
				public float height;
				//public float height1;


				public void Draw(SpriteBatch sb, int w, int num, int h, float scale, float[] arr)
				{
					w = w - 40;

					if (num == 1)
					{
						sb.DrawSprite(texplot, x + shift, w - 5 - val / scale / 2, 3, (1 + val / scale), a, Color.MediumSeaGreen);

						font1.DrawString(sb, "Number of customers changed bank", shift + 10, w - (graphSize + 10), Color.Black);
						font2.DrawString(sb, arr[0].ToString(), shift - 15, w - (graphSize), Color.Black);
						font2.DrawString(sb, (arr[0] / 2).ToString(), shift - 15, w - (graphSize / 2), Color.Black);
						font2.DrawString(sb, "0", shift - 15, w - 4, Color.Black);

					}
					else if (num == 2)
					{

						sb.DrawSprite(texplot, x + shift, w - (graphSize + 25) - val / scale / 2, 3, (1 + val / scale), a, Color.MediumBlue);
						font1.DrawString(sb, "Lost funds", shift + 10, w - 2 * (graphSize + 18), Color.Black);
						font2.DrawString(sb, arr[1].ToString("0000"), shift - 15, w - (graphSize * 2 + 25), Color.Black);
						font2.DrawString(sb, (arr[1] / 2).ToString("000"), shift - 15, w - ((graphSize * 1.5f + 25)), Color.Black);
						font2.DrawString(sb, "0", shift - 15, w - (graphSize + 25), Color.Black);

					}

					else if (num == 3)
					{

						sb.DrawSprite(texplot, x + shift, w - 2 * (graphSize + 25) - val / scale / 2, 3, (1 + val / scale), a, Color.Lavender);
						font1.DrawString(sb, "General probalistic stability coefficient", shift + 10, w - 3 * (graphSize + 20), Color.Black);
						font2.DrawString(sb, arr[2].ToString(".00"), shift - 15, w - (graphSize * 3 + 50), Color.Black);
						font2.DrawString(sb, (arr[2] / 2).ToString(".00"), shift - 15, w - ((graphSize * 2.5f + 50)), Color.Black);
						font2.DrawString(sb, "0", shift - 15, w - 2 * (graphSize + 25), Color.Black);


					}

					else if (num == 4)
					{

						sb.DrawSprite(texplot, x + shift, w - 3 * (graphSize + 25) - val / scale / 2, 3, (1 + val / scale), a, Color.MediumAquamarine);
						font1.DrawString(sb, "Average degree", shift + 10, w - 4 * (graphSize + 21), Color.Black);
						font2.DrawString(sb, arr[3].ToString(), shift - 15, w - (graphSize * 4 + 75), Color.Black);
						font2.DrawString(sb, (arr[3] / 2).ToString(), shift - 15, w - ((graphSize * 3.5f + 75)), Color.Black);
						font2.DrawString(sb, "0", shift - 15, w - 3 * (graphSize + 25), Color.Black);
						


					}

					else if (num == 5)
					{

						sb.DrawSprite(texplot, x + shift, w - 4 * (graphSize + 25) - val / scale / 2, 3, (1 + val / scale), a, Color.MediumOrchid);
						font1.DrawString(sb, "Bankrupts count", shift + 10, w - 5 * (graphSize + 22), Color.Black);
						font2.DrawString(sb, arr[4].ToString(), shift - 15, w - (graphSize * 5 + 100), Color.Black);
						font2.DrawString(sb, (arr[4] / 2).ToString(), shift - 15, w - ((graphSize * 4.5f + 100)), Color.Black);
						font2.DrawString(sb, "0", shift - 15, w - (4 * (graphSize + 25)), Color.Black);
						
					}


					//if (difference1 > 6)
					//{

















					sb.DrawSprite(texplot, shift + 180, w - 2 - 1 / 2, 350, 1, a, Color.Black);
					font1.DrawString(sb, "Time, day", shift + 380, w - 20, Color.Black);
					font2.DrawString(sb, "599", shift +  300, w - 10, Color.Black);
					font2.DrawString(sb, "300", shift + 150, w - 10, Color.Black);



					//font1.DrawString(sb, string.Format("{0}", y), 64, 256 + h1 + h2 * 4, Color.White);
					//}

				}


	}

}
