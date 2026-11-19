
using Wholesale.Web.ViewModels;

namespace Wholesale.Web.Services
{
    public static class SampleItemData
    {
        public static List<ItemViewModel> GetItems()
        {
            return new List<ItemViewModel>
            {
                new ItemViewModel
                {
                    ItemID = 1,
                    ItemName = "Pepsi",
                    Size = "330ml",
                    Price = 10000,
                    StockQuantity = 100
                },
                new ItemViewModel
                {
                    ItemID = 2,
                    ItemName = "Coca-Cola",
                    Size = "330ml",
                    Price = 11000,
                    StockQuantity = 80
                },
                new ItemViewModel
                {
                    ItemID = 3,
                    ItemName = "Aquafina",
                    Size = "500ml",
                    Price = 7000,
                    StockQuantity = 150
                },
                new ItemViewModel
                {
                    ItemID = 4,
                    ItemName = "Sting",
                    Size = "330ml",
                    Price = 12000,
                    StockQuantity = 60
                },
                new ItemViewModel
                {
                    ItemID = 5,
                    ItemName = "Oolong Tea",
                    Size = "450ml",
                    Price = 13000,
                    StockQuantity = 40
                }
            };
        }
    }
}
