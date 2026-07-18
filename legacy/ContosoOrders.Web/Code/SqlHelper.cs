using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ContosoOrders.Web.Code
{
    /// <summary>
    /// Thin ADO.NET helper. Every DAL call goes through here so the connection string
    /// and stored-procedure conventions live in one place.
    /// </summary>
    public static class SqlHelper
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["ContosoOrders"].ConnectionString; }
        }

        public static DataTable ExecuteDataTable(string procName, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = CreateCommand(conn, procName, parameters))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static DataSet ExecuteDataSet(string procName, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = CreateCommand(conn, procName, parameters))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        public static object ExecuteScalar(string procName, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = CreateCommand(conn, procName, parameters))
            {
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        private static SqlCommand CreateCommand(SqlConnection conn, string procName, SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd;
        }
    }
}
