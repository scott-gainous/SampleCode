using System;

namespace ContosoOrders.Web.Code.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
