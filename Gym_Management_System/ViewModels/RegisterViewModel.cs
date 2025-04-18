using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Attributes;



namespace GymManagement.ViewModels
{
  public class RegisterViewModel
  {
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(255)]
    [RegularExpression("^[a-zA-Z\\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Please enter your username.")]
    [StringLength(255)]
    [Remote("CheckUsername", "Validation", ErrorMessage = "This username is already taken.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Remote("CheckEmail", "Validation", ErrorMessage = "This email is already registered.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required.")]
    [MinAge(18, ErrorMessage = "You must be at least 18 years old.")]
    [DataType(DataType.Date)]
    public DateTime? DOB { get; set; }

  }
}
