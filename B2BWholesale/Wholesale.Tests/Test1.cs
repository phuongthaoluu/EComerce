
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wholesale.Web.ViewModels;

namespace Wholesale.Tests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void CalculateAmount_ValidQuantityAndPrice_ReturnsCorrectAmount()
        {
            // Arrange
            var item = new OrderItemViewModel
            {
                ItemName = "Pepsi",
                Quantity = 5,
                UnitAmount = 15000m
            };

            // Act
            decimal actualAmount = item.Amount;

            // Assert
            Assert.AreEqual(75000m, actualAmount);
        }


        [TestMethod]
        public void CalculateAmount_ZeroQuantity_ReturnsZero()
        {
            // Arrange
            var item = new OrderItemViewModel
            {
                ItemName = "Pepsi",
                Quantity = 0,
                UnitAmount = 15000m
            };

            // Act
            decimal actualAmount = item.Amount;

            // Assert
            Assert.AreEqual(0m, actualAmount);
        }


        [TestMethod]
        public void CalculateAmount_DecimalPrice_ReturnsCorrectAmount()
        {
            // Arrange
            var item = new OrderItemViewModel
            {
                ItemName = "Coca-Cola",
                Quantity = 3,
                UnitAmount = 12500.50m
            };

            // Act
            decimal actualAmount = item.Amount;

            // Assert
            Assert.AreEqual(37501.50m, actualAmount);
        }


        [TestMethod]
        public void CalculateOrderTotal_MultipleItems_ReturnsCorrectTotal()
        {
            // Arrange
            var order = new OrderDetailsViewModel
            {
                Items = new List<OrderItemViewModel>
        {
            new OrderItemViewModel
            {
                ItemName = "Pepsi",
                Quantity = 5,
                UnitAmount = 15000m
            },
            new OrderItemViewModel
            {
                ItemName = "Coca-Cola",
                Quantity = 10,
                UnitAmount = 12000m
            }
        }
            };

            // Act
            decimal total = order.Items.Sum(item => item.Amount);

            // Assert
            Assert.AreEqual(195000m, total);
        }


        [TestMethod]
        public void CanTransition_PendingToConfirmed_ReturnsTrue()
        {
            // Arrange
            string from = "Pending";
            string to = "Confirmed";

            // Act
            bool result = Wholesale.WinForms.Services.OrderRules
                .CanTransition(from, to);

            // Assert
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void CanTransition_PendingToDelivered_ReturnsFalse()
        {
            // Arrange
            string from = "Pending";
            string to = "Delivered";

            // Act
            bool result = Wholesale.WinForms.Services.OrderRules
                .CanTransition(from, to);

            // Assert
            Assert.IsFalse(result);
        }


        [DataTestMethod]
        [DataRow("Pending", "Confirmed", true)]
        [DataRow("Pending", "Cancelled", true)]
        [DataRow("Pending", "Delivered", false)]
        [DataRow("Confirmed", "Shipped", true)]
        [DataRow("Confirmed", "Cancelled", true)]
        [DataRow("Confirmed", "Delivered", false)]
        [DataRow("Shipped", "Delivered", true)]
        [DataRow("Shipped", "Cancelled", false)]
        [DataRow("Delivered", "Pending", false)]
        [DataRow("Cancelled", "Confirmed", false)]
        public void CanTransition_MultipleStatuses_ReturnsExpected(
            string from,
            string to,
            bool expected)
        {
            // Act
            bool actual = Wholesale.WinForms.Services.OrderRules
                .CanTransition(from, to);

            // Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void Login_EmptyUsername_ReturnsEmpty()
        {
            // Arrange
            string username = "";
            string password = "123456";

            // Act
            var result = Wholesale.WinForms.Services.LoginService
                .Login(username, password);

            // Assert
            Assert.AreEqual(
                Wholesale.WinForms.Services.LoginStatus.Empty,
                result.Status);
        }


        [TestMethod]
        public void Login_EmptyPassword_ReturnsEmpty()
        {
            // Arrange
            string username = "admin";
            string password = "";

            // Act
            var result = Wholesale.WinForms.Services.LoginService
                .Login(username, password);

            // Assert
            Assert.AreEqual(
                Wholesale.WinForms.Services.LoginStatus.Empty,
                result.Status);
        }


        [TestMethod]
        public void PasswordHasher_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            string password = "Admin@123";

            string hashedPassword =
                Wholesale.WinForms.Security.PasswordHasher.Hash(password);

            // Act
            bool result =
                Wholesale.WinForms.Security.PasswordHasher.Verify(
                    password,
                    hashedPassword);

            // Assert
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void PasswordHasher_WrongPassword_ReturnsFalse()
        {
            // Arrange
            string correctPassword = "Admin@123";
            string wrongPassword = "Admin@456";

            string hashedPassword =
                Wholesale.WinForms.Security.PasswordHasher.Hash(correctPassword);

            // Act
            bool result =
                Wholesale.WinForms.Security.PasswordHasher.Verify(
                    wrongPassword,
                    hashedPassword);

            // Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void PasswordHasher_InvalidHash_ReturnsFalse()
        {
            // Arrange
            string password = "Admin@123";
            string invalidHash = "invalid-hash-data";

            // Act
            bool result =
                Wholesale.WinForms.Security.PasswordHasher.Verify(
                    password,
                    invalidHash);

            // Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void PasswordHasher_SamePassword_ProducesDifferentHashes()
        {
            // Arrange
            string password = "Admin@123";

            // Act
            string hash1 = Wholesale.WinForms.Security.PasswordHasher.Hash(password);
            string hash2 = Wholesale.WinForms.Security.PasswordHasher.Hash(password);

            // Assert
            Assert.AreNotEqual(hash1, hash2);

            Assert.IsTrue(
                Wholesale.WinForms.Security.PasswordHasher.Verify(password, hash1));

            Assert.IsTrue(
                Wholesale.WinForms.Security.PasswordHasher.Verify(password, hash2));
        }


    }
}
