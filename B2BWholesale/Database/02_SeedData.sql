/* =====================================================================
   02_SeedData.sql  -  Sample data for WholesaleDB (SQL Server)
   Item 25 rows | Agent 20 | Users 23 | [Order] 20 | OrderDetail 30
   Run AFTER the schema exists: 01_CreateSchema.sql OR the EF Core migration (Update-Database).  Safe to re-run (clears the tables first).
   DEMO PASSWORDS (dummy): all Admin users = Admin@123 ; all Agent users = Agent@123
   Passwords are stored as PBKDF2-SHA256 hashes: PBKDF2$iterations$salt(base64)$hash(base64)




   -- WARNING: This script deletes all existing data
-- in the five tables before inserting sample data.
-- Use only for development/demo databases.


   ===================================================================== */
USE WholesaleDB;
GO
SET NOCOUNT ON;
GO

-- Clear existing data (child tables first) and reset identity counters
DELETE FROM dbo.OrderDetail;
DELETE FROM dbo.[Order];
DELETE FROM dbo.Users;
DELETE FROM dbo.Item;
DELETE FROM dbo.Agent;
DBCC CHECKIDENT ('dbo.OrderDetail', RESEED, 0);
DBCC CHECKIDENT ('dbo.[Order]', RESEED, 0);
DBCC CHECKIDENT ('dbo.Users', RESEED, 0);
DBCC CHECKIDENT ('dbo.Item', RESEED, 0);
DBCC CHECKIDENT ('dbo.Agent', RESEED, 0);
GO

-- 1) Item (25 rows; some items deliberately low / out of stock for the dashboard warning)
SET IDENTITY_INSERT dbo.Item ON;
INSERT INTO dbo.Item (ItemID, ItemName, Size, Price, StockQuantity) VALUES
  (1, N'Nước suối tinh khiết', N'500ml x 24 chai', 78000, 320),
  (2, N'Nước suối tinh khiết', N'1.5L x 12 chai', 69000, 210),
  (3, N'Nước ngọt cola', N'330ml x 24 lon', 215000, 150),
  (4, N'Nước ngọt hương chanh', N'330ml x 24 lon', 205000, 12),
  (5, N'Trà xanh đóng chai', N'455ml x 24 chai', 192000, 180),
  (6, N'Nước tăng lực', N'250ml x 24 lon', 285000, 95),
  (7, N'Sữa tươi tiệt trùng ít đường', N'180ml x 48 hộp', 340000, 64),
  (8, N'Sữa đặc có đường', N'380g x 48 lon', 1180000, 40),
  (9, N'Mì gói hương gà', N'75g x 30 gói', 96000, 420),
  (10, N'Mì gói tôm chua cay', N'80g x 30 gói', 108000, 18),
  (11, N'Phở bò ăn liền', N'65g x 24 gói', 132000, 140),
  (12, N'Dầu ăn đậu nành', N'1L x 12 chai', 372000, 85),
  (13, N'Dầu ăn đậu nành', N'5L x 4 can', 690000, 9),
  (14, N'Gạo thơm ST25', N'5kg x 4 túi', 340000, 120),
  (15, N'Gạo thơm Jasmine', N'10kg x 1 bao', 215000, 75),
  (16, N'Đường tinh luyện', N'1kg x 20 gói', 420000, 55),
  (17, N'Muối i-ốt', N'500g x 40 gói', 160000, 0),
  (18, N'Nước mắm cá cơm', N'500ml x 12 chai', 540000, 48),
  (19, N'Nước tương đậu nành', N'500ml x 12 chai', 310000, 15),
  (20, N'Bột ngọt', N'400g x 20 gói', 640000, 62),
  (21, N'Nước giặt hương hoa', N'3.2kg x 4 can', 760000, 35),
  (22, N'Bột giặt', N'6kg x 3 túi', 540000, 28),
  (23, N'Nước rửa chén', N'750ml x 12 chai', 290000, 70),
  (24, N'Giấy vệ sinh', N'10 cuộn x 12 lốc', 520000, 110),
  (25, N'Khăn giấy rút', N'180 tờ x 36 gói', 330000, 17);
SET IDENTITY_INSERT dbo.Item OFF;
GO

