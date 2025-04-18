using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Attributes;

namespace GymManagement.Models
{
  public class EditProfileViewModel
  {
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(255)]
    [RegularExpression("^[a-zA-Z\\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Remote(action: "CheckEmailEdit", controller: "Validation", ErrorMessage = "This email is already registered.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Date of birth is required.")]
    [MinAge(18, ErrorMessage = "You must be at least 18 years old.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DOB { get; set; }

    public string? ProfileImageUrl { get; set; }

    public IList<string> RoleNames { get; set; } = new List<string>();

    public IFormFile? ProfileImageFile { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[\\W_]).+$",
        ErrorMessage = "Password must include upper, lower, number, and special character.")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }
  }
}