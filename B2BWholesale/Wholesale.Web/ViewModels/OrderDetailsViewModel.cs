
namespace Wholesale.Web.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = "";
        public decimal TotalAmount { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new();
    }

    public class OrderItemViewModel
    {
        public string ItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitAmount { get; set; }

        public decimal Amount => Quantity * UnitAmount;
    }
}