-- 2) Agent (20 rows)
SET IDENTITY_INSERT dbo.Agent ON;
INSERT INTO dbo.Agent (AgentID, AgentName, Address, Phone) VALUES
  (1, N'Đại lý Thành Phát', N'125 Nguyễn Thị Thập, P. Tân Phong, Quận 7, TP.HCM', '0903112201'),
  (2, N'Tạp hóa Minh Châu', N'48 Lê Văn Việt, P. Hiệp Phú, TP. Thủ Đức, TP.HCM', '0903112202'),
  (3, N'Siêu thị mini An Khang', N'210 Quang Trung, P. 10, Quận Gò Vấp, TP.HCM', '0903112203'),
  (4, N'Cửa hàng Bình Dân', N'15 Phạm Văn Chiêu, P. 9, Quận Gò Vấp, TP.HCM', '0903112204'),
  (5, N'Đại lý Hồng Phúc', N'77 Nguyễn Văn Linh, P. Tân Thuận Tây, Quận 7, TP.HCM', '0903112205'),
  (6, N'Tạp hóa Ngọc Lan', N'32 Tô Ký, P. Tân Chánh Hiệp, Quận 12, TP.HCM', '0903112206'),
  (7, N'Siêu thị mini Gia Hưng', N'9 Đường số 11, P. Bình Chiểu, TP. Thủ Đức, TP.HCM', '0903112207'),
  (8, N'Đại lý Quốc Bảo', N'301 Lý Thường Kiệt, P. 15, Quận 11, TP.HCM', '0903112208'),
  (9, N'Cửa hàng Tiện Lợi Sao Mai', N'64 Hoàng Văn Thụ, P. 4, Quận Tân Bình, TP.HCM', '0903112209'),
  (10, N'Tạp hóa Thanh Tâm', N'18 Nguyễn Ảnh Thủ, P. Hiệp Thành, Quận 12, TP.HCM', '0903112210'),
  (11, N'Đại lý Kim Ngân', N'256 Cách Mạng Tháng 8, P. 10, Quận 3, TP.HCM', '0903112211'),
  (12, N'Siêu thị mini Việt Hòa', N'88 Đại lộ Bình Dương, P. Phú Hòa, TP. Thủ Dầu Một, Bình Dương', '0903112212'),
  (13, N'Đại lý Tân Lập', N'12 Phạm Văn Thuận, P. Tam Hiệp, TP. Biên Hòa, Đồng Nai', '0903112213'),
  (14, N'Tạp hóa Phước Lộc', N'143 Quốc lộ 1A, TT. Bến Lức, Huyện Bến Lức, Long An', '0903112214'),
  (15, N'Cửa hàng Hoàng Gia', N'57 Ấp Bắc, P. 4, TP. Mỹ Tho, Tiền Giang', '0903112215'),
  (16, N'Đại lý Nam Phương', N'205 Trần Hưng Đạo, P. 1, TP. Vũng Tàu, Bà Rịa - Vũng Tàu', '0903112216'),
  (17, N'Tạp hóa Út Hiền', N'21 Nguyễn Trãi, P. 2, Quận 5, TP.HCM', '0903112217'),
  (18, N'Đại lý Long Thịnh', N'190 Võ Văn Ngân, P. Bình Thọ, TP. Thủ Đức, TP.HCM', '0903112218'),
  (19, N'Siêu thị mini Phú Quý', N'73 Lê Đức Thọ, P. 6, Quận Gò Vấp, TP.HCM', '0903112219'),
  (20, N'Cửa hàng Tây Ninh Mart', N'340 Đường 30/4, P. 1, TP. Tây Ninh, Tây Ninh', '0903112220');
SET IDENTITY_INSERT dbo.Agent OFF;
GO

