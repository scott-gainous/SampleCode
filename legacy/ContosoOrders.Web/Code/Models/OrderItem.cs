namespace ContosoOrders.Web.Code.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
