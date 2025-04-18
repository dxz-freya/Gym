using System.ComponentModel.DataAnnotations;

namespace GymManagement.ViewModels
{
  public class LoginViewModel
  {
    [Required(ErrorMessage = "Please enter your username.")]
    [StringLength(255)]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; } = string.Empty;
  }
}
