using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Service.Implementation
{
	public class ImageCAPTCHAService : ICAPTCHAService
	{
		public (string Code, Stream Image) GetCAPTCHA(int length, string set)
		{
			throw new NotImplementedException();
		}
	}
}
