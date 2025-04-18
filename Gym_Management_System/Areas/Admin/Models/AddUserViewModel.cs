using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Attributes;

namespace GymManagement.Areas.Admin.Models
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(255)]
        [RegularExpression("^[a-zA-Z\\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter your username.")]
        [StringLength(255)]
        [Remote("CheckUsername", "Validation", "Admin", ErrorMessage = "This username is already taken.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Remote("CheckEmail", "Validation", "Admin", ErrorMessage = "This email is already registered.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please enter a password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{6,}$",
            ErrorMessage = "Password must be at least 6 characters and include upper/lowercase letters, a number, and a special character.")]
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

        [Required(ErrorMessage = "Please select a role.")]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
