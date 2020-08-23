using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Option
{
	public interface IJWTConfig
	{
		public string SigningKey { get; set; }
		public string Issuer { get; set; }
	}
}
