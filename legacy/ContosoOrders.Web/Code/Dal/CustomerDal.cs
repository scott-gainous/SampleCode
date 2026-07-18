using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ContosoOrders.Web.Code.Models;

namespace ContosoOrders.Web.Code.Dal
{
    public static class CustomerDal
    {
        public static List<Customer> GetCustomers(bool includeInactive)
        {
            DataTable table = SqlHelper.ExecuteDataTable("dbo.usp_Customer_List",
                new SqlParameter("@IncludeInactive", includeInactive));
            List<Customer> customers = new List<Customer>();
            foreach (DataRow row in table.Rows)
            {
                customers.Add(MapCustomer(row));
            }
            return customers;
        }

        public static Customer GetCustomer(int customerId)
        {
            DataTable table = SqlHelper.ExecuteDataTable("dbo.usp_Customer_Get",
                new SqlParameter("@CustomerId", customerId));
            if (table.Rows.Count == 0)
            {
                return null;
            }
            return MapCustomer(table.Rows[0]);
        }

        public static int SaveCustomer(Customer customer)
        {
            object newId = SqlHelper.ExecuteScalar("dbo.usp_Customer_Save",
                new SqlParameter("@CustomerId", customer.CustomerId),
                new SqlParameter("@CompanyName", customer.CompanyName),
                new SqlParameter("@ContactName", customer.ContactName),
                new SqlParameter("@Email", customer.Email),
                new SqlParameter("@Phone", (object)customer.Phone ?? DBNull.Value),
                new SqlParameter("@City", (object)customer.City ?? DBNull.Value),
                new SqlParameter("@Country", (object)customer.Country ?? DBNull.Value),
                new SqlParameter("@IsActive", customer.IsActive));
            return Convert.ToInt32(newId);
        }

        private static Customer MapCustomer(DataRow row)
        {
            Customer c = new Customer();
            c.CustomerId = (int)row["CustomerId"];
            c.CompanyName = (string)row["CompanyName"];
            c.ContactName = (string)row["ContactName"];
            c.Email = (string)row["Email"];
            c.Phone = row["Phone"] == DBNull.Value ? null : (string)row["Phone"];
            c.City = row["City"] == DBNull.Value ? null : (string)row["City"];
            c.Country = row["Country"] == DBNull.Value ? null : (string)row["Country"];
            c.IsActive = (bool)row["IsActive"];
            c.CreatedUtc = (DateTime)row["CreatedUtc"];
            return c;
        }
    }
}
