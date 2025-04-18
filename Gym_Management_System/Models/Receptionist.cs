using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
  public class Receptionist : User
  {
    [Required]
    [StringLength(200)]
    public string Responsibilities { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    public bool IsAvailable { get; set; }

    [Required]
    public int BranchId { get; set; }  // Foreign key to GymBranch

    public GymBranch? GymBranch { get; set; }
  }
}
