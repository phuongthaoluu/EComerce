using Microsoft.EntityFrameworkCore;
using Wholesale.Web.Models;

namespace Wholesale.Web.Data
{
    /// <summary>
    /// Code First model of WholesaleDB. Table/column names, keys, foreign-key rules, CHECK, UNIQUE and DEFAULT
    /// constraints are identical to Database/01_CreateSchema.sql, so Database/02_SeedData.sql works on the
    /// database created by the EF migration.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Agent> Agents => Set<Agent>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ---------------- Agent
            mb.Entity<Agent>(e =>
            {
                e.ToTable("Agent", t =>
                {
                    t.HasCheckConstraint("CK_Agent_Name", "LEN(LTRIM(RTRIM([AgentName]))) > 0");
                    t.HasCheckConstraint("CK_Agent_Phone", "[Phone] NOT LIKE '%[^0-9+]%' AND LEN([Phone]) BETWEEN 8 AND 15");
                });
                e.HasKey(x => x.AgentID).HasName("PK_Agent");
                e.Property(x => x.AgentName).HasMaxLength(150).IsRequired();
                e.Property(x => x.Address).HasMaxLength(250).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(15).IsUnicode(false).IsRequired();
                e.HasIndex(x => x.Phone).IsUnique().HasDatabaseName("UQ_Agent_Phone");
            });

            // ---------------- Item
            mb.Entity<Item>(e =>
            {
                e.ToTable("Item", t =>
                {
                    t.HasCheckConstraint("CK_Item_Name", "LEN(LTRIM(RTRIM([ItemName]))) > 0");
                    t.HasCheckConstraint("CK_Item_Price", "[Price] > 0");
                    t.HasCheckConstraint("CK_Item_Stock", "[StockQuantity] >= 0");   // stock can never go negative
                });
                e.HasKey(x => x.ItemID).HasName("PK_Item");
                e.Property(x => x.ItemName).HasMaxLength(150).IsRequired();
                e.Property(x => x.Size).HasMaxLength(50).IsRequired();
                e.Property(x => x.Price).HasColumnType("decimal(18,2)");
                e.Property(x => x.StockQuantity).HasDefaultValue(0);
                e.HasIndex(x => new { x.ItemName, x.Size }).IsUnique().HasDatabaseName("UQ_Item_Name_Size");
            });

            // ---------------- Order
            mb.Entity<Order>(e =>
            {
                e.ToTable("Order", t =>
                    t.HasCheckConstraint("CK_Order_Status", "[OrderStatus] IN (N'Pending', N'Confirmed', N'Shipped', N'Delivered', N'Cancelled')"));
                e.HasKey(x => x.OrderID).HasName("PK_Order");
                e.Property(x => x.OrderDate).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
                e.Property(x => x.OrderStatus).HasMaxLength(20).IsRequired().HasDefaultValue("Pending");
                e.HasOne(x => x.Agent).WithMany(a => a.Orders).HasForeignKey(x => x.AgentID)
                    .OnDelete(DeleteBehavior.Restrict)                 // an Agent with orders cannot be deleted
                    .HasConstraintName("FK_Order_Agent");
                e.HasIndex(x => x.AgentID).HasDatabaseName("IX_Order_AgentID");
            });

            // ---------------- OrderDetail
            mb.Entity<OrderDetail>(e =>
            {
                e.ToTable("OrderDetail", t =>
                {
                    t.HasCheckConstraint("CK_OrderDetail_Qty", "[Quantity] > 0");
                    t.HasCheckConstraint("CK_OrderDetail_Unit", "[UnitAmount] >= 0");
                });
                e.HasKey(x => x.ID).HasName("PK_OrderDetail");
                e.Property(x => x.UnitAmount).HasColumnType("decimal(18,2)");
                e.HasOne(x => x.Order).WithMany(o => o.OrderDetails).HasForeignKey(x => x.OrderID)
                    .OnDelete(DeleteBehavior.Cascade)                  // deleting an order deletes its lines
                    .HasConstraintName("FK_OrderDetail_Order");
                e.HasOne(x => x.Item).WithMany(i => i.OrderDetails).HasForeignKey(x => x.ItemID)
                    .OnDelete(DeleteBehavior.Restrict)                 // an ordered Item cannot be deleted
                    .HasConstraintName("FK_OrderDetail_Item");
                e.HasIndex(x => new { x.OrderID, x.ItemID }).IsUnique().HasDatabaseName("UQ_OrderDetail_Order_Item");
                e.HasIndex(x => x.ItemID).HasDatabaseName("IX_OrderDetail_ItemID");
            });

            // ---------------- Users
            mb.Entity<User>(e =>
            {
                e.ToTable("Users", t =>
                {
                    t.HasCheckConstraint("CK_Users_Role", "[Role] IN (N'Admin', N'Agent')");
                    t.HasCheckConstraint("CK_Users_Role_Agent", "([Role] = N'Admin' AND [AgentID] IS NULL) OR ([Role] = N'Agent' AND [AgentID] IS NOT NULL)");
                    t.HasCheckConstraint("CK_Users_Email", "[Email] LIKE '_%@_%._%'");
                    t.HasCheckConstraint("CK_Users_Failed", "[FailedLoginCount] >= 0");
                });
                e.HasKey(x => x.UserID).HasName("PK_Users");
                e.Property(x => x.UserName).HasMaxLength(50).IsRequired();
                e.Property(x => x.Email).HasMaxLength(150).IsRequired();
                e.Property(x => x.Password).HasMaxLength(200).IsRequired();
                e.Property(x => x.Lock).HasDefaultValue(false);
                e.Property(x => x.Role).HasMaxLength(20).IsRequired();
                e.Property(x => x.FailedLoginCount).HasDefaultValue(0);
                e.Property(x => x.LastLoginAt).HasColumnType("datetime2(0)");
                e.HasIndex(x => x.UserName).IsUnique().HasDatabaseName("UQ_Users_UserName");
                e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_Users_Email");
                e.HasIndex(x => x.AgentID).IsUnique().HasFilter("[AgentID] IS NOT NULL").HasDatabaseName("UX_Users_AgentID");
                e.HasOne(x => x.Agent).WithOne(a => a.User).HasForeignKey<User>(x => x.AgentID)
                    .OnDelete(DeleteBehavior.Cascade)                  // deleting an agent removes its web login
                    .HasConstraintName("FK_Users_Agent");
            });
        }
    }
}
