using System.Globalization;
namespace GymManagement.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public string ClassName { get; set; } = "";
        public DateTime SessionDate { get; set; }
        public string Status { get; set; } = "";
        
        public string? SessionName { get; set; }  
        public DateTime BookingDate { get; set; } 

        public string? TrainerName { get; set; }
        public string? BranchName { get; set; } 

        public string? RoomName { get; set; } 
        public bool IsCheckedIn { get; set; } 
        //for display purpose in razor view
        public string SessionDateFormatted => SessionDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        public string BookingDateFormatted => BookingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}