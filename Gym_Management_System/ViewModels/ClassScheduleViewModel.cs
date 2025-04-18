using System;
using System.Globalization; 
namespace GymManagement.ViewModels
{
  public class ClassScheduleViewModel
  {
      public int SessionId { get; set; }
      public string ClassName { get; set; } = "";
      public string TrainerName { get; set; } = "";
      public string RoomName { get; set; } = "";
      public string BranchName { get; set; } = ""; 
      public DateTime? SessionDateTime { get; set; }
      public int Capacity { get; set; }
      public int BookedCount { get; set; }
      public bool IsBookedByUser { get; set; }

      public string SessionDateFormatted =>
      SessionDateTime?.ToString("HH:mm / MMM dd, yyyy", CultureInfo.InvariantCulture) ?? "";
  }
}