using GymManagement.Models; // 别忘了加这个引用

public class SessionViewModel
{
  public required string ClassName { get; set; }
  public required DateTime SessionDate { get; set; }
  public required string RoomName { get; set; }
  public required int TotalBookings { get; set; }

  public required SessionCategory Category { get; set; } // ✅ 新增字段
}
