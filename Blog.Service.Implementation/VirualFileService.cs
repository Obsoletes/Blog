using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blog;
using Microsoft.Extensions.Options;

namespace Blog.Service.Implementation
{
	public abstract class VirualFileService : IVirualFileService
	{
		[ReadValueFrom(typeof(IOptions<Option.Config>), "Value")]
		public abstract Option.Config Config { get; }
		bool IVirualFileService.IsFileExist(string name)
		{
			return File.Exists(Path.Combine(Config.VirualImagePath, name));
		}

		bool IVirualFileService.IsFileRedirectExist(string name)
		{
			return File.Exists(Path.Combine(Config.VirualImagePath, $"{name}.{Config.RedirectSuffix}"));
		}

		FileInfo IVirualFileService.ReadFile(string name)
		{
			return new FileInfo(Path.Combine(Config.VirualImagePath, name));
		}

		string IVirualFileService.ReadRedirect(string name)
		{
			return File.ReadAllText(Path.Combine(Config.VirualImagePath, $"{name}.{Config.RedirectSuffix}"));
		}
	}
}
