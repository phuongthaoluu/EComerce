namespace Wholesale.Web.ViewModels
{
    public class MyOrderViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }
}
