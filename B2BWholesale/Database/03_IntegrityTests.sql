/* =====================================================================
   03_IntegrityTests.sql  -  Evidence for Task 2b (integrity / constraints)
   Every test is expected to print "PASS". The whole script runs inside a
   transaction that is ROLLED BACK at the end, so the data is not changed.
   Take a screenshot of the Messages tab for the report (section 3.4).
   ===================================================================== */
USE WholesaleDB;
GO
SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT '=== A. Foreign keys ===';
BEGIN TRY
    INSERT INTO dbo.[Order] (AgentID) VALUES (9999);
    PRINT 'FAIL  A1 order with a missing Agent was inserted';
END TRY
BEGIN CATCH PRINT 'PASS  A1 FK_Order_Agent blocks a missing parent: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.OrderDetail (OrderID, ItemID, Quantity, UnitAmount) VALUES (9999, 1, 1, 1000);
    PRINT 'FAIL  A2 detail with a missing Order was inserted';
END TRY
BEGIN CATCH PRINT 'PASS  A2 FK_OrderDetail_Order blocks a missing parent: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.OrderDetail (OrderID, ItemID, Quantity, UnitAmount) VALUES (1, 9999, 1, 1000);
    PRINT 'FAIL  A3 detail with a missing Item was inserted';
END TRY
BEGIN CATCH PRINT 'PASS  A3 FK_OrderDetail_Item blocks a missing parent: ' + ERROR_MESSAGE(); END CATCH;

PRINT '=== B. Delete rules ===';
BEGIN TRY
    DELETE FROM dbo.Agent WHERE AgentID = 1;                 -- agent 1 has orders
    PRINT 'FAIL  B1 agent with orders was deleted';
END TRY
BEGIN CATCH PRINT 'PASS  B1 deleting an Agent that has Orders is blocked (NO ACTION): ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    DELETE FROM dbo.Item WHERE ItemID = 9;                   -- item 9 was ordered
    PRINT 'FAIL  B2 ordered item was deleted';
END TRY
BEGIN CATCH PRINT 'PASS  B2 deleting an Item that was ordered is blocked (NO ACTION): ' + ERROR_MESSAGE(); END CATCH;

DECLARE @linesBefore INT = (SELECT COUNT(*) FROM dbo.OrderDetail WHERE OrderID = 3);
DELETE FROM dbo.[Order] WHERE OrderID = 3;
DECLARE @linesAfter INT = (SELECT COUNT(*) FROM dbo.OrderDetail WHERE OrderID = 3);
PRINT CASE WHEN @linesBefore > 0 AND @linesAfter = 0 THEN 'PASS' ELSE 'FAIL' END
    + '  B3 ON DELETE CASCADE: order 3 had ' + CAST(@linesBefore AS VARCHAR(10)) + ' line(s), after delete ' + CAST(@linesAfter AS VARCHAR(10));

DECLARE @usersBefore INT = (SELECT COUNT(*) FROM dbo.Users WHERE AgentID = 20);
DELETE FROM dbo.Agent WHERE AgentID = 20;                    -- agent 20 has no orders
DECLARE @usersAfter INT = (SELECT COUNT(*) FROM dbo.Users WHERE AgentID = 20);
PRINT CASE WHEN @usersBefore = 1 AND @usersAfter = 0 THEN 'PASS' ELSE 'FAIL' END
    + '  B4 deleting an Agent without orders removes its login (cascade): users before=' + CAST(@usersBefore AS VARCHAR(10)) + ', after=' + CAST(@usersAfter AS VARCHAR(10));

