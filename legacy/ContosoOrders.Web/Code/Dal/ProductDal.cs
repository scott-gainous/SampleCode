using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ContosoOrders.Web.Code.Models;

namespace ContosoOrders.Web.Code.Dal
{
    public static class ProductDal
    {
        public static List<Product> GetProducts(bool includeDiscontinued)
        {
            DataTable table = SqlHelper.ExecuteDataTable("dbo.usp_Product_List",
                new SqlParameter("@IncludeDiscontinued", includeDiscontinued));
            List<Product> products = new List<Product>();
            foreach (DataRow row in table.Rows)
            {
                Product p = new Product();
                p.ProductId = (int)row["ProductId"];
                p.Sku = (string)row["Sku"];
                p.Name = (string)row["Name"];
                p.UnitPrice = (decimal)row["UnitPrice"];
                p.UnitsInStock = (int)row["UnitsInStock"];
                p.Discontinued = (bool)row["Discontinued"];
                products.Add(p);
            }
            return products;
        }
    }
}
