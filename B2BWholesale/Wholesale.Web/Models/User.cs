namespace Wholesale.Web.Models
{
    /// <summary>Login account (SQL Server table "Users"). Role = Admin (WinForms) or Agent (web portal).</summary>
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        /// <summary>PBKDF2 hash string (PBKDF2$iterations$salt$hash), never the plain password.</summary>
        public string Password { get; set; } = string.Empty;
        public bool Lock { get; set; }
        public string Role { get; set; } = "Agent";
        /// <summary>Required when Role = Agent, null for Admin.</summary>
        public int? AgentID { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public Agent? Agent { get; set; }
    }
}
