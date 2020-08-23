using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Blog.Extension;
using System.Drawing;

namespace Blog.Service
{
	[Inject(Lifetime.Singleton)]
	public interface ICAPTCHAService
	{
		(string Code, Stream Image) GetCAPTCHA(int length, string set, int width, int height, Color backgroundColor, Color fontColor);
	}
}
