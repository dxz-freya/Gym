using Microsoft.AspNetCore.Mvc.Rendering;
using GymManagement.Models;

namespace GymManagement.ViewModels
{
    public class TimetableBookingViewModel
    {
        public List<Session> Sessions { get; set; } = new();
        public List<SelectListItem> CustomerList { get; set; } = new();
        public string? SelectedCustomerId { get; set; }
    }
}
