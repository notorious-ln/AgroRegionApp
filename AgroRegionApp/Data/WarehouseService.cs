using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AgroRegionApp.Data
{
    internal sealed class StockRow
    {
        public int StockId { get; set; }
        public string ProductName { get; set; }
        public string Variety { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity { get; set; }
        public DateTime? CheckDate { get; set; }
    }

    internal sealed class ShipmentOrderRow
    {
        public string OrderNumber { get; set; }
        public DateTime Date { get; set; }
        public string PartyName { get; set; }
        public string OrderType { get; set; }
        public string StatusName { get; set; }
        public string ProductName { get; set; }
        public int QtyTons { get; set; }
    }

    internal static class WarehouseService
    {
        public static List<StockRow> GetStocks()
        {
            const string sql = @"
SELECT ps.StockID, p.Name, p.Variety, w.Name, ps.Quantity, ps.CheckDate
FROM ProductStock ps
INNER JOIN Product p ON p.ProductID = ps.ProductID
INNER JOIN Warehouse w ON w.WarehouseID = ps.WarehouseID
ORDER BY p.Name, w.Name";

            var list = new List<StockRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new StockRow
                    {
                        StockId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Variety = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        WarehouseName = reader.GetString(3),
                        Quantity = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        CheckDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                    });
                }
            }

            return list;
        }

        public static List<ShipmentOrderRow> GetShipmentOrders()
        {
            const string sql = @"
SELECT 'ЗП-' + RIGHT('00000' + CAST(so.SalesOrderID AS VARCHAR(5)), 5),
       so.CreationDate,
       c.Name,
       N'Продажа',
       st.StatusName,
       ISNULL(p.Name, N'—'),
       ISNULL(ps.Quantity, 0)
FROM SalesOrder so
INNER JOIN Customer c ON c.CustomerID = so.CustomerID
INNER JOIN SalesOrderStatus st ON st.StatusID = so.StatusID
LEFT JOIN ProductStock ps ON ps.StockID = so.StockID
LEFT JOIN Product p ON p.ProductID = ps.ProductID
WHERE st.StatusName IN (N'Подтверждён', N'Готов к отгрузке', N'Новый')

UNION ALL

SELECT 'ЗЗ-' + RIGHT('00000' + CAST(po.PurchaseOrderID AS VARCHAR(5)), 5),
       po.CreationDate,
       s.Name,
       N'Закупка',
       st.StatusName,
       ISNULL(p.Name, N'—'),
       ISNULL(ps.Quantity, 50)
FROM PurchaseOrder po
INNER JOIN Supplier s ON s.SupplierID = po.SupplierID
INNER JOIN PurchaseOrderStatus st ON st.StatusID = po.StatusID
LEFT JOIN ProductStock ps ON ps.StockID = (
    SELECT TOP 1 StockID FROM ProductStock ORDER BY StockID)
LEFT JOIN Product p ON p.ProductID = ps.ProductID
WHERE st.StatusName IN (N'Оформлен', N'Исполнен')

ORDER BY 2 DESC";

            var list = new List<ShipmentOrderRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new ShipmentOrderRow
                    {
                        OrderNumber = reader.GetString(0),
                        Date = reader.GetDateTime(1),
                        PartyName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        OrderType = reader.GetString(3),
                        StatusName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        ProductName = reader.IsDBNull(5) ? "—" : reader.GetString(5),
                        QtyTons = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
                    });
                }
            }

            return list;
        }

        public static void UpdateStock(int stockId, int quantity)
        {
            const string sql = @"
UPDATE ProductStock SET Quantity = @Qty, CheckDate = @Date WHERE StockID = @Id";
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Qty", quantity);
                cmd.Parameters.AddWithValue("@Date", DateTime.Today);
                cmd.Parameters.AddWithValue("@Id", stockId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
