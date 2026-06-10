USE AgroCompany;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Customer') AND name = N'INN')
BEGIN
    ALTER TABLE dbo.Customer ADD INN NVARCHAR(20) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Customer') AND name = N'DebtAmount')
BEGIN
    ALTER TABLE dbo.Customer
        ADD DebtAmount INT NOT NULL
            CONSTRAINT DF_Customer_DebtAmount DEFAULT 0;
END
GO

UPDATE dbo.Customer
SET INN = N'7701234567', DebtAmount = 85000
WHERE Name = N'ООО "АгроМаркет"';
GO

UPDATE dbo.Customer
SET INN = N'7709876543', DebtAmount = 230000
WHERE Name = N'ООО "ЗерноТорг"';
GO
