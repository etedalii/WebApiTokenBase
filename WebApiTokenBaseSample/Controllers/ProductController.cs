using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiTokenBaseSample.Model;

namespace WebApiTokenBaseSample.Controllers
{
	[ApiController]
	[Route("[controller]")]
	[Authorize]
	public class ProductController : ControllerBase
	{
		[HttpGet(Name = "GetAllProducts")]
		public IEnumerable<Product> Get()
		{
			return Enumerable.Range(1, 5).Select(index => new Product
			{
				Id = Random.Shared.Next(1, 8000),//id++,
				Name = Guid.NewGuid().ToString(),
				Code = Guid.NewGuid().ToString()
			})
			.ToArray();
		}
	}
}
