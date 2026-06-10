USE AgroCompany;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SalesOrder') AND name = N'StockConfirmed')
BEGIN
    ALTER TABLE dbo.SalesOrder
        ADD StockConfirmed BIT NOT NULL
            CONSTRAINT DF_SalesOrder_StockConfirmed DEFAULT 0;
END
GO

UPDATE so
SET so.StockConfirmed = 1
FROM dbo.SalesOrder so
INNER JOIN dbo.ProductStock ps ON ps.StockID = so.StockID
WHERE ps.CheckDate IS NOT NULL;
GO
