using System;
using System.Data;
using Wholesale.WinForms.Data;
using Wholesale.WinForms.Security;

namespace Wholesale.WinForms.Services
{
    public enum LoginStatus { Success, Empty, InvalidCredentials, Locked, NotAdmin }

    public class LoginResult
    {
        public LoginStatus Status { get; set; }
        public string Message { get; set; }
        public AuthUser User { get; set; }
    }

    /// <summary>Task 1.1 - secure login. Only active (not locked) users with Role = Admin may enter.</summary>
    public static class LoginService
    {
        public static LoginResult Login(string userNameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrEmpty(password))
                return new LoginResult { Status = LoginStatus.Empty, Message = "Please enter your username (or email) and password." };

            string ident = userNameOrEmail.Trim();
            int maxFailed = Db.GetIntSetting("MaxFailedLogins", 5);

            DataTable dt = Db.Query(
                "SELECT UserID, UserName, Email, [Password], [Lock], [Role], FailedLoginCount " +
                "FROM dbo.Users WHERE UserName = @u OR Email = @u",
                Db.P("@u", ident));

            // generic message: do not reveal whether the account exists
            var invalid = new LoginResult { Status = LoginStatus.InvalidCredentials, Message = "Invalid username or password." };
            if (dt.Rows.Count == 0) return invalid;

            DataRow r = dt.Rows[0];
            int id = Convert.ToInt32(r["UserID"]);

            if (Convert.ToBoolean(r["Lock"]))
                return new LoginResult { Status = LoginStatus.Locked, Message = "This account is locked. Please contact the system administrator." };

            if (!PasswordHasher.Verify(password, Convert.ToString(r["Password"])))
            {
                DataTable upd = Db.Query(
                    "UPDATE dbo.Users SET FailedLoginCount = FailedLoginCount + 1, " +
                    "[Lock] = CASE WHEN FailedLoginCount + 1 >= @max THEN 1 ELSE [Lock] END " +
                    "OUTPUT inserted.FailedLoginCount, inserted.[Lock] WHERE UserID = @id",
                    Db.P("@max", maxFailed), Db.P("@id", id));
                int failed = Convert.ToInt32(upd.Rows[0]["FailedLoginCount"]);
                bool locked = Convert.ToBoolean(upd.Rows[0]["Lock"]);
                if (locked)
                    return new LoginResult { Status = LoginStatus.Locked, Message = "Too many failed attempts. The account has been locked." };
                invalid.Message = "Invalid username or password. Attempts left: " + Math.Max(0, maxFailed - failed) + ".";
                return invalid;
            }

            string role = Convert.ToString(r["Role"]);
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return new LoginResult { Status = LoginStatus.NotAdmin, Message = "Access denied. Only users with the Admin role can use the back-office application." };

            Db.Execute("UPDATE dbo.Users SET FailedLoginCount = 0, LastLoginAt = SYSDATETIME() WHERE UserID = @id", Db.P("@id", id));
            return new LoginResult
            {
                Status = LoginStatus.Success,
                Message = "OK",
                User = new AuthUser
                {
                    UserID = id,
                    UserName = Convert.ToString(r["UserName"]),
                    Email = Convert.ToString(r["Email"]),
                    Role = role
                }
            };
        }
    }
}
