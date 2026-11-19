
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Wholesale.Web.Controllers;
using Wholesale.Web.Data;
using Wholesale.Web.Models;
using Wholesale.Web.ViewModels;

namespace Wholesale.Tests
{
    [TestClass]
    public class OrdersControllerTests
    {
        [TestMethod]
        public async Task Create_QuantityExceedsStock_ReturnsError()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new AppDbContext(options);

            db.Items.Add(new Item
            {
                ItemID = 1,
                ItemName = "Pepsi",
                Size = "330ml",
                Price = 15000m,
                StockQuantity = 10
            });

            await db.SaveChangesAsync();

            var controller = new OrdersController(db, new TestEnvironment());

            var order = new CreateOrderViewModel();

            order.Lines.Add(new OrderLineViewModel
            {
                ItemID = 1,
                Quantity = 15
            });

            // Act
            var result = await controller.Create(order);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(controller.ModelState.IsValid);

            var errors = controller.ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage);

            StringAssert.Contains(
                string.Join("; ", errors),
                "Insufficient stock");

            Assert.AreEqual(
                10,
                (await db.Items.FirstAsync()).StockQuantity);
        }

        private class TestEnvironment : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; } = Environments.Development;
            public string ApplicationName { get; set; } = "Wholesale.Web";
            public string ContentRootPath { get; set; } = "";
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
            public string WebRootPath { get; set; } = "";
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        }


        [TestMethod]
        public async Task Create_QuantityEqualsStock_ReturnsPreview()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new AppDbContext(options);

            db.Items.Add(new Item
            {
                ItemID = 1,
                ItemName = "Pepsi",
                Size = "330ml",
                Price = 15000m,
                StockQuantity = 10
            });

            await db.SaveChangesAsync();

            var controller = new OrdersController(db, new TestEnvironment());

            var order = new CreateOrderViewModel();

            order.Lines.Add(new OrderLineViewModel
            {
                ItemID = 1,
                Quantity = 10
            });

            // Act
            var result = await controller.Create(order);

            // Assert
            Assert.IsTrue(controller.ModelState.IsValid);

            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var view = (ViewResult)result;

            Assert.AreEqual("Preview", view.ViewName);

            Assert.AreEqual(
                10,
                (await db.Items.FirstAsync()).StockQuantity);
        }


        [TestMethod]
        public async Task Create_NegativeQuantity_ReturnsError()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new AppDbContext(options);

            db.Items.Add(new Item
            {
                ItemID = 1,
                ItemName = "Pepsi",
                Size = "330ml",
                Price = 15000m,
                StockQuantity = 10
            });

            await db.SaveChangesAsync();

            var controller = new OrdersController(db, new TestEnvironment());

            var order = new CreateOrderViewModel();

            order.Lines.Add(new OrderLineViewModel
            {
                ItemID = 1,
                Quantity = -5
            });

            // Act
            var result = await controller.Create(order);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(controller.ModelState.IsValid);

            var errors = controller.ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage);

            StringAssert.Contains(
                string.Join("; ", errors),
                "Quantity cannot be negative.");

            Assert.AreEqual(
                10,
                (await db.Items.FirstAsync()).StockQuantity);
        }


    }
}
