using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Wholesale.WinForms.Data
{
    public static class ItemRepository
    {
        public static DataTable Search(string text, bool lowOnly, int threshold)
        {
            string sql = "SELECT ItemID, ItemName, Size, Price, StockQuantity FROM dbo.Item WHERE 1 = 1";
            var ps = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(text))
            {
                sql += " AND (ItemName LIKE @s OR Size LIKE @s OR CAST(ItemID AS NVARCHAR(20)) = @raw)";
                ps.Add(Db.P("@s", "%" + text.Trim() + "%"));
                ps.Add(Db.P("@raw", text.Trim()));
            }
            if (lowOnly)
            {
                sql += " AND StockQuantity <= @t";
                ps.Add(Db.P("@t", threshold));
            }
            sql += " ORDER BY ItemName, Size";
            return Db.Query(sql, ps.ToArray());
        }

        public static int Insert(string name, string size, decimal price, int stock)
        {
            object id = Db.Scalar(
                "INSERT INTO dbo.Item (ItemName, Size, Price, StockQuantity) OUTPUT inserted.ItemID VALUES (@n, @s, @p, @q)",
                Db.P("@n", name), Db.P("@s", size), Db.P("@p", price), Db.P("@q", stock));
            return Convert.ToInt32(id);
        }

        public static void Update(int id, string name, string size, decimal price, int stock)
        {
            Db.Execute(
                "UPDATE dbo.Item SET ItemName = @n, Size = @s, Price = @p, StockQuantity = @q WHERE ItemID = @id",
                Db.P("@n", name), Db.P("@s", size), Db.P("@p", price), Db.P("@q", stock), Db.P("@id", id));
        }

        public static void Delete(int id)
        {
            Db.Execute("DELETE FROM dbo.Item WHERE ItemID = @id", Db.P("@id", id));
        }

        /// <summary>Adds (delta &gt; 0) or removes (delta &lt; 0) stock. Returns false if it would make stock negative.</summary>
        public static bool AdjustStock(int id, int delta)
        {
            int rows = Db.Execute(
                "UPDATE dbo.Item SET StockQuantity = StockQuantity + @d WHERE ItemID = @id AND StockQuantity + @d >= 0",
                Db.P("@d", delta), Db.P("@id", id));
            return rows > 0;
        }
    }
}
