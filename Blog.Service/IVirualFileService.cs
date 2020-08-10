using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Blog.Extension;

namespace Blog.Service
{
	[Inject(Lifetime.Singleton)]
	public interface IVirualFileService
	{
		string Get();
		bool IsFileExist(string name);
		bool IsFileRedirectExist(string name);
		FileInfo ReadFile(string name);
		string ReadRedirect(string name);
	}
}
