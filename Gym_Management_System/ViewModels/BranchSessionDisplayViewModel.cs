// 文件路径建议: ViewModels/BranchSessionDisplayViewModel.cs
namespace GymManagementSystem.ViewModels
{
    public class BranchSessionDisplayViewModel
    {
        public string DayOfWeek { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string TrainerName { get; set; } = "";
    }
}
