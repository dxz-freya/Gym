namespace GymManagement.ViewModels
{
    public class GymClassViewModel
    {
        public int GymClassId { get; set; }

        public required string ClassName { get; set; }

        public required DateTime AvailableTime { get; set; }

        public required int Duration { get; set; }

        public required string Description { get; set; }

        // ✅ 显示图用：从 DB 读取的文件名
        public string? ImageName { get; set; } = "class-default.jpg";

        // ✅ 表单上传图用：用于接收上传文件
        public IFormFile? ImageFile { get; set; }
    }
}
