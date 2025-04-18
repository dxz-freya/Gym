using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Helpers
{
    public static class ToastHelper
    {
        public static void ShowToast(Controller controller, string message, string type = "info", string title = null)
        {
            controller.TempData["ToastMessage"] = message;
            controller.TempData["ToastClass"] = type; // "success", "danger", "warning", "info"
            controller.TempData["ToastTitle"] = title ?? GetDefaultTitle(type);
        }

        private static string GetDefaultTitle(string type)
        {
            return type switch
            {
                "success" => "Success!",
                "danger" => "Oops!",
                "warning" => "Alert!",
                "info" => "Note:",
                _ => "Message"
            };
        }
    }
}
