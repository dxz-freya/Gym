using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models
{
  public class GymClass
  {
    [Key]
    public int GymClassId { get; set; }

    [Required]
    [StringLength(100)]
    public string ClassName { get; set; } = string.Empty;

    [Required]
    public DateTime AvailableTime { get; set; }

    [Required]
    [Range(1, 300, ErrorMessage = "Duration should be between 1 and 300 minutes.")]
    public int Duration { get; set; } // in minutes

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string TrainerId { get; set; } = string.Empty;  // Trainer inherits from IdentityUser

    [ForeignKey("TrainerId")]
    public Trainer Trainer { get; set; } = null!;

    public string? ImageName { get; set; } = "class-default.jpg";


    [Required]
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
  }
}
