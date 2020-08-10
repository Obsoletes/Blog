using Microsoft.AspNetCore.Mvc;
using System;

namespace Blog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class IndexController : ControllerBase
	{
		Service.ITokenService Service{ get; }
		public IndexController(Service.ITokenService service)
		{
			Service = service;
		}
		[HttpGet]
		public string Index()
		{
			return Service.GenerateRefreshToken();
		}
	}
}
