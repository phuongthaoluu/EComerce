using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Wholesale.WinForms.Data
{
    public static class AgentRepository
    {
        public static DataTable Search(string text)
        {
            string sql =
                "SELECT a.AgentID, a.AgentName, a.Address, a.Phone, " +
                "(SELECT COUNT(*) FROM dbo.[Order] o WHERE o.AgentID = a.AgentID) AS Orders " +
                "FROM dbo.Agent a WHERE 1 = 1";
            var ps = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(text))
            {
                sql += " AND (a.AgentName LIKE @s OR a.Address LIKE @s OR a.Phone LIKE @s OR CAST(a.AgentID AS NVARCHAR(20)) = @raw)";
                ps.Add(Db.P("@s", "%" + text.Trim() + "%"));
                ps.Add(Db.P("@raw", text.Trim()));
            }
            sql += " ORDER BY a.AgentName";
            return Db.Query(sql, ps.ToArray());
        }

        public static DataTable All()
        {
            return Db.Query("SELECT AgentID, AgentName FROM dbo.Agent ORDER BY AgentName");
        }

        public static int Insert(string name, string address, string phone)
        {
            object id = Db.Scalar(
                "INSERT INTO dbo.Agent (AgentName, Address, Phone) OUTPUT inserted.AgentID VALUES (@n, @a, @p)",
                Db.P("@n", name), Db.P("@a", address), Db.P("@p", phone));
            return Convert.ToInt32(id);
        }

        public static void Update(int id, string name, string address, string phone)
        {
            Db.Execute("UPDATE dbo.Agent SET AgentName = @n, Address = @a, Phone = @p WHERE AgentID = @id",
                Db.P("@n", name), Db.P("@a", address), Db.P("@p", phone), Db.P("@id", id));
        }

        public static void Delete(int id)
        {
            Db.Execute("DELETE FROM dbo.Agent WHERE AgentID = @id", Db.P("@id", id));
        }
    }
}
