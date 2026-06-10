USE AgroCompany;
GO

IF NOT EXISTS (SELECT 1 FROM SalesOrderStatus)
    INSERT INTO SalesOrderStatus (StatusName) VALUES
        (N'Новый'), (N'Подтверждён'), (N'Готов к отгрузке'), (N'Отгружен');
GO

IF NOT EXISTS (SELECT 1 FROM PurchaseOrderStatus)
    INSERT INTO PurchaseOrderStatus (StatusName) VALUES
        (N'Оформлен'), (N'Исполнен'), (N'Отменён');
GO

IF NOT EXISTS (SELECT 1 FROM Warehouse)
    INSERT INTO Warehouse (Name) VALUES (N'Склад №1 (Центральный)'), (N'Склад №2 (Северный)');
GO

IF NOT EXISTS (SELECT 1 FROM Product)
    INSERT INTO Product (Name, Variety, Seasonality) VALUES
        (N'Пшеница 3 кл.', N'Экстра', N'Лето-2025'),
        (N'Ячмень фуражный', N'1-й класс', N'Осень-2025'),
        (N'Кукуруза', N'Гибрид', N'Осень-2025'),
        (N'Подсолнечник', N'Масличный', N'Лето-2025');
GO

IF NOT EXISTS (SELECT 1 FROM Customer)
    INSERT INTO Customer (Name, PhoneNumber, Email, Address, INN, DebtAmount) VALUES
        (N'ООО "АгроМаркет"', '+7 495 123-45-67', 'info@agromarket.ru', N'г. Москва, ул. Ленина, 10', N'7701234567', 85000),
        (N'ООО "ЗерноТорг"', '+7 499 345-67-89', 'info@zernotorg.ru', N'г. Москва, пр. Мира, 22', N'7709876543', 230000);
GO

IF NOT EXISTS (SELECT 1 FROM Supplier)
    INSERT INTO Supplier (Name, PhoneNumber, Email) VALUES
        (N'ООО "АгроСнаб"', '+7 495 500-10-20', 'agro@snab.ru'),
        (N'ЗАО "Зерновые ресурсы"', '+7 812 600-20-30', 'grain@res.ru');
GO

IF NOT EXISTS (SELECT 1 FROM ProductStock)
BEGIN
    INSERT INTO ProductStock (ProductID, WarehouseID, Quantity, CheckDate) VALUES
        (1, 1, 85, '2026-06-09'),
        (2, 1, 30, '2026-06-09'),
        (3, 2, 0, '2026-06-07'),
        (4, 2, 44, '2026-06-08');
END
GO

IF NOT EXISTS (SELECT 1 FROM SalesOrder)
BEGIN
    DECLARE @Emp INT = (SELECT TOP 1 EmployeeID FROM Employee);
    IF @Emp IS NOT NULL
        INSERT INTO SalesOrder (CustomerID, EmployeeID, StockID, StatusID, CreationDate, PricePerKg, Quantity) VALUES
            (1, @Emp, 1, 1, '2026-06-09', 5.00, 50),
            (2, @Emp, 2, 2, '2026-06-08', 4.00, 20);
END
GO

IF NOT EXISTS (SELECT 1 FROM PurchaseOrder)
BEGIN
    DECLARE @Emp2 INT = (SELECT TOP 1 EmployeeID FROM Employee);
    IF @Emp2 IS NOT NULL
        INSERT INTO PurchaseOrder (SupplierID, EmployeeID, StatusID, CreationDate) VALUES
            (1, @Emp2, 1, '2026-06-05'),
            (2, @Emp2, 2, '2026-06-01');
END
GO
