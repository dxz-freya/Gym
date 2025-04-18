using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
  public class Admin : User
  {
    [Required]
    public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();

    [Required]
    public ICollection<GymClass> GymClasses { get; set; } = new List<GymClass>();

    [Required]
    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    [Required]
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
  }
}
