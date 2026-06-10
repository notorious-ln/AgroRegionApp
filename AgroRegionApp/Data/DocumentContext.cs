using System;
using System.Linq;
using AgroRegionApp;

namespace AgroRegionApp.Data
{
    internal sealed class DocumentContext
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime Date { get; set; }
        public string CounterpartyName { get; set; }
        public string CounterpartyInn { get; set; }
        public string CounterpartyPhone { get; set; }
        public string ProductName { get; set; }
        public string ProductVariety { get; set; }
        public string WarehouseName { get; set; }
        public int QuantityTons { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OrderTotal { get; set; }
        public bool IsPurchase { get; set; }

        public string ProductFull
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProductVariety) || ProductVariety == "—")
                    return ProductName ?? "—";
                return ProductName + ", " + ProductVariety;
            }
        }

        public string CounterpartyInnSafe =>
            string.IsNullOrWhiteSpace(CounterpartyInn) ? "—" : CounterpartyInn;

        public string CounterpartyLine
        {
            get
            {
                var line = CounterpartyName ?? "—";
                if (!string.IsNullOrWhiteSpace(CounterpartyInn))
                    line += ", ИНН " + CounterpartyInn;
                if (!string.IsNullOrWhiteSpace(CounterpartyPhone))
                    line += ", тел. " + CounterpartyPhone;
                return line;
            }
        }

        public string TtnNumber => "ТТН-" + Id.ToString("D5");
        public string ContractLabel => "Договор поставки № " + OrderNumber;
        public string SumFormatted => OrderTotal.ToString("N0");
        public string UnitPriceFormatted => UnitPrice.ToString("G29");
        public string DateFormatted => Date.ToString("dd.MM.yyyy");
        public string UnitPriceLabel => IsPurchase ? "₽/т" : "₽/кг";

        public string SupplierLine => IsPurchase ? CounterpartyLine : CompanyProfile.OrganizationLine;
        public string BuyerLine => IsPurchase ? CompanyProfile.OrganizationLine : CounterpartyLine;

        public static DocumentContext FromSales(SalesOrderRow order)
        {
            return new DocumentContext
            {
                IsPurchase = false,
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Date = order.Date,
                CounterpartyName = order.CustomerName,
                CounterpartyInn = order.CustomerInn,
                CounterpartyPhone = order.CustomerPhone,
                ProductName = order.ProductName,
                ProductVariety = order.ProductVariety,
                WarehouseName = order.WarehouseName,
                QuantityTons = order.QuantityTons,
                UnitPrice = order.PricePerKg,
                OrderTotal = order.OrderTotal
            };
        }

        public static DocumentContext FromPurchase(PurchaseOrderRow order)
        {
            var items = PurchaseMockData.GetItems(order.Id);
            var first = items.FirstOrDefault();
            return new DocumentContext
            {
                IsPurchase = true,
                Id = order.Id,
                OrderNumber = $"ЗЗ-{order.Id:D5}",
                Date = order.Date,
                CounterpartyName = order.SupplierName,
                CounterpartyPhone = order.SupplierPhone,
                ProductName = first?.ProductName ?? "—",
                ProductVariety = "—",
                WarehouseName = "—",
                QuantityTons = items.Sum(i => i.QtyTons),
                UnitPrice = first?.PricePerTon ?? 0,
                OrderTotal = PurchaseMockData.GetOrderTotal(order.Id)
            };
        }
    }
}
