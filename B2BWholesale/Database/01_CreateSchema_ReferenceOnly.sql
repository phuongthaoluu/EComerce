/* =====================================================================
   01_CreateSchema.sql  -  B2B Wholesale Order Management System
   Database : WholesaleDB (Microsoft SQL Server 2017+ / SQL Server Express / LocalDB)
   Run order: 01_CreateSchema.sql -> 02_SeedData.sql -> (optional) 03_IntegrityTests.sql, 04_DashboardQueries.sql
   WARNING  : drops and re-creates the 5 tables (all data is lost). Re-runnable on purpose.

   Tables   : Item, Agent, [Order], OrderDetail, Users
   Notes    : * Table name "Order" is a reserved word -> always written [Order].
              * OrderDetail.UnitAmount = unit price at the time of the order (price snapshot);
                line total = Quantity * UnitAmount.
              * Users.AgentID (extra column) links a login with Role='Agent' to its Agent record.
              * Users.FailedLoginCount / LastLoginAt (extra columns) support the account-lock rule.
              * Passwords are NEVER stored in plain text (PBKDF2-SHA256 hash string).
   ===================================================================== */
USE master;
GO
IF DB_ID(N'WholesaleDB') IS NULL
    CREATE DATABASE WholesaleDB;
GO
USE WholesaleDB;
GO

/* ---------- 0. Drop old objects (child tables first) ---------- */
IF OBJECT_ID(N'dbo.OrderDetail', N'U') IS NOT NULL DROP TABLE dbo.OrderDetail;
IF OBJECT_ID(N'dbo.[Order]',     N'U') IS NOT NULL DROP TABLE dbo.[Order];
IF OBJECT_ID(N'dbo.Users',       N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Item',        N'U') IS NOT NULL DROP TABLE dbo.Item;
IF OBJECT_ID(N'dbo.Agent',       N'U') IS NOT NULL DROP TABLE dbo.Agent;
GO

/* ---------- 1. Agent (distributor customers) ---------- */
CREATE TABLE dbo.Agent
(
    AgentID    INT IDENTITY(1,1)  NOT NULL,
    AgentName  NVARCHAR(150)      NOT NULL,
    Address    NVARCHAR(250)      NOT NULL,
    Phone      VARCHAR(15)        NOT NULL,
    CONSTRAINT PK_Agent        PRIMARY KEY (AgentID),
    CONSTRAINT UQ_Agent_Phone  UNIQUE (Phone),
    CONSTRAINT CK_Agent_Name   CHECK (LEN(LTRIM(RTRIM(AgentName))) > 0),
    CONSTRAINT CK_Agent_Phone  CHECK (Phone NOT LIKE '%[^0-9+]%' AND LEN(Phone) BETWEEN 8 AND 15)
);
GO

/* ---------- 2. Item (products in the warehouse) ---------- */
CREATE TABLE dbo.Item
(
    ItemID         INT IDENTITY(1,1)  NOT NULL,
    ItemName       NVARCHAR(150)      NOT NULL,
    Size           NVARCHAR(50)       NOT NULL,
    Price          DECIMAL(18,2)      NOT NULL,
    StockQuantity  INT                NOT NULL CONSTRAINT DF_Item_Stock DEFAULT (0),
    CONSTRAINT PK_Item            PRIMARY KEY (ItemID),
    CONSTRAINT UQ_Item_Name_Size  UNIQUE (ItemName, Size),
    CONSTRAINT CK_Item_Name       CHECK (LEN(LTRIM(RTRIM(ItemName))) > 0),
    CONSTRAINT CK_Item_Price      CHECK (Price > 0),
    CONSTRAINT CK_Item_Stock      CHECK (StockQuantity >= 0)          -- stock can never go negative
);
GO

/* ---------- 3. [Order] (master of the master-detail order) ---------- */
CREATE TABLE dbo.[Order]
(
    OrderID      INT IDENTITY(1,1)  NOT NULL,
    OrderDate    DATETIME2(0)       NOT NULL CONSTRAINT DF_Order_Date   DEFAULT (SYSDATETIME()),
    AgentID      INT                NOT NULL,
    OrderStatus  NVARCHAR(20)       NOT NULL CONSTRAINT DF_Order_Status DEFAULT (N'Pending'),
    CONSTRAINT PK_Order PRIMARY KEY (OrderID),
    CONSTRAINT FK_Order_Agent FOREIGN KEY (AgentID) REFERENCES dbo.Agent (AgentID)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,                                           -- an Agent with orders cannot be deleted
    CONSTRAINT CK_Order_Status CHECK (OrderStatus IN (N'Pending', N'Confirmed', N'Shipped', N'Delivered', N'Cancelled'))
);
CREATE INDEX IX_Order_AgentID      ON dbo.[Order] (AgentID);
CREATE INDEX IX_Order_Status_Date  ON dbo.[Order] (OrderStatus, OrderDate DESC);
GO

/* ---------- 4. OrderDetail (detail lines) ---------- */
CREATE TABLE dbo.OrderDetail
(
    ID          INT IDENTITY(1,1)  NOT NULL,
    OrderID     INT                NOT NULL,
    ItemID      INT                NOT NULL,
    Quantity    INT                NOT NULL,
    UnitAmount  DECIMAL(18,2)      NOT NULL,
    CONSTRAINT PK_OrderDetail PRIMARY KEY (ID),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID) REFERENCES dbo.[Order] (OrderID)
        ON UPDATE CASCADE
        ON DELETE CASCADE,                                             -- deleting an order deletes its lines
    CONSTRAINT FK_OrderDetail_Item FOREIGN KEY (ItemID) REFERENCES dbo.Item (ItemID)
        ON UPDATE CASCADE
        ON DELETE NO ACTION,                                           -- an Item that was ordered cannot be deleted
    CONSTRAINT UQ_OrderDetail_Order_Item UNIQUE (OrderID, ItemID),     -- one line per item in an order
    CONSTRAINT CK_OrderDetail_Qty        CHECK (Quantity > 0),
    CONSTRAINT CK_OrderDetail_Unit       CHECK (UnitAmount >= 0)
);
CREATE INDEX IX_OrderDetail_ItemID ON dbo.OrderDetail (ItemID);
GO

