using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiTokenBaseSample.Controllers
{
	[Authorize(Roles = UserRoles.Manager)]
	[Route("api/[controller]")]
	[ApiController]
	public class ManagerController : ControllerBase
	{
		[HttpGet]
		public IActionResult Get()
		{
			return Ok("Welcome to Manager Controller");
		}
	}
}
