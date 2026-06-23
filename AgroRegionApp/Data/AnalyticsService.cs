using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AgroRegionApp.Data
{
    internal sealed class AnalyticsPeriod
    {
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public string DisplayName { get; set; }

        public bool IsFullYear => !Quarter.HasValue;
    }

    internal sealed class MonthlyAmountRow
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Amount { get; set; }
        public int QuantityTons { get; set; }
        public int Count { get; set; }
    }

    internal sealed class StockPivotRow
    {
        public string ProductName { get; set; }
        public int Warehouse1Tons { get; set; }
        public int Warehouse2Tons { get; set; }
        public int TotalTons { get; set; }
        public DateTime? CheckDate { get; set; }

        public string Status =>
            TotalTons == 0 ? "Нет в наличии" : TotalTons < 30 ? "Мало" : "В наличии";
    }

    internal sealed class DebtorRow
    {
        public string CustomerName { get; set; }
        public int OrderCount { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DebtAmount { get; set; }

        public string Status => DebtAmount > 0 ? "Есть долг" : "Оплачено";
    }

    internal sealed class AnalyticsSummary
    {
        public string PeriodLabel { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public int TotalStockTons { get; set; }
        public decimal TotalDebt { get; set; }
        public List<MonthlyAmountRow> SalesByMonth { get; set; } = new List<MonthlyAmountRow>();
        public List<MonthlyAmountRow> PurchasesByMonth { get; set; } = new List<MonthlyAmountRow>();
        public List<StockPivotRow> Stocks { get; set; } = new List<StockPivotRow>();
        public List<DebtorRow> Debtors { get; set; } = new List<DebtorRow>();
    }

    internal static class AnalyticsService
    {
        private static readonly string[] MonthNames =
            { "", "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

        private static readonly string[] QuarterNames =
            { "", "I кв.", "II кв.", "III кв.", "IV кв." };

        public static List<AnalyticsPeriod> GetAvailablePeriods()
        {
            var list = new List<AnalyticsPeriod>();
            foreach (var year in new[] { 2026, 2025 })
            {
                list.Add(new AnalyticsPeriod
                {
                    Year = year,
                    DisplayName = year + " год"
                });

                var maxQuarter = year == DateTime.Now.Year
                    ? (DateTime.Now.Month - 1) / 3 + 1
                    : 4;
                for (var quarter = 1; quarter <= maxQuarter; quarter++)
                {
                    list.Add(new AnalyticsPeriod
                    {
                        Year = year,
                        Quarter = quarter,
                        DisplayName = QuarterNames[quarter] + " " + year
                    });
                }
            }

            return list;
        }

        public static AnalyticsSummary Load(AnalyticsPeriod period)
        {
            if (period == null)
                throw new ArgumentNullException(nameof(period));

            var summary = new AnalyticsSummary
            {
                PeriodLabel = period.DisplayName,
                SalesByMonth = LoadSalesByMonth(period),
                PurchasesByMonth = LoadPurchasesByMonth(period),
                Stocks = LoadStocksPivot(),
                Debtors = LoadDebtors(period)
            };

            summary.TotalSales = summary.SalesByMonth.Sum(r => r.Amount);
            summary.TotalPurchases = summary.PurchasesByMonth.Sum(r => r.Amount);
            summary.TotalStockTons = summary.Stocks.Sum(r => r.TotalTons);
            summary.TotalDebt = LoadTotalDebt();
            return summary;
        }

        private static decimal LoadTotalDebt()
        {
            const string sql = @"
SELECT ISNULL(SUM(DebtAmount), 0)
FROM Customer
WHERE ISNULL(DebtAmount, 0) > 0";
            try
            {
                using (var conn = Db.OpenConnection())
                using (var cmd = new SqlCommand(sql, conn))
                    return Convert.ToDecimal(cmd.ExecuteScalar());
            }
            catch
            {
                return 0;
            }
        }

        private static List<MonthlyAmountRow> LoadSalesByMonth(AnalyticsPeriod period)
        {
            const string sql = @"
SELECT MONTH(so.CreationDate) AS M,
       COUNT(*) AS Cnt,
       ISNULL(SUM(so.PricePerKg * ISNULL(so.Quantity, 0) * 1000), 0) AS Amount,
       ISNULL(SUM(ISNULL(so.Quantity, 0)), 0) AS Qty
FROM SalesOrder so
WHERE YEAR(so.CreationDate) = @Year
  AND (@Quarter IS NULL OR DATEPART(QUARTER, so.CreationDate) = @Quarter)
GROUP BY MONTH(so.CreationDate)
ORDER BY M";

            return FillMonthlyGaps(LoadMonthlyFromDb(sql, period, true), period);
        }

        private static List<MonthlyAmountRow> LoadPurchasesByMonth(AnalyticsPeriod period)
        {
            var byMonth = PurchaseMockData.GetHistory()
                .Where(h => h.Date.Year == period.Year)
                .Where(h => !period.Quarter.HasValue || ((h.Date.Month - 1) / 3 + 1) == period.Quarter.Value)
                .GroupBy(h => h.Date.Month)
                .ToDictionary(
                    g => g.Key,
                    g => new MonthlyAmountRow
                    {
                        Month = g.Key,
                        MonthName = MonthNames[g.Key],
                        Count = g.Count(),
                        Amount = g.Sum(h => (decimal)h.QtyTons * 1000m * h.PricePerKg)
                    });

            const string sql = @"
SELECT MONTH(po.CreationDate) AS M,
       po.PurchaseOrderID
FROM PurchaseOrder po
WHERE YEAR(po.CreationDate) = @Year
  AND (@Quarter IS NULL OR DATEPART(QUARTER, po.CreationDate) = @Quarter)
ORDER BY M";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Year", period.Year);
                cmd.Parameters.AddWithValue("@Quarter", (object)period.Quarter ?? DBNull.Value);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var month = reader.GetInt32(0);
                        var orderId = reader.GetInt32(1);
                        if (!byMonth.TryGetValue(month, out var row))
                        {
                            row = new MonthlyAmountRow { Month = month, MonthName = MonthNames[month] };
                            byMonth[month] = row;
                        }

                        row.Count++;
                        row.Amount += PurchaseMockData.GetOrderTotal(orderId);
                    }
                }
            }

            return FillMonthlyGaps(byMonth.Values.ToList(), period);
        }

        private static List<MonthlyAmountRow> LoadMonthlyFromDb(string sql, AnalyticsPeriod period, bool includeQty)
        {
            var list = new List<MonthlyAmountRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Year", period.Year);
                cmd.Parameters.AddWithValue("@Quarter", (object)period.Quarter ?? DBNull.Value);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var month = reader.GetInt32(0);
                        list.Add(new MonthlyAmountRow
                        {
                            Month = month,
                            MonthName = MonthNames[month],
                            Count = reader.GetInt32(1),
                            Amount = reader.GetDecimal(2),
                            QuantityTons = includeQty && !reader.IsDBNull(3) ? reader.GetInt32(3) : 0
                        });
                    }
                }
            }

            return list;
        }

        private static (int StartMonth, int EndMonth) GetMonthRange(AnalyticsPeriod period)
        {
            if (period.IsFullYear)
            {
                var last = period.Year == DateTime.Now.Year ? DateTime.Now.Month : 12;
                return (1, last);
            }

            var quarter = period.Quarter.Value;
            var start = (quarter - 1) * 3 + 1;
            var lastMonth = quarter * 3;
            if (period.Year == DateTime.Now.Year)
                lastMonth = Math.Min(lastMonth, DateTime.Now.Month);
            return (start, lastMonth);
        }

        private static List<MonthlyAmountRow> FillMonthlyGaps(List<MonthlyAmountRow> rows, AnalyticsPeriod period)
        {
            var map = new Dictionary<int, MonthlyAmountRow>();
            foreach (var row in rows)
                map[row.Month] = row;

            var range = GetMonthRange(period);
            var result = new List<MonthlyAmountRow>();
            for (var month = range.StartMonth; month <= range.EndMonth; month++)
            {
                if (map.TryGetValue(month, out var row))
                    result.Add(row);
                else
                    result.Add(new MonthlyAmountRow { Month = month, MonthName = MonthNames[month] });
            }

            return result;
        }

        private static List<StockPivotRow> LoadStocksPivot()
        {
            const string sql = @"
SELECT p.Name,
       ISNULL(SUM(CASE WHEN w.Name LIKE N'Склад №1%' THEN ps.Quantity ELSE 0 END), 0) AS Wh1,
       ISNULL(SUM(CASE WHEN w.Name LIKE N'Склад №2%' THEN ps.Quantity ELSE 0 END), 0) AS Wh2,
       ISNULL(SUM(ps.Quantity), 0) AS Total,
       MAX(ps.CheckDate) AS CheckDate
FROM Product p
LEFT JOIN ProductStock ps ON ps.ProductID = p.ProductID
LEFT JOIN Warehouse w ON w.WarehouseID = ps.WarehouseID
GROUP BY p.ProductID, p.Name
ORDER BY p.Name";

            var list = new List<StockPivotRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new StockPivotRow
                    {
                        ProductName = reader.GetString(0),
                        Warehouse1Tons = reader.GetInt32(1),
                        Warehouse2Tons = reader.GetInt32(2),
                        TotalTons = reader.GetInt32(3),
                        CheckDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4)
                    });
                }
            }

            return list;
        }

        private static List<DebtorRow> LoadDebtors(AnalyticsPeriod period)
        {
            const string sql = @"
SELECT c.Name,
       COUNT(so.SalesOrderID) AS OrderCount,
       ISNULL(SUM(so.PricePerKg * ISNULL(so.Quantity, 0) * 1000), 0) AS OrderTotal,
       ISNULL(c.DebtAmount, 0) AS DebtAmount
FROM Customer c
LEFT JOIN SalesOrder so ON so.CustomerID = c.CustomerID
    AND YEAR(so.CreationDate) = @Year
    AND (@Quarter IS NULL OR DATEPART(QUARTER, so.CreationDate) = @Quarter)
GROUP BY c.CustomerID, c.Name, c.DebtAmount
HAVING COUNT(so.SalesOrderID) > 0 OR ISNULL(c.DebtAmount, 0) > 0
ORDER BY DebtAmount DESC, c.Name";

            var list = new List<DebtorRow>();
            try
            {
                using (var conn = Db.OpenConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Year", period.Year);
                    cmd.Parameters.AddWithValue("@Quarter", (object)period.Quarter ?? DBNull.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var total = reader.GetDecimal(2);
                            var debt = reader.GetDecimal(3);
                            list.Add(new DebtorRow
                            {
                                CustomerName = reader.GetString(0),
                                OrderCount = reader.GetInt32(1),
                                OrderTotal = total,
                                DebtAmount = debt,
                                PaidAmount = total - debt
                            });
                        }
                    }
                }
            }
            catch
            {
                // DebtAmount column may be missing before migration script is applied.
            }

            return list;
        }

        public static string FormatMoney(decimal value) =>
            value.ToString("N0").Replace('\u00A0', ' ') + " ₽";

        public static string FormatMoneyShort(decimal value)
        {
            if (value >= 1_000_000m)
                return $"{value / 1_000_000m:0.00} млн ₽";
            return FormatMoney(value);
        }
    }
}
