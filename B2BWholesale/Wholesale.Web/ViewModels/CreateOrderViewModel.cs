
namespace Wholesale.Web.ViewModels
{
    public class OrderLineViewModel
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderViewModel
    {
        public List<OrderLineViewModel> Lines { get; set; }
            = new List<OrderLineViewModel>();
    }
}
