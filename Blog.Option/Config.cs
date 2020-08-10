using System;
using System.IO;

namespace Blog.Config
{
	public class Config
	{
		public static string Key { get; } = "Config";
		public string[] VirualImagePathPart { get; set; } = null!;
		public string CAPTCHACharSet { get; set; } = null!;
		public string VirualImagePath => System.IO.Path.Combine(VirualImagePathPart);
		public string RedirectSuffix { get; set; } = null!;
	}
}
