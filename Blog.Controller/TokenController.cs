using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TokenController: ControllerBase
	{
		private Service.ITokenService TokenService{ get; }
		public TokenController(Service.ITokenService tokenService)
		{
			TokenService = tokenService;
		}
		[HttpPost]
		public ActionResult Create()
		{
			return Ok();
		}
	}
}
