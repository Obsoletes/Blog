using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Option
{
	public enum CAPTCHAStyle
	{
		Dark, Light, Auto
	}
	public class CAPTCHAConfig
	{
		public static string Key { get; set; } = "CAPTCHA";
		public string CharSet { get; set; } = null!;
		public CAPTCHAStyle Style { get; set; }
		public int MaxWidth { get; set; }
		public int MaxHeight { get; set; }
		public int Length { get; set; }
		public Color DarkBackgroundColor { get; set; }
		public Color DarkFontColor { get; set; }
		public Color LightBackgroundColor { get; set; }
		public Color LightFontColor { get; set; }
	}
}
