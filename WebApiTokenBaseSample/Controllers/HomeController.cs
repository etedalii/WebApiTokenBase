using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiTokenBaseSample.Controllers
{
	[Authorize(Roles = UserRoles.Client + "," + UserRoles.Manager)]
	[Route("api/[controller]")]
	[ApiController]
	public class HomeController : ControllerBase
	{
		[HttpGet("client")]
		public IActionResult GetClient()
		{
			return Ok("Welcome to Client Home-Controller");
		}

		[HttpGet("manager")]
		public IActionResult GetManager()
		{
			return Ok("Welcome to Manager Home-Controller");
		}
	}
}
