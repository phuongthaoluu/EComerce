
# Software Engineering Lab – Assignment 2

## B2B Wholesale Management System

### 1. GitHub Repository

https://github.com/phuongthaoluu/ECommerce

### 2. Video Demo (Optional)

https://drive.google.com/file/d/1ogVbSQA_e5BNXTdCgH1yX20dBt97fZvB/view?usp=sharing

### 3. Database Connection Configuration

#### Requirements

- Visual Studio 2022
- .NET 10 SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)

#### Step 1: Open the Project

Open `B2BWholesale.slnx` in Visual Studio.

#### Step 2: Configure the Database

The project uses SQL Server with a database named `WholesaleDB`.

**For the ASP.NET Core MVC application:**

Open `Wholesale.Web/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "WholesaleDB": "Server=.\\SQLEXPRESS;Database=WholesaleDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

If `appsettings.Development.json` exists, update its connection string as well.

**For the Windows Forms application:**

Open `Wholesale.WinForms/App.config` and update the connection string:

```xml
<connectionStrings>
  <add name="WholesaleDB"
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=WholesaleDB;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=10"
       providerName="Microsoft.Data.SqlClient" />
</connectionStrings>
```

Replace `.\SQLEXPRESS` with your SQL Server instance name.

#### Step 3: Create and Seed the Database

1. Open the Visual Studio Package Manager Console.
2. Apply the existing Entity Framework Core migrations using `Update-Database`.
3. Open SQL Server Management Studio.
4. Execute `02_SeedData.sql` to insert sample data if necessary.

#### Step 4: Run the Applications

- Set `Wholesale.Web` as the startup project to run the Agent website.
- Set `Wholesale.WinForms` as the startup project to run the Admin application.

Both applications use the same `WholesaleDB` database.

### 4. Contribution of Each Member

#### Thanh – 524H0068 (35%)

- Database design, constraints, migrations, and sample data: 10%.
- WinForms Admin application, including login, item management, agent management, order management, shipping manifest, and dashboard: 25%.

#### Minh – 524H0170 (30%)

- ASP.NET Core MVC Agent login: 5%.
- Product catalog and order creation: 15%.
- Web user interface: 5%.
- Application configuration: 5%.

#### Luu Phuong Thao – 524H0192 (35%)

- My Orders, order details, and printable invoice: 10%.
- Unit testing using MSTest: 15%.
- GitHub collaboration and integration: 5%.
- Documentation and final report: 5%.

**Total Contribution: 100%**
