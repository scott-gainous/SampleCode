using System;

namespace ContosoOrders.Web.Code.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CompanyName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string ShipCity { get; set; }
        public string ShipCountry { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
