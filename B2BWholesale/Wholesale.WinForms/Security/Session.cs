namespace Wholesale.WinForms.Security
{
    public class AuthUser
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    /// <summary>Holds the logged-in Admin for the lifetime of the main window.</summary>
    public static class Session
    {
        public static AuthUser CurrentUser { get; set; }
        public static void Clear() { CurrentUser = null; }
    }
}
