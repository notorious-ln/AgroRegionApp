using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AgroRegionApp.Data
{
    internal sealed class PurchaseOrderRow
    {
        public int Id { get; set; }
        public byte StatusId { get; set; }
        public DateTime Date { get; set; }
        public string SupplierName { get; set; }
        public string StatusName { get; set; }
        public string SupplierPhone { get; set; }
        public string SupplierEmail { get; set; }

        public string OrderNumber => $"ЗЗ-{Id:D5}";

        public bool CanGenerateDocuments =>
            StatusName == "Оформлен" || StatusName == "Исполнен";
    }

    internal sealed class PurchaseStatusOption
    {
        public byte Id { get; set; }
        public string Name { get; set; }
    }

    internal sealed class SupplierOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    internal static class PurchaseService
    {
        public static List<PurchaseOrderRow> GetOrders()
        {
            const string sql = @"
SELECT po.PurchaseOrderID,
       st.StatusID,
       po.CreationDate,
       s.Name AS SupplierName,
       s.PhoneNumber,
       s.Email,
       st.StatusName
FROM PurchaseOrder po
INNER JOIN Supplier s ON s.SupplierID = po.SupplierID
INNER JOIN PurchaseOrderStatus st ON st.StatusID = po.StatusID
ORDER BY po.CreationDate DESC, po.PurchaseOrderID DESC";

            var list = new List<PurchaseOrderRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new PurchaseOrderRow
                    {
                        Id = reader.GetInt32(0),
                        StatusId = reader.GetByte(1),
                        Date = reader.GetDateTime(2),
                        SupplierName = reader.GetString(3),
                        SupplierPhone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        SupplierEmail = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        StatusName = reader.GetString(6)
                    });
                }
            }

            return list;
        }

        public static List<SupplierOption> GetSuppliers()
        {
            var list = new List<SupplierOption>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("SELECT SupplierID, Name FROM Supplier ORDER BY Name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    list.Add(new SupplierOption { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            }

            return list;
        }

        public static int CreateOrder(int supplierId, int employeeId, byte statusId = 1)
        {
            const string sql = @"
INSERT INTO PurchaseOrder (SupplierID, EmployeeID, StatusID, CreationDate)
VALUES (@SupplierID, @EmployeeID, @StatusID, @Date);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@SupplierID", supplierId);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.Parameters.AddWithValue("@StatusID", statusId);
                cmd.Parameters.AddWithValue("@Date", DateTime.Today);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static List<PurchaseStatusOption> GetStatuses()
        {
            var list = new List<PurchaseStatusOption>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("SELECT StatusID, StatusName FROM PurchaseOrderStatus ORDER BY StatusID", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    list.Add(new PurchaseStatusOption { Id = reader.GetByte(0), Name = reader.GetString(1) });
            }

            return list;
        }

        public static void UpdateOrder(int orderId, byte statusId)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(
                "UPDATE PurchaseOrder SET StatusID = @StatusID WHERE PurchaseOrderID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", orderId);
                cmd.Parameters.AddWithValue("@StatusID", statusId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Заказ не найден.");
            }
        }

        public static void DeleteOrder(int orderId)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM PurchaseOrder WHERE PurchaseOrderID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", orderId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Заказ не найден.");
            }
        }

        public static byte GetDefaultStatusId()
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("SELECT TOP 1 StatusID FROM PurchaseOrderStatus ORDER BY StatusID", conn))
            {
                var result = cmd.ExecuteScalar();
                return result == null ? (byte)1 : Convert.ToByte(result);
            }
        }
    }
}