-- 3) Users (3 Admin + 20 Agent). Lock=1: 'kho.cu' (former staff) and 'agent15'. Role Agent links to Agent via AgentID.
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserID, UserName, Email, [Password], [Lock], [Role], AgentID, FailedLoginCount) VALUES
  (1, N'admin', N'admin@wholesale.vn', N'PBKDF2$100000$xsNre0rjYeI5+po6WUWB2Q==$22Y9it7V+dBrxDt5BqbNYBEuJkbYa+qgaXZprZblF9I=', 0, N'Admin', NULL, 0),
  (2, N'kho.minh', N'minh.kho@wholesale.vn', N'PBKDF2$100000$k7vL4rJCPB9pem2gHI5hsw==$rzM7jtn4tNzcyxrclEs9ZnhC/jsf8RaOZ129NCEqrgk=', 0, N'Admin', NULL, 0),
  (3, N'kho.cu', N'kho.cu@wholesale.vn', N'PBKDF2$100000$9ZrwdN8hn6xr/ip49f/qtg==$+nos4yxaCob0n4bBFDiBLLOWaA6VDBfhOmoC9cMY0co=', 1, N'Admin', NULL, 5),
  (4, N'agent01', N'thanhphat@daily.vn', N'PBKDF2$100000$2mXx7V/KqfVn0br8LACX+g==$0X1W86KbB43dR+BmkwkBgnpWoU43HZxa8B2/CGXS0NM=', 0, N'Agent', 1, 0),
  (5, N'agent02', N'minhchau@daily.vn', N'PBKDF2$100000$Zo6ui+71QUBPy06PV7zuZg==$LmTeFVe/nta43k/BOnXB9mQhNZ4h+re1DHYOBW7vug0=', 0, N'Agent', 2, 0),
  (6, N'agent03', N'ankhang@daily.vn', N'PBKDF2$100000$V/50q2oqi/9UqcBOX3ZTjA==$c8lZiIJPLNr0qhMYNVjfQP1+NpukoS8NsDR7aNgk0ro=', 0, N'Agent', 3, 0),
  (7, N'agent04', N'binhdan@daily.vn', N'PBKDF2$100000$7NK/zNvQmKUwfnyAI1XFsg==$XqQXdBq8ZtCU9VHv3xuO57wQ77xuOv/g6EBW+tCJ6zM=', 0, N'Agent', 4, 0),
  (8, N'agent05', N'hongphuc@daily.vn', N'PBKDF2$100000$rLmYeoeNDc5KUmviJxHmZg==$MzZoJV5/DcXx9UGim/sgbcHU526kNQouFIPtXu1w+T4=', 0, N'Agent', 5, 0),
  (9, N'agent06', N'ngoclan@daily.vn', N'PBKDF2$100000$2HfdbggY2TOVGlxL7pjz1A==$0lNY1sk1wysAuJDSgHVXFAfaIFzauImoxlwXezjr6/U=', 0, N'Agent', 6, 0),
  (10, N'agent07', N'giahung@daily.vn', N'PBKDF2$100000$E4rfRHz4XwJV2XkI6ihcJg==$OJ8fN3CMUJGZFbrxJdwrzIhS2t681tAelB97lGGWsrs=', 0, N'Agent', 7, 0),
  (11, N'agent08', N'quocbao@daily.vn', N'PBKDF2$100000$UTPYp+h7zeG35eaYcozu9Q==$P4S347XWlrCFIWXjJ0bxk0fQTHB7VCda9DaWLoihPcM=', 0, N'Agent', 8, 0),
  (12, N'agent09', N'saomai@daily.vn', N'PBKDF2$100000$9GROdYhAtc1oCtyFuBlN7Q==$xUzS2CvJGOa0rDcyfYHhGSFi6pGiXnkLdQvNRycKAaM=', 0, N'Agent', 9, 0),
  (13, N'agent10', N'thanhtam@daily.vn', N'PBKDF2$100000$VwVRfa7BJFypD+jrFo4oMw==$Ra9i179pM2IBcNMl4xbWuYNJH1zxkCPAZHW9VU6rRAw=', 0, N'Agent', 10, 0),
  (14, N'agent11', N'kimngan@daily.vn', N'PBKDF2$100000$UJqWQv9fUbFIQk58VcZ2dw==$DTSf10GvXTvtW8CF39DD6fXNz90q2KWfHWfpNQ1Norc=', 0, N'Agent', 11, 0),
  (15, N'agent12', N'viethoa@daily.vn', N'PBKDF2$100000$I2HigoyrRsRzG4DTlrBFfw==$foku8AbZIJH5tkpGdcwsjOpB3jYz2vqKe/Isf1QEkHk=', 0, N'Agent', 12, 0),
  (16, N'agent13', N'tanlap@daily.vn', N'PBKDF2$100000$RGvWzvGpKW+weM6OfDD1qQ==$TSIHewuduVL/uI6eS33QyyJzTzIvTfYWZQ4QCkl8sd8=', 0, N'Agent', 13, 0),
  (17, N'agent14', N'phuocloc@daily.vn', N'PBKDF2$100000$fZlC6CNRmwh40tbU5PhBSg==$u8cWliQ8me5sY9vWRG4HghK2DcI1YTCJDNXZa2tmakE=', 0, N'Agent', 14, 0),
  (18, N'agent15', N'hoanggia@daily.vn', N'PBKDF2$100000$p3AQUXnqKHXWK7TqZMPYzw==$hjh0lJnKDiB1QurCx2cLvi7UzSakxVJC3zrVkVQ+do0=', 1, N'Agent', 15, 5),
  (19, N'agent16', N'namphuong@daily.vn', N'PBKDF2$100000$qQkvGc9TPnDbIniww8Bttw==$sywYIEJUldLS2YlodhZ/kK8nyWDlZFt15E5EwUhE8Kg=', 0, N'Agent', 16, 0),
  (20, N'agent17', N'uthien@daily.vn', N'PBKDF2$100000$VkT7yAAGVPZaZuKDJcJCQg==$RiBTMyWLyxufDy1nPOKR0iwnlxxvnjKb5vJeHsGhlGk=', 0, N'Agent', 17, 0),
  (21, N'agent18', N'longthinh@daily.vn', N'PBKDF2$100000$apzyc7FDBuw10Rv3wUncLg==$x1Hm8wZT3H9ZEMv2NEmuk340b5571D7lC07RKoG5xwE=', 0, N'Agent', 18, 0),
  (22, N'agent19', N'phuquy@daily.vn', N'PBKDF2$100000$gtye9/Cw/2wIIqrXXIdcug==$GMwRPGdqpp0OIRlG20FcKDtXSzz8ETHci5UtSh3I630=', 0, N'Agent', 19, 0),
  (23, N'agent20', N'tayninhmart@daily.vn', N'PBKDF2$100000$FlSper7erPCwpXUQ80EHpg==$h2erMsDElYW34uezjq8mRCp5LRI3ImZgfsLZXXqeY0o=', 0, N'Agent', 20, 0);
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- 4) [Order] (20 rows; statuses: Pending, Confirmed, Shipped, Delivered, Cancelled)
SET IDENTITY_INSERT dbo.[Order] ON;
INSERT INTO dbo.[Order] (OrderID, OrderDate, AgentID, OrderStatus) VALUES
  (1, '20260824 09:15:00', 1, N'Delivered'),
  (2, '20260829 14:40:00', 3, N'Delivered'),
  (3, '20260903 10:05:00', 5, N'Delivered'),
  (4, '20260908 16:20:00', 2, N'Cancelled'),
  (5, '20260912 08:55:00', 8, N'Delivered'),
  (6, '20260917 11:30:00', 1, N'Delivered'),
  (7, '20260922 15:10:00', 12, N'Delivered'),
  (8, '20260927 09:45:00', 6, N'Delivered'),
  (9, '20261002 13:25:00', 4, N'Delivered'),
  (10, '20261006 10:50:00', 9, N'Delivered'),
  (11, '20261011 14:05:00', 13, N'Delivered'),
  (12, '20261015 09:10:00', 3, N'Cancelled'),
  (13, '20261019 16:35:00', 7, N'Delivered'),
  (14, '20261024 11:15:00', 10, N'Shipped'),
  (15, '20261029 08:40:00', 5, N'Shipped'),
  (16, '20261103 15:55:00', 11, N'Shipped'),
  (17, '20261108 10:20:00', 14, N'Confirmed'),
  (18, '20261112 13:45:00', 1, N'Confirmed'),
  (19, '20261116 09:30:00', 16, N'Pending'),
  (20, '20261117 14:50:00', 8, N'Pending');
