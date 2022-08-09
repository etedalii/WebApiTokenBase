using System.ComponentModel.DataAnnotations;

namespace WebApiTokenBaseSample.ViewModel
{
	public class LoginVM
	{
		[Required]
		public string EmailAddress { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
