USE AgroCompany;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Supplier') AND name = N'INN')
BEGIN
    ALTER TABLE dbo.Supplier ADD INN NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Supplier') AND name = N'Address')
BEGIN
    ALTER TABLE dbo.Supplier ADD Address NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Product') AND name = N'Unit')
BEGIN
    ALTER TABLE dbo.Product
        ADD Unit NVARCHAR(10) NOT NULL
            CONSTRAINT DF_Product_Unit DEFAULT N'т';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Product') AND name = N'BasePrice')
BEGIN
    ALTER TABLE dbo.Product
        ADD BasePrice INT NOT NULL
            CONSTRAINT DF_Product_BasePrice DEFAULT 0;
END
GO

UPDATE dbo.Product SET Unit = N'т', BasePrice = 5000 WHERE Name = N'Пшеница 3 кл.';
UPDATE dbo.Product SET Unit = N'т', BasePrice = 4000 WHERE Name = N'Ячмень фуражный';
UPDATE dbo.Product SET Unit = N'т', BasePrice = 5500 WHERE Name = N'Кукуруза';
UPDATE dbo.Product SET Unit = N'т', BasePrice = 8000 WHERE Name = N'Подсолнечник';
GO
