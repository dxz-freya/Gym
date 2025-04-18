using System.ComponentModel.DataAnnotations;
using GymManagement.Models;

namespace GymManagement.Models
{
  public class Trainer : User
  {
    [Required]
    public string Specialization { get; set; } = "";

    [Required]
    [Display(Name = "Experience Start Date")]
    public DateTime ExperienceStarted { get; set; }

    public int BranchId { get; set; }
    public GymBranch? GymBranch { get; set; }
    public ICollection<GymClass> GymClasses { get; set; } = new List<GymClass>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public string Bio { get; set; } = string.Empty; // 个人简介
  }
}
