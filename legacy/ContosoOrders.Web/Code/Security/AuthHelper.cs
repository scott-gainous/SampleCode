using System;
using System.Security.Cryptography;
using System.Text;

namespace ContosoOrders.Web.Code.Security
{
    public static class AuthHelper
    {
        /// <summary>
        /// Legacy password hashing: unsalted SHA1, lowercase hex. Known-weak; kept for
        /// compatibility until the "auth" migration unit replaces it (see ledger).
        /// </summary>
        public static string HashPassword(string password)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
