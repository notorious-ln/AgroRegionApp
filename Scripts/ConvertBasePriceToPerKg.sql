USE AgroCompany;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Product') AND name = N'BasePrice')
BEGIN
    UPDATE dbo.Product
    SET BasePrice = CASE
        WHEN BasePrice >= 1000 THEN BasePrice / 1000
        WHEN Name = N'Пшеница 3 кл.' THEN 5
        WHEN Name = N'Ячмень фуражный' THEN 4
        WHEN Name = N'Кукуруза' THEN 5
        WHEN Name = N'Подсолнечник' THEN 8
        WHEN Name = NCHAR(1071)+NCHAR(1073)+NCHAR(1083)+NCHAR(1086)+NCHAR(1082)+NCHAR(1086) THEN 35
        ELSE BasePrice
    END
    WHERE BasePrice > 100 OR Name IN (
        N'Пшеница 3 кл.', N'Ячмень фуражный', N'Кукуруза', N'Подсолнечник',
        NCHAR(1071)+NCHAR(1073)+NCHAR(1083)+NCHAR(1086)+NCHAR(1082)+NCHAR(1086));
END
GO
