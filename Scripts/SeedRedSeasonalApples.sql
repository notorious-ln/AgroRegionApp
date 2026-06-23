-- Run via PowerShell to preserve Cyrillic:
--   powershell -ExecutionPolicy Bypass -File Scripts\FixAppleEncoding.ps1
--
-- sqlcmd may corrupt UTF-8 Cyrillic unless the file is saved as UTF-16 LE with BOM.

USE AgroCompany;
GO

-- Красные сорта яблока
IF NOT EXISTS (SELECT 1 FROM Product WHERE Name = N'Яблоко' AND Variety = N'Джонатан')
    INSERT INTO Product (Name, Variety, Seasonality) VALUES
        (N'Яблоко', N'Джонатан',       N'Осень-2026'),
        (N'Яблоко', N'Лобо',           N'Осень-2026'),
        (N'Яблоко', N'Айдаред',        N'Осень-2026'),
        (N'Яблоко', N'Старкримсон',    N'Осень-2026'),
        (N'Яблоко', N'Джонаголд',      N'Осень-2026'),
        (N'Яблоко', N'Гала',           N'Осень-2026'),
        (N'Яблоко', N'Фуджи',          N'Осень-2026'),
        (N'Яблоко', N'Ред Делишес',    N'Осень-2026'),
        (N'Яблоко', N'Жигулевское',    N'Осень-2026'),
        (N'Яблоко', N'Имрус',          N'Осень-2026'),
        (N'Яблоко', N'Алое на серебре',N'Осень-2026'),
        (N'Яблоко', N'Старкин',        N'Осень-2026'),
        (N'Яблоко', N'Кортланд',       N'Осень-2026'),
        (N'Яблоко', N'Мелба',          N'Лето-2026'),
        (N'Яблоко', N'Белый налив',    N'Лето-2026');
GO

-- Сезонные сорта яблока (не хранимые)
IF NOT EXISTS (SELECT 1 FROM Product WHERE Name = N'Яблоко' AND Variety = N'Летнее полосатое')
    INSERT INTO Product (Name, Variety, Seasonality) VALUES
        (N'Яблоко', N'Летнее полосатое',  N'Лето-2026'),
        (N'Яблоко', N'Осеннее полосатое', N'Осень-2026'),
        (N'Яблоко', N'Пепин шафранный',   N'Осень-2026'),
        (N'Яблоко', N'Московское золотое',N'Осень-2026'),
        (N'Яблоко', N'Подарок Мичурина',  N'Осень-2026'),
        (N'Яблоко', N'Ранетка',           N'Осень-2026'),
        (N'Яблоко', N'Антоновка',         N'Осень-2026'),
        (N'Яблоко', N'Боровинка',         N'Лето-2026'),
        (N'Яблоко', N'Китайка золотистая',N'Осень-2026'),
        (N'Яблоко', N'Пармен зимний',     N'Осень-2026');
GO

-- Остатки на складах для новых позиций номенклатуры
INSERT INTO ProductStock (ProductID, WarehouseID, Quantity, CheckDate)
SELECT p.ProductID,
       CASE WHEN ABS(CHECKSUM(p.ProductID)) % 2 = 0 THEN 1 ELSE 2 END,
       10 + (ABS(CHECKSUM(p.ProductID, p.Variety)) % 41),
       CAST(GETDATE() AS DATE)
FROM Product p
WHERE p.Name = N'Яблоко'
  AND NOT EXISTS (
      SELECT 1 FROM ProductStock ps WHERE ps.ProductID = p.ProductID
  );
GO
