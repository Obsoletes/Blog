using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Blog.Service.Implementation
{
	public abstract class ImageCAPTCHAService : ICAPTCHAService
	{
		public abstract IRandomGenerator Random { get; }
		public abstract Option.CAPTCHAConfig CAPTCHAConfig { get; }
		public (string Code, Stream Image) GetCAPTCHA(int length, string set, int width, int height, Color backgroundColor, Color fontColor)
		{
			string code = Random.GenerateString(length, set);
			using Bitmap bitmap = new Bitmap(width, height);
			using Graphics graph = Graphics.FromImage(bitmap);

			Random rand = new Random();
			graph.Clear(backgroundColor);
			DrawCaptchaCode(graph, code, fontColor, width, height);


			DrawDisorderLine(graph, backgroundColor, width, height);
			AdjustRippleEffect(bitmap);

			MemoryStream ms = new MemoryStream();

			bitmap.Save(ms, ImageFormat.Png);
			ms.Position = 0;
			return (code.ToUpper(), ms);
		}
		void DrawCaptchaCode(Graphics graph, string code, Color color, int width, int height)
		{
			SolidBrush fontBrush = new SolidBrush(color);
			int fontSize = Convert.ToInt32(width / code.Length);
			Font font = new Font(FontFamily.GenericSerif, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
			for (int i = 0; i < code.Length; i++)
			{

				int shiftPx = fontSize / 6;
				float x = i * fontSize + Random.GenerateInterger(-shiftPx, shiftPx) + Random.GenerateInterger(-shiftPx, shiftPx);
				int maxY = height - fontSize;
				if (maxY < 0) maxY = 0;
				float y = Random.GenerateInterger(0, maxY);

				graph.DrawString(code[i].ToString(), font, fontBrush, x, y);
			}
		}
		void DrawDisorderLine(Graphics graph, Color color, int width, int height)
		{
			Pen linePen = new Pen(new SolidBrush(color), 3);
			for (int i = 0; i < Random.GenerateInterger(3, 5); i++)
			{
				Point startPoint = new Point(Random.GenerateInterger(0, width), Random.GenerateInterger(0, height));
				Point endPoint = new Point(Random.GenerateInterger(0, width), Random.GenerateInterger(0, height));
				graph.DrawLine(linePen, startPoint, endPoint);
			}
		}
		unsafe void AdjustRippleEffect(Bitmap bitmap)
		{
			short nWave = 6;
			int nWidth = bitmap.Width;
			int nHeight = bitmap.Height;

			Point[,] pt = new Point[nWidth, nHeight];

			for (int x = 0; x < nWidth; ++x)
			{
				for (int y = 0; y < nHeight; ++y)
				{
					var xo = nWave * Math.Sin(2.0 * 3.1415 * y / 128.0);
					var yo = nWave * Math.Cos(2.0 * 3.1415 * x / 128.0);

					var newX = x + xo;
					var newY = y + yo;

					if (newX > 0 && newX < nWidth)
					{
						pt[x, y].X = (int)newX;
					}
					else
					{
						pt[x, y].X = 0;
					}


					if (newY > 0 && newY < nHeight)
					{
						pt[x, y].Y = (int)newY;
					}
					else
					{
						pt[x, y].Y = 0;
					}
				}
			}

			Bitmap bSrc = (Bitmap)bitmap.Clone();

			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int scanline = bitmapData.Stride;

			IntPtr scan0 = bitmapData.Scan0;
			IntPtr srcScan0 = bmSrc.Scan0;

			unsafe
			{
				byte* p = (byte*)(void*)scan0;
				byte* pSrc = (byte*)(void*)srcScan0;

				int nOffset = bitmapData.Stride - bitmap.Width * 3;

				for (int y = 0; y < nHeight; ++y)
				{
					for (int x = 0; x < nWidth; ++x)
					{
						var xOffset = pt[x, y].X;
						var yOffset = pt[x, y].Y;

						if (yOffset >= 0 && yOffset < nHeight && xOffset >= 0 && xOffset < nWidth)
						{
							if (pSrc != null)
							{
								p[0] = pSrc[yOffset * scanline + xOffset * 3];
								p[1] = pSrc[yOffset * scanline + xOffset * 3 + 1];
								p[2] = pSrc[yOffset * scanline + xOffset * 3 + 2];
							}
						}

						p += 3;
					}
					p += nOffset;
				}
			}

			bitmap.UnlockBits(bitmapData);
			bSrc.UnlockBits(bmSrc);
			bSrc.Dispose();
		}
	}
}

