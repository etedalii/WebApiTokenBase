using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiTokenBaseSample.Model;

namespace WebApiTokenBaseSample.DbContexts
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option)
		{

		}

		public DbSet<RefreshToken> RefreshTokens { get; set; }
	}
}