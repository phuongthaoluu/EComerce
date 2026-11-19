namespace Wholesale.Web.Models
{
    public class OrderDetail
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        /// <summary>Unit price at the time of the order (price snapshot). Line total = Quantity * UnitAmount.</summary>
        public decimal UnitAmount { get; set; }

        public Order? Order { get; set; }
        public Item? Item { get; set; }
    }
}
