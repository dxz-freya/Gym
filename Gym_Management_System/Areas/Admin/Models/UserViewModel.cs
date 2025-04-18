using Microsoft.AspNetCore.Identity;
using GymManagement.Models;
using GymManagement.ViewModels;


namespace GymManagement.Areas.Admin.Models
{
    public class UserViewModel
    {
        public IEnumerable<User> Users { get; set; } = null!;
        public IEnumerable<IdentityRole> Roles { get; set; } = null!;
        public PagingInfo PagingInfo { get; set; } = new(); // ✅ 分页支持
    }
}
