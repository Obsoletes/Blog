using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace Blog.Controllers
{
	[Route("Virual")]
	[ApiController]
	public class VirualFileController : ControllerBase
	{
		private Service.IVirualFileHub VirualFileService { get; }
		private ILogger<VirualFileController> Logger { get; }
		private FileExtensionContentTypeProvider FileExtensionContentTypeProvider { get; }
		public VirualFileController(Service.IVirualFileHub virualFileService, ILogger<VirualFileController> logger)
		{
			VirualFileService = virualFileService;
			Logger = logger;
			FileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
		}
		[HttpGet]
		public IActionResult Get()
		{
			return Ok(VirualFileService.Image.Get());
		}
		[HttpGet("File/{**name}")]
		[ResponseCache(Duration = 3600)]
		public ActionResult<Stream> GetFile(string name)
		{
			Logger.BeginScope(name);
			Logger.LogInformation(VirualFileService.GetType().FullName);
			Logger.LogInformation(VirualFileService.Image.GetType().FullName);
			if (VirualFileService.Image.IsFileExist(name))
			{
				var info = VirualFileService.Image.ReadFile(name);
				Logger.LogInformation("{0} exist at {1}", name, info.FullName);
				if (!FileExtensionContentTypeProvider.TryGetContentType(info.Name, out string contentType))
				{
					contentType = "application/octet-stream";
				}
				Logger.LogInformation("ContentType:", contentType);
				return File(info.OpenRead(), contentType);
			}
			else if (VirualFileService.Image.IsFileRedirectExist(name))
			{

				string redirect = VirualFileService.Image.ReadRedirect(name);
				Logger.LogInformation("{0} redirect exist.redirect to {1}", name, redirect);
				if (redirect != name)
					return RedirectPermanent(redirect);
			}
			Logger.LogInformation("not found");
			return NotFound();
		}
	}
}
