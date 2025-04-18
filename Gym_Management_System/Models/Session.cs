using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models
{

  public enum SessionCategory
  {
    Yoga,
    Pilates,
    StrengthTraining,
    Cardio,
    HIIT,
    DanceFitness,
    Cycling
  }
  public class Session
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SessionId { get; set; }


    [Required]
    public string SessionName { get; set; } // 修改了数据类型

    [Required]
    public DateTime SessionDateTime { get; set; }

    [Required]
    public int Capacity { get; set; }

    [Required]
    public SessionCategory Category { get; set; }

    // 🔸 GymClass FK
    [Required]
    public int GymClassId { get; set; }

    [ForeignKey("GymClassId")]
    public GymClass GymClass { get; set; } = null!;

    // 🔸 Room FK
    [Required]
    public int RoomId { get; set; }

    [ForeignKey("RoomId")]
    public Room Room { get; set; } = null!;

    // 🔸 Trainer FK (string type from IdentityUser)
    [Required]
    public string TrainerId { get; set; } = string.Empty;

    [ForeignKey("TrainerId")]
    public Trainer Trainer { get; set; } = null!;

    // 🔸 Bookings
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    // 🔸 Optional Receptionist FK
    public string? ReceptionistId { get; set; }

    [ForeignKey("ReceptionistId")]
    public Receptionist? Receptionist { get; set; }
  }
}

