namespace GymManagementSystem.ViewModels
{
  public class GymBranchViewModel
  {
    public required int BranchId { get; set; }
    public required string BranchName { get; set; }
    public required string Address { get; set; }
    public required string ContactNumber { get; set; }
    public required int TrainerCount { get; set; }
    public required int ReceptionistCount { get; set; }
    public required int RoomCount { get; set; }

    public required string ImageUrl { get; set; }

  }

}
