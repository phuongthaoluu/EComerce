namespace Wholesale.Web.Models
{
    /// <summary>Master of the master-detail order. Table name in SQL Server: [Order].</summary>
    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public int AgentID { get; set; }
        /// <summary>Pending, Confirmed, Shipped, Delivered or Cancelled.</summary>
        public string OrderStatus { get; set; } = "Pending";

        public Agent? Agent { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
