using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApiTokenBaseSample.DbContexts;
using WebApiTokenBaseSample.Model;
using WebApiTokenBaseSample.ViewModel;

namespace WebApiTokenBaseSample.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly TokenValidationParameters _tokenValidationParameters;
		
		public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration configuration
			,TokenValidationParameters tokenValidationParameters)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_context = context;
			_configuration = configuration;
			_tokenValidationParameters = tokenValidationParameters;	
		}

		[HttpPost("create-user")]
		public async Task<IActionResult> Register([FromBody] RegisterVM registerVM)
		{
			if (ModelState.IsValid)
			{
				var userExists= await _userManager.FindByEmailAsync(registerVM.EmailAddress);
				if(userExists != null)
				{
					return BadRequest($"User {registerVM.EmailAddress} already exists");
				}

				ApplicationUser applicationUser = new ApplicationUser()
				{
					FirstName = registerVM.FirstName,
					LastName = registerVM.LastName,
					Email = registerVM.EmailAddress,
					UserName = registerVM.UserName,
					SecurityStamp = Guid.NewGuid().ToString()
				};

				var result  = await _userManager.CreateAsync(applicationUser, registerVM.Password);
				if (result.Succeeded)
				{
					switch (registerVM.Role)
					{
						case UserRoles.Manager:
							await _userManager.AddToRoleAsync(applicationUser, UserRoles.Manager);
							break;
						case UserRoles.Client:
							await _userManager.AddToRoleAsync(applicationUser, UserRoles.Client);
							break;
						default:
							break;
					}

					return Ok("User Created");
				}
				else
					return BadRequest("User can not be create");
			}
			else
				return BadRequest("Please provide all required fields");
		}

		[AllowAnonymous]
		[HttpPost("user-signIn")]
		public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("Please provide all required fields");
			}

			var userExists = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
			if (userExists != null && await _userManager.CheckPasswordAsync(userExists, loginVM.Password))
			{
				var tokenValue = await GenerateJWTTokenAsync(userExists);
				return Ok(tokenValue);
			}
			return Unauthorized();
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> refreshToken([FromBody] TokenRequestVM tokenRequestVM)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest("Please provide all required fields");
			}

			var result = await VerifyAndGenerateToken(tokenRequestVM);
			return Ok(result);
		}

		private async Task<AuthResultVM> VerifyAndGenerateToken(TokenRequestVM tokenRequestVM)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();
			var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequestVM.RefreshToken);
			var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);

			try
			{
				var tokenCheckResult = jwtTokenHandler.ValidateToken(tokenRequestVM.Token, _tokenValidationParameters, out var validatedToken);

				return await GenerateJWTTokenAsync(dbUser, storedToken);
			}
			catch (SecurityTokenExpiredException)
			{
				if (storedToken.DateExpire >= DateTime.UtcNow)
				{
					return await GenerateJWTTokenAsync(dbUser, storedToken);
				}
				else
				{
					return await GenerateJWTTokenAsync(dbUser, null);
				}
			}
		}

		private async Task<AuthResultVM> GenerateJWTTokenAsync(ApplicationUser user, RefreshToken rToken = null)
		{
			var authClaims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			//Add User Role Claims
			var userRoles = await _userManager.GetRolesAsync(user);
			foreach (var item in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, item));
			}


			var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:Issuer"],
				audience: _configuration["JWT:Audience"],
				expires: DateTime.UtcNow.AddMinutes(1),
				claims: authClaims,
				signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

			var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
			
			if(rToken != null)
			{
				var rTokenResponse = new AuthResultVM()
				{
					Token = jwtToken,
					RefreshToken = rToken.Token,
					ExpiresAt = token.ValidTo
				};

				return rTokenResponse;
			}

			var refreshToken = new RefreshToken()
			{
				JwtId = token.Id,
				IsRevoked = false,
				UserId = user.Id,
				DateAdded = DateTime.UtcNow,
				DateExpire = DateTime.UtcNow.AddMonths(3),
				Token = $"{Guid.NewGuid()}-{Guid.NewGuid()}"
			};
			await _context.RefreshTokens.AddAsync(refreshToken);
			await _context.SaveChangesAsync();

			var response = new AuthResultVM()
			{
				Token = jwtToken,
				RefreshToken = refreshToken.Token,
				ExpiresAt = token.ValidTo
			};

			return response;
		}
	}
}