using System;
using System.Collections.Generic;
using System.Linq;

namespace AgroRegionApp.Data
{
    internal sealed class PurchaseLineItem
    {
        public string ProductName { get; set; }
        public int QtyTons { get; set; }
        public decimal PricePerKg { get; set; }
        public decimal Total => QtyTons * 1000m * PricePerKg;
    }

    internal sealed class PurchaseHistoryRow
    {
        public DateTime Date { get; set; }
        public string SupplierName { get; set; }
        public string ProductName { get; set; }
        public int QtyTons { get; set; }
        public decimal PricePerKg { get; set; }
    }

    internal static class PurchaseMockData
    {
        private static readonly Dictionary<int, List<PurchaseLineItem>> ItemsByOrder = new Dictionary<int, List<PurchaseLineItem>>
        {
            [1] = new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Пшеница 3 кл.", QtyTons = 100, PricePerKg = 4.8m },
                new PurchaseLineItem { ProductName = "Ячмень фуражный", QtyTons = 50, PricePerKg = 4.0m }
            },
            [2] = new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Кукуруза", QtyTons = 80, PricePerKg = 5.2m }
            },
            [3] = new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Ячмень фуражный", QtyTons = 40, PricePerKg = 3.9m }
            }
        };

        private static readonly List<PurchaseHistoryRow> History = new List<PurchaseHistoryRow>
        {
            new PurchaseHistoryRow { Date = new DateTime(2026, 6, 1), SupplierName = "ООО \"АгроСнаб\"", ProductName = "Пшеница 3 кл.", QtyTons = 200, PricePerKg = 4.8m },
            new PurchaseHistoryRow { Date = new DateTime(2026, 5, 15), SupplierName = "ИП Гришин В.П.", ProductName = "Ячмень фуражный", QtyTons = 80, PricePerKg = 3.9m },
            new PurchaseHistoryRow { Date = new DateTime(2026, 5, 10), SupplierName = "ЗАО \"Зерновые ресурсы\"", ProductName = "Кукуруза", QtyTons = 150, PricePerKg = 5.2m },
            new PurchaseHistoryRow { Date = new DateTime(2026, 5, 1), SupplierName = "ООО \"АгроСнаб\"", ProductName = "Пшеница 3 кл.", QtyTons = 250, PricePerKg = 4.7m },
            new PurchaseHistoryRow { Date = new DateTime(2026, 4, 20), SupplierName = "ЗАО \"Зерновые ресурсы\"", ProductName = "Подсолнечник", QtyTons = 60, PricePerKg = 7.8m }
        };

        public static List<PurchaseLineItem> GetItems(int orderId)
        {
            if (ItemsByOrder.TryGetValue(orderId, out var items))
                return items;

            return new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Пшеница 3 кл.", QtyTons = 50, PricePerKg = 4.8m }
            };
        }

        public static int GetItemCount(int orderId) => GetItems(orderId).Count;

        public static decimal GetOrderTotal(int orderId) => GetItems(orderId).Sum(i => i.Total);

        public static List<PurchaseHistoryRow> GetHistory() => History;
    }
}
