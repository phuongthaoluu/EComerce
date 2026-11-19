using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Wholesale.WinForms.Data
{
    /// <summary>Task 1.4 queries. basis: "ACTIVE" = every order except Cancelled; "COMPLETED" = Shipped + Delivered.</summary>
    public static class DashboardRepository
    {
        private static string BasisClause(string basis)
        {
            return basis == "COMPLETED"
                ? " AND o.OrderStatus IN (N'Shipped', N'Delivered')"
                : " AND o.OrderStatus <> N'Cancelled'";
        }

        public static DateTime? FirstOrderDate()
        {
            object o = Db.Scalar("SELECT MIN(OrderDate) FROM dbo.[Order]");
            return (o == null || o == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(o);
        }

        public static DataTable BestSelling(DateTime from, DateTime toInclusive, int? agentId, string basis, int top)
        {
            string sql =
                "SELECT TOP (@top) i.ItemID, i.ItemName, i.Size, SUM(d.Quantity) AS QtySold, SUM(d.Quantity * d.UnitAmount) AS Revenue " +
                "FROM dbo.OrderDetail d " +
                "JOIN dbo.[Order] o ON o.OrderID = d.OrderID " +
                "JOIN dbo.Item i ON i.ItemID = d.ItemID " +
                "WHERE o.OrderDate >= @from AND o.OrderDate < @to" + BasisClause(basis);
            var ps = new List<SqlParameter> { Db.P("@top", top), Db.P("@from", from.Date), Db.P("@to", toInclusive.Date.AddDays(1)) };
            if (agentId.HasValue)
            {
                sql += " AND o.AgentID = @agent";
                ps.Add(Db.P("@agent", agentId.Value));
            }
            sql += " GROUP BY i.ItemID, i.ItemName, i.Size ORDER BY QtySold DESC, Revenue DESC";
            return Db.Query(sql, ps.ToArray());
        }

        public static DataTable LowStock(int threshold)
        {
            return Db.Query(
                "SELECT ItemID, ItemName, Size, StockQuantity, " +
                "CASE WHEN StockQuantity = 0 THEN N'OUT OF STOCK' WHEN StockQuantity <= @t / 2 THEN N'CRITICAL' ELSE N'LOW' END AS Level " +
                "FROM dbo.Item WHERE StockQuantity <= @t ORDER BY StockQuantity, ItemName",
                Db.P("@t", threshold));
        }

        public static DataTable RevenueByAgent(DateTime from, DateTime toInclusive, int? agentId, string basis)
        {
            string sql =
                "SELECT a.AgentID, a.AgentName, COUNT(DISTINCT o.OrderID) AS Orders, SUM(d.Quantity) AS Quantity, " +
                "SUM(d.Quantity * d.UnitAmount) AS Revenue " +
                "FROM dbo.Agent a " +
                "JOIN dbo.[Order] o ON o.AgentID = a.AgentID " +
                "JOIN dbo.OrderDetail d ON d.OrderID = o.OrderID " +
                "WHERE o.OrderDate >= @from AND o.OrderDate < @to" + BasisClause(basis);
            var ps = new List<SqlParameter> { Db.P("@from", from.Date), Db.P("@to", toInclusive.Date.AddDays(1)) };
            if (agentId.HasValue)
            {
                sql += " AND a.AgentID = @agent";
                ps.Add(Db.P("@agent", agentId.Value));
            }
            sql += " GROUP BY a.AgentID, a.AgentName ORDER BY Revenue DESC";
            return Db.Query(sql, ps.ToArray());
        }
    }
}
