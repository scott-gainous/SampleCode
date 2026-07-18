using System;
using System.Data;
using System.Data.SqlClient;

namespace ContosoOrders.Web.Code.Dal
{
    public static class UserDal
    {
        /// <summary>
        /// Returns the user's role when the credentials match, otherwise null.
        /// NOTE: inline SQL rather than a stored procedure — an inconsistency this
        /// legacy codebase really has; recorded in migration/inventory.md (unit: auth).
        /// </summary>
        public static string ValidateUser(string username, string passwordHash)
        {
            using (SqlConnection conn = new SqlConnection(SqlHelper.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Role FROM dbo.Users WHERE Username = @Username AND PasswordHash = @PasswordHash", conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                conn.Open();
                object role = cmd.ExecuteScalar();
                return role == null ? null : (string)role;
            }
        }
    }
}