SET IDENTITY_INSERT dbo.[Order] OFF;
GO

-- 5) OrderDetail (30 rows). UnitAmount = unit price of the item at the time of the order (price snapshot).
SET IDENTITY_INSERT dbo.OrderDetail ON;
INSERT INTO dbo.OrderDetail (ID, OrderID, ItemID, Quantity, UnitAmount) VALUES
  (1, 1, 4, 12, 199000),
  (2, 2, 17, 5, 160000),
  (3, 2, 21, 15, 760000),
  (4, 3, 18, 20, 540000),
  (5, 3, 24, 20, 504000),
  (6, 3, 25, 5, 320000),
  (7, 4, 4, 20, 199000),
  (8, 5, 22, 12, 524000),
  (9, 5, 16, 10, 407000),
  (10, 6, 12, 15, 372000),
  (11, 7, 17, 5, 160000),
  (12, 7, 21, 5, 737000),
  (13, 8, 10, 20, 105000),
  (14, 9, 21, 10, 760000),
  (15, 10, 1, 20, 78000),
  (16, 10, 23, 20, 290000),
  (17, 10, 12, 15, 372000),
  (18, 11, 3, 15, 215000),
  (19, 12, 19, 20, 310000),
  (20, 12, 18, 5, 540000),
  (21, 13, 14, 20, 340000),
  (22, 14, 13, 10, 690000),
  (23, 14, 17, 15, 160000),
  (24, 15, 19, 20, 310000),
  (25, 16, 17, 8, 160000),
  (26, 16, 1, 50, 78000),
  (27, 17, 5, 3, 192000),
  (28, 18, 23, 5, 290000),
  (29, 19, 23, 15, 290000),
  (30, 20, 16, 6, 420000);
SET IDENTITY_INSERT dbo.OrderDetail OFF;
GO

-- Row counts
SELECT 'Item' AS TableName, COUNT(*) AS Rows FROM dbo.Item
UNION ALL SELECT 'Agent', COUNT(*) FROM dbo.Agent
UNION ALL SELECT 'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT 'Order', COUNT(*) FROM dbo.[Order]
UNION ALL SELECT 'OrderDetail', COUNT(*) FROM dbo.OrderDetail;
GO