/* ---------- 5. Users (Admin for WinForms, Agent for the web portal) ---------- */
CREATE TABLE dbo.Users
(
    UserID            INT IDENTITY(1,1)  NOT NULL,
    UserName          NVARCHAR(50)       NOT NULL,
    Email             NVARCHAR(150)      NOT NULL,
    [Password]        NVARCHAR(200)      NOT NULL,                     -- PBKDF2 hash string, not plain text
    [Lock]            BIT                NOT NULL CONSTRAINT DF_Users_Lock   DEFAULT (0),
    [Role]            NVARCHAR(20)       NOT NULL,
    AgentID           INT                NULL,                         -- required when Role = 'Agent'
    FailedLoginCount  INT                NOT NULL CONSTRAINT DF_Users_Failed DEFAULT (0),
    LastLoginAt       DATETIME2(0)       NULL,
    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email),
    CONSTRAINT FK_Users_Agent FOREIGN KEY (AgentID) REFERENCES dbo.Agent (AgentID)
        ON UPDATE CASCADE
        ON DELETE CASCADE,                                             -- deleting an agent removes its web login
    CONSTRAINT CK_Users_Role      CHECK ([Role] IN (N'Admin', N'Agent')),
    CONSTRAINT CK_Users_Role_Agent CHECK (([Role] = N'Admin' AND AgentID IS NULL) OR ([Role] = N'Agent' AND AgentID IS NOT NULL)),
    CONSTRAINT CK_Users_Email     CHECK (Email LIKE '_%@_%._%'),
    CONSTRAINT CK_Users_Failed    CHECK (FailedLoginCount >= 0)
);
-- one web login per agent
CREATE UNIQUE INDEX UX_Users_AgentID ON dbo.Users (AgentID) WHERE AgentID IS NOT NULL;
GO

PRINT 'WholesaleDB schema created: Item, Agent, [Order], OrderDetail, Users.';
GO
