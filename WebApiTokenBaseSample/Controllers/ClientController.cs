using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiTokenBaseSample.Controllers
{
	[Authorize(Roles = UserRoles.Client)]
	[Route("api/[controller]")]
	[ApiController]
	public class ClientController : ControllerBase
	{
		[HttpGet]
		public IActionResult Get()
		{
			return Ok("Welcome to Client Controller");
		}
	}
}
