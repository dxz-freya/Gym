using GymManagement.Models;
using System.Collections.Generic;

namespace GymManagementSystem.ViewModels
{
    public class GymBranchDetailsViewModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = "";
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? ImageUrl { get; set; }

        public List<TrainerWithClassesViewModel> Trainers { get; set; } = new();
        public List<BranchSessionDisplayViewModel> Sessions { get; set; } = new();  // 🔸 添加



        public List<Room> Rooms { get; set; } = new(); // ← 添加这一行


    }

    public class TrainerWithClassesViewModel
    {
        public string Name { get; set; } = "";
        public string? ImageUrl { get; set; }
        public List<string> ClassNames { get; set; } = new();
    }


}
