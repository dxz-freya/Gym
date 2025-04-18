using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Areas.Admin.Models
{
    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter full name.")]
        [StringLength(255)]
        [RegularExpression("^[a-zA-Z\\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter contact email.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Remote(action: "CheckEmailEdit", controller: "Validation", areaName: "Admin", AdditionalFields = "Id", ErrorMessage = "This email is already registered.")]
        [Display(Name = "Contact")]
        public string Contact { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
