using System;

namespace Blog.Option
{
	public class Config
	{
		public static string Key { get; set; } = "Config";
		public string[] VirualImagePathPart { get; set; } = null!;
		public string VirualImagePath => System.IO.Path.Combine(VirualImagePathPart);
		public string RedirectSuffix { get; set; } = null!;
	}
}
