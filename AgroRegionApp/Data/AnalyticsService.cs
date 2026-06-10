using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AgroRegionApp.Data
{
    internal sealed class MonthlyAmountRow
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    internal sealed class StockSummaryRow
    {
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity { get; set; }
        public DateTime? CheckDate { get; set; }
    }

    internal sealed class AnalyticsSummary
    {
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public int TotalStockTons { get; set; }
        public int OrderCount { get; set; }
        public List<MonthlyAmountRow> SalesByMonth { get; set; } = new List<MonthlyAmountRow>();
        public List<MonthlyAmountRow> PurchasesByMonth { get; set; } = new List<MonthlyAmountRow>();
        public List<StockSummaryRow> Stocks { get; set; } = new List<StockSummaryRow>();
    }

    internal static class AnalyticsService
    {
        private static readonly string[] MonthNames =
            { "", "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

        public static AnalyticsSummary Load(int year)
        {
            var summary = new AnalyticsSummary();
            summary.SalesByMonth = LoadSalesByMonth(year, summary);
            summary.PurchasesByMonth = LoadPurchasesByMonth(year);
            summary.Stocks = LoadStocks(summary);
            summary.TotalSales = 0;
            foreach (var row in summary.SalesByMonth)
                summary.TotalSales += row.Amount;
            summary.TotalPurchases = 0;
            foreach (var row in summary.PurchasesByMonth)
                summary.TotalPurchases += row.Amount;
            return summary;
        }

        private static List<MonthlyAmountRow> LoadSalesByMonth(int year, AnalyticsSummary summary)
        {
            const string sql = @"
SELECT MONTH(so.CreationDate) AS M,
       COUNT(*) AS Cnt,
       ISNULL(SUM(so.PricePerKg * ISNULL(ps.Quantity, 0)), 0) AS Amount
FROM SalesOrder so
LEFT JOIN ProductStock ps ON ps.StockID = so.StockID
WHERE YEAR(so.CreationDate) = @Year
GROUP BY MONTH(so.CreationDate)
ORDER BY M";

            return LoadMonthly(sql, year, summary, true);
        }

        private static List<MonthlyAmountRow> LoadPurchasesByMonth(int year)
        {
            const string sql = @"
SELECT MONTH(po.CreationDate) AS M,
       COUNT(*) AS Cnt,
       CAST(COUNT(*) * 100000 AS DECIMAL(18,2)) AS Amount
FROM PurchaseOrder po
WHERE YEAR(po.CreationDate) = @Year
GROUP BY MONTH(po.CreationDate)
ORDER BY M";

            return LoadMonthly(sql, year, null, false);
        }

        private static List<MonthlyAmountRow> LoadMonthly(string sql, int year, AnalyticsSummary summary, bool countOrders)
        {
            var list = new List<MonthlyAmountRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Year", year);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var month = reader.GetInt32(0);
                        var row = new MonthlyAmountRow
                        {
                            Month = month,
                            MonthName = MonthNames[month],
                            Count = reader.GetInt32(1),
                            Amount = reader.GetDecimal(2)
                        };
                        list.Add(row);
                        if (countOrders && summary != null)
                            summary.OrderCount += row.Count;
                    }
                }
            }

            return list;
        }

        private static List<StockSummaryRow> LoadStocks(AnalyticsSummary summary)
        {
            const string sql = @"
SELECT p.Name, w.Name, ps.Quantity, ps.CheckDate
FROM ProductStock ps
INNER JOIN Product p ON p.ProductID = ps.ProductID
INNER JOIN Warehouse w ON w.WarehouseID = ps.WarehouseID
ORDER BY p.Name, w.Name";

            var list = new List<StockSummaryRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var qty = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    list.Add(new StockSummaryRow
                    {
                        ProductName = reader.GetString(0),
                        WarehouseName = reader.GetString(1),
                        Quantity = qty,
                        CheckDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3)
                    });
                    summary.TotalStockTons += qty;
                }
            }

            return list;
        }
    }
}
