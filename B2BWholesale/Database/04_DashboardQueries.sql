/* =====================================================================
   04_DashboardQueries.sql  -  The SQL behind Task 1.4 (WinForms dashboard)
   Same statements the application runs (with parameters here as variables).
   "Active" revenue = every order that is NOT Cancelled.
   ===================================================================== */
USE WholesaleDB;
GO
DECLARE @From DATETIME2(0) = '20260101', @To DATETIME2(0) = '20270101';   -- @To is exclusive
DECLARE @TopN INT = 5, @LowStockThreshold INT = 20, @AgentID INT = NULL;  -- set @AgentID for one specific agent

-- 1) Best-selling items (by quantity sold, then revenue)
SELECT TOP (@TopN)
       i.ItemID, i.ItemName, i.Size,
       SUM(d.Quantity)                 AS QtySold,
       SUM(d.Quantity * d.UnitAmount)  AS Revenue
FROM dbo.OrderDetail d
JOIN dbo.[Order] o ON o.OrderID = d.OrderID
JOIN dbo.Item    i ON i.ItemID  = d.ItemID
WHERE o.OrderDate >= @From AND o.OrderDate < @To
  AND o.OrderStatus <> N'Cancelled'
  AND (@AgentID IS NULL OR o.AgentID = @AgentID)
GROUP BY i.ItemID, i.ItemName, i.Size
ORDER BY QtySold DESC, Revenue DESC;

-- 2) Low-stock warning
SELECT ItemID, ItemName, Size, StockQuantity,
       CASE WHEN StockQuantity = 0                        THEN N'OUT OF STOCK'
            WHEN StockQuantity <= @LowStockThreshold / 2  THEN N'CRITICAL'
            ELSE N'LOW' END AS Level
FROM dbo.Item
WHERE StockQuantity <= @LowStockThreshold
ORDER BY StockQuantity, ItemName;

-- 3) Total revenue by agent (set @AgentID to see one specific agent)
SELECT a.AgentID, a.AgentName,
       COUNT(DISTINCT o.OrderID)        AS Orders,
       SUM(d.Quantity)                  AS Quantity,
       SUM(d.Quantity * d.UnitAmount)   AS Revenue
FROM dbo.Agent a
JOIN dbo.[Order]    o ON o.AgentID = a.AgentID
JOIN dbo.OrderDetail d ON d.OrderID = o.OrderID
WHERE o.OrderDate >= @From AND o.OrderDate < @To
  AND o.OrderStatus <> N'Cancelled'
  AND (@AgentID IS NULL OR a.AgentID = @AgentID)
GROUP BY a.AgentID, a.AgentName
ORDER BY Revenue DESC;
GO
