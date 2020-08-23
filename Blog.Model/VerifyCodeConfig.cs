using System;

namespace Blog.Model
{
	public class VerifyCodeConfig
	{
		public Option.CAPTCHAStyle? Style { get; set; }
		public int? Width { get; set; }
		public int? Height { get; set; }
	}
}
