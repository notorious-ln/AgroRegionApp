using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AgroRegionApp.Data
{
    internal sealed class SalesOrderRow
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public decimal PricePerKg { get; set; }
        public int StockQuantity { get; set; }
        public string StatusName { get; set; }
    }

    internal static class SalesService
    {
        public static List<SalesOrderRow> GetOrders()
        {
            const string sql = @"
SELECT so.SalesOrderID,
       so.CreationDate,
       c.Name,
       p.Name,
       w.Name,
       so.PricePerKg,
       ISNULL(ps.Quantity, 0),
       st.StatusName
FROM SalesOrder so
INNER JOIN Customer c ON c.CustomerID = so.CustomerID
INNER JOIN SalesOrderStatus st ON st.StatusID = so.StatusID
LEFT JOIN ProductStock ps ON ps.StockID = so.StockID
LEFT JOIN Product p ON p.ProductID = ps.ProductID
LEFT JOIN Warehouse w ON w.WarehouseID = ps.WarehouseID
ORDER BY so.CreationDate DESC, so.SalesOrderID DESC";

            var list = new List<SalesOrderRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new SalesOrderRow
                    {
                        Id = reader.GetInt32(0),
                        Date = reader.GetDateTime(1),
                        CustomerName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        ProductName = reader.IsDBNull(3) ? "—" : reader.GetString(3),
                        WarehouseName = reader.IsDBNull(4) ? "—" : reader.GetString(4),
                        PricePerKg = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        StockQuantity = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        StatusName = reader.IsDBNull(7) ? "" : reader.GetString(7)
                    });
                }
            }

            return list;
        }
    }
}