PRINT '=== C. CHECK constraints ===';
BEGIN TRY
    UPDATE dbo.Item SET StockQuantity = -5 WHERE ItemID = 1;
    PRINT 'FAIL  C1 negative stock accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C1 CK_Item_Stock: stock cannot be negative: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    UPDATE dbo.Item SET StockQuantity = StockQuantity - 100000 WHERE ItemID = 1;   -- simulates over-deduction
    PRINT 'FAIL  C2 over-deduction accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C2 over-deduction of stock is rejected by the database: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.Item (ItemName, Size, Price, StockQuantity) VALUES (N'Test', N'1', 0, 1);
    PRINT 'FAIL  C3 price 0 accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C3 CK_Item_Price: price must be > 0: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.[Order] (AgentID, OrderStatus) VALUES (1, N'Lost');
    PRINT 'FAIL  C4 invalid status accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C4 CK_Order_Status: only Pending/Confirmed/Shipped/Delivered/Cancelled: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.OrderDetail (OrderID, ItemID, Quantity, UnitAmount) VALUES (1, 2, 0, 1000);
    PRINT 'FAIL  C5 quantity 0 accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C5 CK_OrderDetail_Qty: quantity must be > 0: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.Users (UserName, Email, [Password], [Role], AgentID) VALUES (N'x1', N'x1@test.vn', N'h', N'Manager', NULL);
    PRINT 'FAIL  C6 invalid role accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C6 CK_Users_Role: role must be Admin or Agent: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.Users (UserName, Email, [Password], [Role], AgentID) VALUES (N'x2', N'x2@test.vn', N'h', N'Agent', NULL);
    PRINT 'FAIL  C7 Agent login without an Agent accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C7 CK_Users_Role_Agent: an Agent user must reference an Agent: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.Agent (AgentName, Address, Phone) VALUES (N'Test', N'Test', N'12ab');
    PRINT 'FAIL  C8 invalid phone accepted';
END TRY
BEGIN CATCH PRINT 'PASS  C8 CK_Agent_Phone: digits only, 8-15 characters: ' + ERROR_MESSAGE(); END CATCH;

PRINT '=== D. UNIQUE constraints ===';
BEGIN TRY
    INSERT INTO dbo.Users (UserName, Email, [Password], [Role], AgentID) VALUES (N'admin', N'new@test.vn', N'h', N'Admin', NULL);
    PRINT 'FAIL  D1 duplicate username accepted';
END TRY
BEGIN CATCH PRINT 'PASS  D1 UQ_Users_UserName: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.Users (UserName, Email, [Password], [Role], AgentID) VALUES (N'newadmin', N'admin@wholesale.vn', N'h', N'Admin', NULL);
    PRINT 'FAIL  D2 duplicate email accepted';
END TRY
BEGIN CATCH PRINT 'PASS  D2 UQ_Users_Email: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.Item (ItemName, Size, Price, StockQuantity) VALUES (N'Nước suối tinh khiết', N'500ml x 24 chai', 1000, 1);
    PRINT 'FAIL  D3 duplicate item+size accepted';
END TRY
BEGIN CATCH PRINT 'PASS  D3 UQ_Item_Name_Size: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.OrderDetail (OrderID, ItemID, Quantity, UnitAmount)
    SELECT TOP 1 OrderID, ItemID, 1, 1000 FROM dbo.OrderDetail;      -- same (OrderID, ItemID) again
    PRINT 'FAIL  D4 duplicate order line accepted';
END TRY
BEGIN CATCH PRINT 'PASS  D4 UQ_OrderDetail_Order_Item: ' + ERROR_MESSAGE(); END CATCH;

BEGIN TRY
    INSERT INTO dbo.Agent (AgentName, Address, Phone) VALUES (N'Test', N'Test', N'0903112201');
    PRINT 'FAIL  D5 duplicate phone accepted';
END TRY
BEGIN CATCH PRINT 'PASS  D5 UQ_Agent_Phone: ' + ERROR_MESSAGE(); END CATCH;

PRINT '=== E. DEFAULT constraints ===';
INSERT INTO dbo.[Order] (AgentID) VALUES (2);
SELECT TOP 1 OrderID, OrderDate, AgentID, OrderStatus AS DefaultStatus_ShouldBe_Pending
FROM dbo.[Order] ORDER BY OrderID DESC;
INSERT INTO dbo.Item (ItemName, Size, Price) VALUES (N'Test default stock', N'1', 1000);
SELECT TOP 1 ItemID, ItemName, StockQuantity AS DefaultStock_ShouldBe_0 FROM dbo.Item ORDER BY ItemID DESC;

ROLLBACK TRANSACTION;
PRINT '=== Done. Transaction rolled back, data unchanged. ===';
GO
