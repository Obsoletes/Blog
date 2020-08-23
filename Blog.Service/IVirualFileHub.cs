using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blog.Extension;

namespace Blog.Service
{
	[Inject(Lifetime.Singleton)]
	public interface IVirualFileHub
	{
		IVirualFileService File { get; }
		ICAPTCHAService CAPTCHAService { get; }
	}
}
