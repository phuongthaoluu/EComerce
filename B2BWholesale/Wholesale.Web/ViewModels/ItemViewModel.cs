
namespace Wholesale.Web.ViewModels
{
    public class ItemViewModel
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
