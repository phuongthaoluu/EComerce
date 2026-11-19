using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wholesale.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agent",
                columns: table => new
                {
                    AgentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agent", x => x.AgentID);
                    table.CheckConstraint("CK_Agent_Name", "LEN(LTRIM(RTRIM([AgentName]))) > 0");
                    table.CheckConstraint("CK_Agent_Phone", "[Phone] NOT LIKE '%[^0-9+]%' AND LEN([Phone]) BETWEEN 8 AND 15");
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemID);
                    table.CheckConstraint("CK_Item_Name", "LEN(LTRIM(RTRIM([ItemName]))) > 0");
                    table.CheckConstraint("CK_Item_Price", "[Price] > 0");
                    table.CheckConstraint("CK_Item_Stock", "[StockQuantity] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    AgentID = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.OrderID);
                    table.CheckConstraint("CK_Order_Status", "[OrderStatus] IN (N'Pending', N'Confirmed', N'Shipped', N'Delivered', N'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Order_Agent",
                        column: x => x.AgentID,
                        principalTable: "Agent",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Lock = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AgentID = table.Column<int>(type: "int", nullable: true),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.CheckConstraint("CK_Users_Email", "[Email] LIKE '_%@_%._%'");
                    table.CheckConstraint("CK_Users_Failed", "[FailedLoginCount] >= 0");
                    table.CheckConstraint("CK_Users_Role", "[Role] IN (N'Admin', N'Agent')");
                    table.CheckConstraint("CK_Users_Role_Agent", "([Role] = N'Admin' AND [AgentID] IS NULL) OR ([Role] = N'Agent' AND [AgentID] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Users_Agent",
                        column: x => x.AgentID,
                        principalTable: "Agent",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.ID);
                    table.CheckConstraint("CK_OrderDetail_Qty", "[Quantity] > 0");
                    table.CheckConstraint("CK_OrderDetail_Unit", "[UnitAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_OrderDetail_Item",
                        column: x => x.ItemID,
                        principalTable: "Item",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order",
                        column: x => x.OrderID,
                        principalTable: "Order",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Agent_Phone",
                table: "Agent",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Item_Name_Size",
                table: "Item",
                columns: new[] { "ItemName", "Size" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_AgentID",
                table: "Order",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ItemID",
                table: "OrderDetail",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "UQ_OrderDetail_Order_Item",
                table: "OrderDetail",
                columns: new[] { "OrderID", "ItemID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Users_AgentID",
                table: "Users",
                column: "AgentID",
                unique: true,
                filter: "[AgentID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Agent");
        }
    }
}
