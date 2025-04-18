using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymManagement.Models;
using System.ComponentModel.DataAnnotations;


namespace GymManagement.Areas.Admin.Models
{
    public class CreateSessionViewModel
    {

        [Required(ErrorMessage = "Session name is required")]
        [Display(Name = "Session Name")]
        public string SessionName { get; set; } = string.Empty;

        [Display(Name = "Session Time")]
        [DataType(DataType.DateTime)]
        public DateTime SessionDateTime { get; set; }
        public int GymClassId { get; set; }
        public string TrainerId { get; set; }
        public int RoomId { get; set; }
        public int Capacity { get; set; }

        public string BranchName { get; set; } = string.Empty;

        public SessionCategory Category { get; set; }  // ✅ 新增
        public List<SelectListItem> CategoryList { get; set; } = new();  // ✅ 新增

        public List<SelectListItem> GymClassList { get; set; } = new();
        public List<SelectListItem> TrainerList { get; set; } = new();
        public List<SelectListItem> RoomList { get; set; } = new();
    }
}
