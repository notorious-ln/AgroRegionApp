using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AgroRegionApp.Data
{
    internal sealed class SalesOrderRow
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int StockId { get; set; }
        public byte StatusId { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerInn { get; set; }
        public int CustomerDebt { get; set; }
        public string ProductName { get; set; }
        public string ProductVariety { get; set; }
        public string WarehouseName { get; set; }
        public decimal PricePerKg { get; set; }
        public int QuantityTons { get; set; }
        public string StatusName { get; set; }
        public bool StockConfirmed { get; set; }

        public string OrderNumber => $"ЗП-{Id:D5}";
        public decimal OrderTotal => PricePerKg * QuantityTons;

        public bool CanGenerateDocuments =>
            (StatusName == "Подтверждён" || StatusName == "Готов к отгрузке") && StockConfirmed;
    }

    internal sealed class SalesStatusOption
    {
        public byte Id { get; set; }
        public string Name { get; set; }
    }

    internal static class SalesService
    {
        public static List<SalesOrderRow> GetOrders()
        {
            const string sql = @"
SELECT so.SalesOrderID,
       so.CustomerID,
       so.StockID,
       st.StatusID,
       so.CreationDate,
       c.Name,
       ISNULL(c.PhoneNumber, N''),
       ISNULL(c.INN, N''),
       ISNULL(c.DebtAmount, 0),
       ISNULL(p.Name, N'—'),
       ISNULL(p.Variety, N'—'),
       ISNULL(w.Name, N'—'),
       so.PricePerKg,
       ISNULL(so.Quantity, 0),
       st.StatusName,
       ISNULL(so.StockConfirmed, 0)
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
                        CustomerId = reader.GetInt32(1),
                        StockId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                        StatusId = reader.GetByte(3),
                        Date = reader.GetDateTime(4),
                        CustomerName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        CustomerPhone = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        CustomerInn = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        CustomerDebt = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                        ProductName = reader.IsDBNull(9) ? "—" : reader.GetString(9),
                        ProductVariety = reader.IsDBNull(10) ? "—" : reader.GetString(10),
                        WarehouseName = reader.IsDBNull(11) ? "—" : reader.GetString(11),
                        PricePerKg = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12),
                        QuantityTons = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                        StatusName = reader.IsDBNull(14) ? "" : reader.GetString(14),
                        StockConfirmed = !reader.IsDBNull(15) && reader.GetBoolean(15)
                    });
                }
            }

            return list;
        }

        public static int GetStockQuantity(int stockId)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("SELECT Quantity FROM ProductStock WHERE StockID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", stockId);
                var result = cmd.ExecuteScalar();
                return result == null ? 0 : Convert.ToInt32(result);
            }
        }

        public static List<SalesStatusOption> GetStatuses()
        {
            var list = new List<SalesStatusOption>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("SELECT StatusID, StatusName FROM SalesOrderStatus ORDER BY StatusID", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    list.Add(new SalesStatusOption { Id = reader.GetByte(0), Name = reader.GetString(1) });
            }

            return list;
        }

        public static void UpdateOrder(int orderId, int stockId, byte statusId, int quantityTons, decimal pricePerKg)
        {
            if (quantityTons <= 0)
                throw new InvalidOperationException("Укажите количество больше нуля.");

            using (var conn = Db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int available;
                    using (var checkCmd = new SqlCommand("SELECT Quantity FROM ProductStock WHERE StockID = @Id", conn, tx))
                    {
                        checkCmd.Parameters.AddWithValue("@Id", stockId);
                        var result = checkCmd.ExecuteScalar();
                        if (result == null)
                            throw new InvalidOperationException("Остаток на складе не найден.");
                        available = Convert.ToInt32(result);
                    }

                    if (quantityTons > available)
                        throw new InvalidOperationException($"Недостаточно остатка на складе. Доступно: {available} т.");

                    const string sql = @"
UPDATE SalesOrder
SET StatusID = @StatusID, Quantity = @Quantity, PricePerKg = @PricePerKg
WHERE SalesOrderID = @Id";

                    using (var cmd = new SqlCommand(sql, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Id", orderId);
                        cmd.Parameters.AddWithValue("@StatusID", statusId);
                        cmd.Parameters.AddWithValue("@Quantity", quantityTons);
                        cmd.Parameters.AddWithValue("@PricePerKg", pricePerKg);
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException("Заказ не найден.");
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static void DeleteOrder(int orderId)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM SalesOrder WHERE SalesOrderID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", orderId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Заказ не найден.");
            }
        }

        public static void ConfirmStock(int orderId)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("UPDATE SalesOrder SET StockConfirmed = 1 WHERE SalesOrderID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", orderId);
                cmd.ExecuteNonQuery();
            }
        }

        public static int CreateOrder(int customerId, int employeeId, int stockId, int quantityTons, decimal pricePerKg, bool stockConfirmed, byte statusId = 1)
        {
            if (quantityTons <= 0)
                throw new InvalidOperationException("Укажите количество больше нуля.");

            using (var conn = Db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int available;
                    using (var checkCmd = new SqlCommand("SELECT Quantity FROM ProductStock WHERE StockID = @Id", conn, tx))
                    {
                        checkCmd.Parameters.AddWithValue("@Id", stockId);
                        var result = checkCmd.ExecuteScalar();
                        if (result == null)
                            throw new InvalidOperationException("Остаток на складе не найден.");
                        available = Convert.ToInt32(result);
                    }

                    if (quantityTons > available)
                        throw new InvalidOperationException($"Недостаточно остатка на складе. Доступно: {available} т.");

                    const string sql = @"
INSERT INTO SalesOrder (CustomerID, EmployeeID, StockID, StatusID, CreationDate, PricePerKg, Quantity, StockConfirmed)
VALUES (@CustomerID, @EmployeeID, @StockID, @StatusID, @Date, @PricePerKg, @Quantity, @StockConfirmed);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int newId;
                    using (var cmd = new SqlCommand(sql, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerId);
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                        cmd.Parameters.AddWithValue("@StockID", stockId);
                        cmd.Parameters.AddWithValue("@StatusID", statusId);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Today);
                        cmd.Parameters.AddWithValue("@PricePerKg", pricePerKg);
                        cmd.Parameters.AddWithValue("@Quantity", quantityTons);
                        cmd.Parameters.AddWithValue("@StockConfirmed", stockConfirmed);
                        newId = (int)cmd.ExecuteScalar();
                    }

                    tx.Commit();
                    return newId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static byte GetDefaultStatusId()
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("SELECT TOP 1 StatusID FROM SalesOrderStatus ORDER BY StatusID", conn))
            {
                var result = cmd.ExecuteScalar();
                return result == null ? (byte)1 : Convert.ToByte(result);
            }
        }
    }
}
