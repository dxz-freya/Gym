using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
  public class GymBranch
  {
    [Key]
    public int BranchId { get; set; }

    [Required]
    [StringLength(100)]
    public string BranchName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? ContactNumber { get; set; }

    [Required]
    public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();  // 1..*

    [Required]
    public ICollection<Receptionist> Receptionists { get; set; } = new List<Receptionist>();  // 1..*

    [Required]
    public ICollection<Room> Rooms { get; set; } = new List<Room>();  // 1..*
  }
}
