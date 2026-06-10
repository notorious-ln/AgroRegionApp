using System;
using System.Collections.Generic;
using System.Linq;

namespace AgroRegionApp.Data
{
    internal sealed class PurchaseLineItem
    {
        public string ProductName { get; set; }
        public int QtyTons { get; set; }
        public int PricePerTon { get; set; }
        public int Total => QtyTons * PricePerTon;
    }

    internal sealed class PurchaseHistoryRow
    {
        public DateTime Date { get; set; }
        public string SupplierName { get; set; }
        public string ProductName { get; set; }
        public int QtyTons { get; set; }
        public int PricePerTon { get; set; }
    }

    internal static class PurchaseMockData
    {
        private static readonly Dictionary<int, List<PurchaseLineItem>> ItemsByOrder = new Dictionary<int, List<PurchaseLineItem>>
        {
            [1] = new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Пшеница 3 кл.", QtyTons = 100, PricePerTon = 4800 },
                new PurchaseLineItem { ProductName = "Ячмень фуражный", QtyTons = 50, PricePerTon = 4000 }
            },
            [2] = new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Кукуруза", QtyTons = 80, PricePerTon = 5200 }
            },
            [3] = new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Ячмень фуражный", QtyTons = 40, PricePerTon = 3900 }
            }
        };

        private static readonly List<PurchaseHistoryRow> History = new List<PurchaseHistoryRow>
        {
            new PurchaseHistoryRow { Date = new DateTime(2026, 6, 1), SupplierName = "ООО \"АгроСнаб\"", ProductName = "Пшеница 3 кл.", QtyTons = 200, PricePerTon = 4800 },
            new PurchaseHistoryRow { Date = new DateTime(2026, 5, 15), SupplierName = "ИП Гришин В.П.", ProductName = "Ячмень фуражный", QtyTons = 80, PricePerTon = 3900 },
            new PurchaseHistoryRow { Date = new DateTime(2026, 5, 10), SupplierName = "ЗАО \"Зерновые ресурсы\"", ProductName = "Кукуруза", QtyTons = 150, PricePerTon = 5200 },
            new PurchaseHistoryRow { Date = new DateTime(2026, 5, 1), SupplierName = "ООО \"АгроСнаб\"", ProductName = "Пшеница 3 кл.", QtyTons = 250, PricePerTon = 4700 },
            new PurchaseHistoryRow { Date = new DateTime(2026, 4, 20), SupplierName = "ЗАО \"Зерновые ресурсы\"", ProductName = "Подсолнечник", QtyTons = 60, PricePerTon = 7800 }
        };

        public static List<PurchaseLineItem> GetItems(int orderId)
        {
            if (ItemsByOrder.TryGetValue(orderId, out var items))
                return items;

            return new List<PurchaseLineItem>
            {
                new PurchaseLineItem { ProductName = "Пшеница 3 кл.", QtyTons = 50, PricePerTon = 4800 }
            };
        }

        public static int GetItemCount(int orderId) => GetItems(orderId).Count;

        public static int GetOrderTotal(int orderId) => GetItems(orderId).Sum(i => i.Total);

        public static List<PurchaseHistoryRow> GetHistory() => History;
    }
}
