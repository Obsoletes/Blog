using System;

namespace Blog.Model
{
	public class Config
	{
		public string[] VirualImagePathPart { get; set; } = null!;
		public string CAPTCHACharSet { get; set; } = null!;
		public string VirualImagePath => System.IO.Path.Combine(VirualImagePathPart);
	}
}
