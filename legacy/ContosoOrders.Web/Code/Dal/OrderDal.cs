using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ContosoOrders.Web.Code.Models;

namespace ContosoOrders.Web.Code.Dal
{
    public static class OrderDal
    {
        public static List<Order> GetOrders(int? customerId, string status)
        {
            DataTable table = SqlHelper.ExecuteDataTable("dbo.usp_Order_List",
                new SqlParameter("@CustomerId", customerId.HasValue ? (object)customerId.Value : DBNull.Value),
                new SqlParameter("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status));
            List<Order> orders = new List<Order>();
            foreach (DataRow row in table.Rows)
            {
                Order o = new Order();
                o.OrderId = (int)row["OrderId"];
                o.CustomerId = (int)row["CustomerId"];
                o.CompanyName = (string)row["CompanyName"];
                o.OrderDate = (DateTime)row["OrderDate"];
                o.Status = (string)row["Status"];
                o.ShipCity = row["ShipCity"] == DBNull.Value ? null : (string)row["ShipCity"];
                o.ShipCountry = row["ShipCountry"] == DBNull.Value ? null : (string)row["ShipCountry"];
                o.OrderTotal = (decimal)row["OrderTotal"];
                orders.Add(o);
            }
            return orders;
        }

        public static Order GetOrder(int orderId, out List<OrderItem> items)
        {
            DataSet ds = SqlHelper.ExecuteDataSet("dbo.usp_Order_GetWithItems",
                new SqlParameter("@OrderId", orderId));

            items = new List<OrderItem>();
            if (ds.Tables[0].Rows.Count == 0)
            {
                return null;
            }

            DataRow header = ds.Tables[0].Rows[0];
            Order o = new Order();
            o.OrderId = (int)header["OrderId"];
            o.CustomerId = (int)header["CustomerId"];
            o.CompanyName = (string)header["CompanyName"];
            o.OrderDate = (DateTime)header["OrderDate"];
            o.Status = (string)header["Status"];
            o.ShipCity = header["ShipCity"] == DBNull.Value ? null : (string)header["ShipCity"];
            o.ShipCountry = header["ShipCountry"] == DBNull.Value ? null : (string)header["ShipCountry"];

            decimal total = 0;
            foreach (DataRow row in ds.Tables[1].Rows)
            {
                OrderItem item = new OrderItem();
                item.OrderItemId = (int)row["OrderItemId"];
                item.ProductId = (int)row["ProductId"];
                item.Sku = (string)row["Sku"];
                item.Name = (string)row["Name"];
                item.Quantity = (int)row["Quantity"];
                item.UnitPrice = (decimal)row["UnitPrice"];
                item.LineTotal = (decimal)row["LineTotal"];
                total += item.LineTotal;
                items.Add(item);
            }
            o.OrderTotal = total;
            return o;
        }

        public static bool UpdateStatus(int orderId, string newStatus)
        {
            // BUSINESS RULE (lives here in the legacy app, NOT in the database):
            // Pending -> Shipped or Cancelled is allowed; Shipped and Cancelled are terminal.
            // Invalid transitions are silently ignored (legacy quirk preserved by parity tests).
            List<OrderItem> ignored;
            Order current = GetOrder(orderId, out ignored);
            if (current == null)
            {
                return false;
            }
            if (current.Status != "Pending")
            {
                return false;
            }
            if (newStatus != "Shipped" && newStatus != "Cancelled")
            {
                return false;
            }

            object rows = SqlHelper.ExecuteScalar("dbo.usp_Order_UpdateStatus",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Status", newStatus));
            return Convert.ToInt32(rows) > 0;
        }
    }
}
