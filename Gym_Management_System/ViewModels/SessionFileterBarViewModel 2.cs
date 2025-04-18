namespace GymManagement.ViewModels
{
    public class SessionFilterBarViewModel
    {
        public List<string> Branches { get; set; } = new();
        public string SelectedBranch { get; set; } = "";
        public string SearchTerm { get; set; } = "";
    }
}