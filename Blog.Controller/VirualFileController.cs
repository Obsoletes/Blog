using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Drawing;

namespace Blog.Controllers
{
	[Route("Virual")]
	[ApiController]
	public class VirualFileController : ControllerBase
	{
		private Service.IVirualFileHub VirualFileService { get; }
		private ILogger<VirualFileController> Logger { get; }
		private FileExtensionContentTypeProvider FileExtensionContentTypeProvider { get; }
		private Option.CAPTCHAConfig CAPTCHAConfig { get; }
		public VirualFileController(Service.IVirualFileHub virualFileService, ILogger<VirualFileController> logger, IOptions<Option.CAPTCHAConfig> options)
		{
			VirualFileService = virualFileService;
			Logger = logger;
			FileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
			CAPTCHAConfig = options.Value;
		}
		[HttpGet]
		public IActionResult Get()
		{
			return Ok();
		}
		[HttpGet("File/{**name}")]
		[ResponseCache(Duration = 3600)]
		public ActionResult<Stream> GetFile(string name)
		{
			if (VirualFileService.File.IsFileExist(name))
			{
				var info = VirualFileService.File.ReadFile(name);
				Logger.LogInformation("{0} exist at {1}", name, info.FullName);
				if (!FileExtensionContentTypeProvider.TryGetContentType(info.Name, out string contentType))
				{
					contentType = "application/octet-stream";
				}
				Logger.LogInformation("ContentType:", contentType);
				return File(info.OpenRead(), contentType);
			}
			else if (VirualFileService.File.IsFileRedirectExist(name))
			{
				string redirect = VirualFileService.File.ReadRedirect(name);
				Logger.LogInformation("{0} redirect exist.redirect to {1}", name, redirect);
				if (redirect != name)
					return RedirectPermanent(redirect);
			}
			Logger.LogInformation("not found");
			return NotFound();
		}
		[HttpGet("VerifyCode")]
		[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
		public ActionResult<Stream> GetVerifyCode([FromQuery]Model.VerifyCodeConfig config)
		{
			var style = config.Style.GetValueOrDefault(CAPTCHAConfig.Style);
			if (style == Option.CAPTCHAStyle.Auto)
			{
				style = CAPTCHAConfig.Style;
			}
			int width = Math.Min(CAPTCHAConfig.MaxWidth, config.Width.GetValueOrDefault(int.MaxValue));
			int height = Math.Min(CAPTCHAConfig.MaxHeight, config.Height.GetValueOrDefault(int.MaxValue));
			Color bgcolor = style == Option.CAPTCHAStyle.Light ? CAPTCHAConfig.LightBackgroundColor : CAPTCHAConfig.DarkBackgroundColor;
			Color fgcolor = style == Option.CAPTCHAStyle.Light ? CAPTCHAConfig.LightFontColor : CAPTCHAConfig.DarkFontColor;

			var (Code, Image) = VirualFileService.CAPTCHAService.GetCAPTCHA(CAPTCHAConfig.Length, CAPTCHAConfig.CharSet,
			width, height, bgcolor, fgcolor);
			Response.Headers.Add("X-Code", Code);
			return File(Image, "image/png");
		}
	}
}
