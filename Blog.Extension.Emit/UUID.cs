using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Extension.Emit
{
	static class UUID
	{
		public static string GenerateRandomString()
		{
			return Guid.NewGuid().ToString();
		}
	}
}
