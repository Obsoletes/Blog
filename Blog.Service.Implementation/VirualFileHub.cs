using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Service.Implementation
{
	public abstract class VirualFileHub  : IVirualFileHub
	{
		public abstract IVirualFileService Image { get; }
		public abstract ICAPTCHAService CAPTCHAService { get; }
	}
}
