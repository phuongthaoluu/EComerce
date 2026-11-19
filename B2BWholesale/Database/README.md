# Database – WholesaleDB (SQL Server)

Run in SSMS, in this order:

1. `01_CreateSchema.sql` – creates database `WholesaleDB` and the 5 tables (drops them first: re-runnable).
2. `02_SeedData.sql` – 25 Items, 20 Agents, 23 Users, 20 Orders, 30 OrderDetails.
3. `03_IntegrityTests.sql` (optional) – 20 negative tests, rolled back at the end.
4. `04_DashboardQueries.sql` (optional) – the 3 queries used by the WinForms dashboard.

Demo accounts (dummy passwords): `admin` / `Admin@123` (Admin), `agent01` / `Agent@123` (Agent), `kho.cu` (locked Admin).
Passwords are PBKDF2-SHA256 hashes (`PBKDF2$100000$salt$hash`).

Notes for the web project (EF Core): table `Order` needs `[Table("Order")]`; `Users.AgentID` links an Agent login to its Agent;
`OrderDetail.UnitAmount` = unit price at order time; statuses = Pending, Confirmed, Shipped, Delivered, Cancelled.
Do not run EF migrations over this database (or map to it without `Update-Database`).
