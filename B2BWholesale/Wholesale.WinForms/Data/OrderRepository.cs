using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Wholesale.WinForms.Services;

namespace Wholesale.WinForms.Data
{
    public static class OrderRepository
    {
        /// <summary>Orders with agent and totals. Pending orders (new web orders) are listed first.</summary>
        public static DataTable GetOrders(string status, string search)
        {
            string sql =
                "SELECT o.OrderID, o.OrderDate, a.AgentName, a.Phone, o.OrderStatus, " +
                "ISNULL(SUM(d.Quantity), 0) AS TotalQty, ISNULL(SUM(d.Quantity * d.UnitAmount), 0) AS TotalAmount " +
                "FROM dbo.[Order] o " +
                "JOIN dbo.Agent a ON a.AgentID = o.AgentID " +
                "LEFT JOIN dbo.OrderDetail d ON d.OrderID = o.OrderID " +
                "WHERE 1 = 1";
            var ps = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND o.OrderStatus = @st";
                ps.Add(Db.P("@st", status));
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (a.AgentName LIKE @s OR CAST(o.OrderID AS NVARCHAR(20)) = @raw)";
                ps.Add(Db.P("@s", "%" + search.Trim() + "%"));
                ps.Add(Db.P("@raw", search.Trim()));
            }
            sql += " GROUP BY o.OrderID, o.OrderDate, a.AgentName, a.Phone, o.OrderStatus " +
                   "ORDER BY CASE WHEN o.OrderStatus = N'Pending' THEN 0 ELSE 1 END, o.OrderDate DESC, o.OrderID DESC";
            return Db.Query(sql, ps.ToArray());
        }

        public static DataTable GetLines(int orderId)
        {
            return Db.Query(
                "SELECT d.ID, i.ItemName, i.Size, d.Quantity, d.UnitAmount, d.Quantity * d.UnitAmount AS LineTotal " +
                "FROM dbo.OrderDetail d JOIN dbo.Item i ON i.ItemID = d.ItemID " +
                "WHERE d.OrderID = @id ORDER BY d.ID",
                Db.P("@id", orderId));
        }

        public static DataRow GetHeader(int orderId)
        {
            DataTable dt = Db.Query(
                "SELECT o.OrderID, o.OrderDate, o.OrderStatus, a.AgentID, a.AgentName, a.Address, a.Phone " +
                "FROM dbo.[Order] o JOIN dbo.Agent a ON a.AgentID = o.AgentID WHERE o.OrderID = @id",
                Db.P("@id", orderId));
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public static int CountByStatus(string status)
        {
            return Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM dbo.[Order] WHERE OrderStatus = @s", Db.P("@s", status)));
        }

        /// <summary>
        /// Changes the status in one transaction. Cancelling an order returns its quantities to stock
        /// (the web portal deducted them when the order was placed).
        /// </summary>
        public static void UpdateStatus(int orderId, string newStatus)
        {
            using (SqlConnection conn = Db.Open())
            using (SqlTransaction tx = conn.BeginTransaction())
            {
                try
                {
                    string current;
                    using (var cmd = new SqlCommand("SELECT OrderStatus FROM dbo.[Order] WITH (UPDLOCK, ROWLOCK) WHERE OrderID = @id", conn, tx))
                    {
                        cmd.Parameters.Add(Db.P("@id", orderId));
                        object o = cmd.ExecuteScalar();
                        if (o == null) throw new InvalidOperationException("Order #" + orderId + " no longer exists.");
                        current = Convert.ToString(o);
                    }
                    if (!OrderRules.CanTransition(current, newStatus))
                        throw new InvalidOperationException("An order that is '" + current + "' cannot be changed to '" + newStatus + "'.");

                    if (newStatus == "Cancelled")
                    {
                        using (var cmd = new SqlCommand(
                            "UPDATE i SET i.StockQuantity = i.StockQuantity + d.Quantity " +
                            "FROM dbo.Item i JOIN dbo.OrderDetail d ON d.ItemID = i.ItemID WHERE d.OrderID = @id", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@id", orderId));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (var cmd = new SqlCommand("UPDATE dbo.[Order] SET OrderStatus = @st WHERE OrderID = @id", conn, tx))
                    {
                        cmd.Parameters.Add(Db.P("@st", newStatus));
                        cmd.Parameters.Add(Db.P("@id", orderId));
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
