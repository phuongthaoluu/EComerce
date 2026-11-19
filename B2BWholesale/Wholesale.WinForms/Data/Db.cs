using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Wholesale.WinForms.Data
{
    /// <summary>Single place that reads the connection string from App.config and runs parameterised SQL.</summary>
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["WholesaleDB"];
                if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                    throw new ConfigurationErrorsException("Connection string 'WholesaleDB' is missing in App.config.");
                return cs.ConnectionString;
            }
        }

        public static int GetIntSetting(string key, int fallback)
        {
            int v;
            return int.TryParse(ConfigurationManager.AppSettings[key], out v) ? v : fallback;
        }

        public static string GetSetting(string key, string fallback)
        {
            string v = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(v) ? fallback : v;
        }

        public static SqlConnection Open()
        {
            var c = new SqlConnection(ConnectionString);
            c.Open();
            return c;
        }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static DataTable Query(string sql, params SqlParameter[] ps)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static int Execute(string sql, params SqlParameter[] ps)
        {
            using (var conn = Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, params SqlParameter[] ps)
        {
            using (var conn = Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>Turns common SQL Server errors into messages a warehouse clerk understands.</summary>
        public static string Friendly(Exception ex)
        {
            var sql = ex as SqlException;
            if (sql == null) return ex.Message;
            switch (sql.Number)
            {
                case 547: return "This record is linked to other data (for example existing orders) or violates a rule (negative stock, invalid value), so the change was refused.";
                case 2627:
                case 2601: return "A record with the same unique value already exists (duplicate name/size, phone, username or email).";
                case 515: return "A required field is empty.";
                case -1:
                case 53:
                case 4060:
                case 18456: return "Cannot connect to the database. Check the 'WholesaleDB' connection string in App.config and that SQL Server is running.\n\n" + sql.Message;
                default: return sql.Message;
            }
        }
    }
}
