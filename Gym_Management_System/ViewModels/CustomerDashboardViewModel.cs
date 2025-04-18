using GymManagement.Models;
namespace GymManagement.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public required string Name { get; set; }
        public required string MembershipType { get; set; }
        public required DateTime SubscriptionDate { get; set; }
        public required List<PaymentViewModel> Payments { get; set; }
        public DateTime? MembershipExpiry { get; set; }
        public required string MembershipStatus { get; set; }
        public Dictionary<DayOfWeek, int> WeeklyActivity { get; set; } = new();

        public required List<BookingViewModel> Bookings { get; set; }
        public required List<BookingViewModel> UpcomingBookings { get; set;}
    }
}