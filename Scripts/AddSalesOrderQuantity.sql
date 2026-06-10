USE AgroCompany;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SalesOrder') AND name = N'Quantity')
BEGIN
    ALTER TABLE dbo.SalesOrder
        ADD Quantity INT NOT NULL
            CONSTRAINT DF_SalesOrder_Quantity DEFAULT 0;
END
GO

UPDATE so
SET so.Quantity = ps.Quantity
FROM dbo.SalesOrder so
INNER JOIN dbo.ProductStock ps ON ps.StockID = so.StockID
WHERE so.Quantity = 0 AND ps.Quantity > 0;
GO
