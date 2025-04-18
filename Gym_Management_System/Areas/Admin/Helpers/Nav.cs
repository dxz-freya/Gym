namespace GymManagement.Areas.Admin.Helpers
{
    public static class Nav
    {
        public static string Active(string value, string current) =>
            value.ToLower() == current?.ToLower() ? "active" : "";
    }
}
