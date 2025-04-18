using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Models
{
  public class User : IdentityUser
  {
    // Required full name of the user
    [Required]
    public string Name { get; set; } = "";

    // Automatically set the join date to now
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    // Optional date of birth
    [DataType(DataType.Date)]
    public DateTime? DOB { get; set; }

    // Stores the uploaded profile image filename (e.g., "user123.jpg")
    public string? ProfileImageName { get; set; } = "default.png";

    // Not mapped to DB: used for displaying roles in views
    [NotMapped]
    public IList<string>? RoleNames { get; set; }
  }
}
