using GymManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.ViewModels
{
    public class ManageBookingsViewModel
    {
        public List<Booking> AllBookings { get; set; } = new();
        public CreateBookingInputModel NewBooking { get; set; } = new();
        public List<SelectListItem> CustomerList { get; set; } = new();
        public List<SelectListItem> SessionList { get; set; } = new();
    }

    public class CreateBookingInputModel
    {
        public string CustomerId { get; set; } = "";
        public int SessionId { get; set; }
    }
}
