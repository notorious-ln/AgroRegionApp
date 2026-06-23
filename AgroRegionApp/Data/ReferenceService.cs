using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AgroRegionApp.Data
{
    internal sealed class CustomerRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Inn { get; set; }
    }

    internal sealed class SupplierRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Inn { get; set; }
        public string Address { get; set; }
    }

    internal sealed class ProductRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Variety { get; set; }
        public string Seasonality { get; set; }
        public string Unit { get; set; }
        public int BasePrice { get; set; }
    }

    internal sealed class CounterpartyEntry
    {
        public bool IsCustomer { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Inn { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public string Type => IsCustomer ? "Покупатель" : "Поставщик";
    }

    internal static class ReferenceService
    {
        private static bool? _supplierExtended;
        private static bool? _productExtended;

        private static bool SupplierHasExtendedFields()
        {
            if (!_supplierExtended.HasValue)
                _supplierExtended = HasColumn("Supplier", "INN");
            return _supplierExtended.Value;
        }

        private static bool ProductHasExtendedFields()
        {
            if (!_productExtended.HasValue)
                _productExtended = HasColumn("Product", "Unit");
            return _productExtended.Value;
        }

        private static bool HasColumn(string table, string column)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(
                "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @Table AND COLUMN_NAME = @Column", conn))
            {
                cmd.Parameters.AddWithValue("@Table", table);
                cmd.Parameters.AddWithValue("@Column", column);
                return cmd.ExecuteScalar() != null;
            }
        }

        public static List<CounterpartyEntry> GetCounterparties()
        {
            var list = new List<CounterpartyEntry>();
            foreach (var c in GetCustomers())
            {
                list.Add(new CounterpartyEntry
                {
                    IsCustomer = true,
                    Id = c.Id,
                    Name = c.Name,
                    Inn = c.Inn,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address
                });
            }

            foreach (var s in GetSuppliers())
            {
                list.Add(new CounterpartyEntry
                {
                    IsCustomer = false,
                    Id = s.Id,
                    Name = s.Name,
                    Inn = s.Inn,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address
                });
            }

            return list;
        }

        public static List<CustomerRow> GetCustomers()
        {
            var list = new List<CustomerRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(
                @"SELECT CustomerID, Name, PhoneNumber, Email, Address, ISNULL(INN, N'')
                  FROM Customer ORDER BY Name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new CustomerRow
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Address = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Inn = reader.IsDBNull(5) ? "" : reader.GetString(5)
                    });
                }
            }

            return list;
        }

        public static List<SupplierRow> GetSuppliers()
        {
            var list = new List<SupplierRow>();
            var sql = SupplierHasExtendedFields()
                ? @"SELECT SupplierID, Name, PhoneNumber, Email, ISNULL(INN, N''), ISNULL(Address, N'')
                    FROM Supplier ORDER BY Name"
                : "SELECT SupplierID, Name, PhoneNumber, Email FROM Supplier ORDER BY Name";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new SupplierRow
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Inn = SupplierHasExtendedFields() && !reader.IsDBNull(4) ? reader.GetString(4) : "",
                        Address = SupplierHasExtendedFields() && !reader.IsDBNull(5) ? reader.GetString(5) : ""
                    });
                }
            }

            return list;
        }

        public static int CreateCustomer(string name, string phone, string email, string address, string inn)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Укажите наименование покупателя.");

            const string sql = @"
INSERT INTO Customer (Name, PhoneNumber, Email, Address, INN, DebtAmount)
VALUES (@Name, @Phone, @Email, @Address, @INN, 0);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@Phone", ToDb(phone));
                cmd.Parameters.AddWithValue("@Email", ToDb(email));
                cmd.Parameters.AddWithValue("@Address", ToDb(address));
                cmd.Parameters.AddWithValue("@INN", ToDb(inn));
                return (int)cmd.ExecuteScalar();
            }
        }

        public static void UpdateCustomer(int id, string name, string phone, string email, string address, string inn)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Укажите наименование покупателя.");

            const string sql = @"
UPDATE Customer
SET Name = @Name, PhoneNumber = @Phone, Email = @Email, Address = @Address, INN = @INN
WHERE CustomerID = @Id";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@Phone", ToDb(phone));
                cmd.Parameters.AddWithValue("@Email", ToDb(email));
                cmd.Parameters.AddWithValue("@Address", ToDb(address));
                cmd.Parameters.AddWithValue("@INN", ToDb(inn));
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Контрагент не найден.");
            }
        }

        public static void DeleteCustomer(int id)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM Customer WHERE CustomerID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                try
                {
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Контрагент не найден.");
                }
                catch (SqlException ex) when (ex.Number == 547)
                {
                    throw new InvalidOperationException(
                        "Нельзя удалить покупателя: есть связанные заказы на продажу.", ex);
                }
            }
        }

        public static int CreateSupplier(string name, string phone, string email, string inn = null, string address = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Укажите наименование поставщика.");

            var sql = SupplierHasExtendedFields()
                ? @"INSERT INTO Supplier (Name, PhoneNumber, Email, INN, Address)
                    VALUES (@Name, @Phone, @Email, @INN, @Address);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);"
                : @"INSERT INTO Supplier (Name, PhoneNumber, Email)
                    VALUES (@Name, @Phone, @Email);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@Phone", ToDb(phone));
                cmd.Parameters.AddWithValue("@Email", ToDb(email));
                if (SupplierHasExtendedFields())
                {
                    cmd.Parameters.AddWithValue("@INN", ToDb(inn));
                    cmd.Parameters.AddWithValue("@Address", ToDb(address));
                }

                return (int)cmd.ExecuteScalar();
            }
        }

        public static void UpdateSupplier(int id, string name, string phone, string email, string inn, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Укажите наименование поставщика.");

            var sql = SupplierHasExtendedFields()
                ? @"UPDATE Supplier
                    SET Name = @Name, PhoneNumber = @Phone, Email = @Email, INN = @INN, Address = @Address
                    WHERE SupplierID = @Id"
                : @"UPDATE Supplier
                    SET Name = @Name, PhoneNumber = @Phone, Email = @Email
                    WHERE SupplierID = @Id";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@Phone", ToDb(phone));
                cmd.Parameters.AddWithValue("@Email", ToDb(email));
                if (SupplierHasExtendedFields())
                {
                    cmd.Parameters.AddWithValue("@INN", ToDb(inn));
                    cmd.Parameters.AddWithValue("@Address", ToDb(address));
                }

                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Контрагент не найден.");
            }
        }

        public static void DeleteSupplier(int id)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM Supplier WHERE SupplierID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                try
                {
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Контрагент не найден.");
                }
                catch (SqlException ex) when (ex.Number == 547)
                {
                    throw new InvalidOperationException(
                        "Нельзя удалить поставщика: есть связанные заказы на закупку.", ex);
                }
            }
        }

        public static List<ProductRow> GetProducts()
        {
            var list = new List<ProductRow>();
            var sql = ProductHasExtendedFields()
                ? @"SELECT ProductID, Name, Variety, Seasonality, ISNULL(Unit, N'т'), ISNULL(BasePrice, 0)
                    FROM Product ORDER BY Name"
                : "SELECT ProductID, Name, Variety, Seasonality FROM Product ORDER BY Name";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new ProductRow
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Variety = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Seasonality = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Unit = ProductHasExtendedFields() && !reader.IsDBNull(4) ? reader.GetString(4) : "т",
                        BasePrice = ProductHasExtendedFields() && !reader.IsDBNull(5) ? reader.GetInt32(5) : GetDefaultBasePrice(reader.GetString(1))
                    });
                }
            }

            return list;
        }

        public static int CreateProduct(string name, string variety, string seasonality, string unit, int basePrice)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Укажите наименование товара.");

            var sql = ProductHasExtendedFields()
                ? @"INSERT INTO Product (Name, Variety, Seasonality, Unit, BasePrice)
                    VALUES (@Name, @Variety, @Seasonality, @Unit, @BasePrice);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);"
                : @"INSERT INTO Product (Name, Variety, Seasonality)
                    VALUES (@Name, @Variety, @Seasonality);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@Variety", ToDb(variety));
                cmd.Parameters.AddWithValue("@Seasonality", ToDb(seasonality));
                if (ProductHasExtendedFields())
                {
                    cmd.Parameters.AddWithValue("@Unit", string.IsNullOrWhiteSpace(unit) ? "т" : unit.Trim());
                    cmd.Parameters.AddWithValue("@BasePrice", basePrice);
                }

                return (int)cmd.ExecuteScalar();
            }
        }

        public static void UpdateProduct(int id, string name, string variety, string seasonality, string unit, int basePrice)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Укажите наименование товара.");

            var sql = ProductHasExtendedFields()
                ? @"UPDATE Product
                    SET Name = @Name, Variety = @Variety, Seasonality = @Seasonality, Unit = @Unit, BasePrice = @BasePrice
                    WHERE ProductID = @Id"
                : @"UPDATE Product
                    SET Name = @Name, Variety = @Variety, Seasonality = @Seasonality
                    WHERE ProductID = @Id";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@Variety", ToDb(variety));
                cmd.Parameters.AddWithValue("@Seasonality", ToDb(seasonality));
                if (ProductHasExtendedFields())
                {
                    cmd.Parameters.AddWithValue("@Unit", string.IsNullOrWhiteSpace(unit) ? "т" : unit.Trim());
                    cmd.Parameters.AddWithValue("@BasePrice", basePrice);
                }

                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Товар не найден.");
            }
        }

        private static int GetDefaultBasePrice(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            if (name.Contains("Пшеница")) return 5;
            if (name.Contains("Ячмень")) return 4;
            if (name.Contains("Кукуруза")) return 5;
            if (name.Contains("Подсолнечник")) return 8;
            if (name.Contains("Яблоко")) return 35;
            return 0;
        }

        public static void DeleteProduct(int id)
        {
            using (var conn = Db.OpenConnection())
            {
                using (var checkOrders = new SqlCommand(@"
SELECT COUNT(*)
FROM SalesOrder so
INNER JOIN ProductStock ps ON ps.StockID = so.StockID
WHERE ps.ProductID = @Id", conn))
                {
                    checkOrders.Parameters.AddWithValue("@Id", id);
                    if ((int)checkOrders.ExecuteScalar() > 0)
                    {
                        throw new InvalidOperationException(
                            "Нельзя удалить товар: есть связанные заказы на продажу.");
                    }
                }

                using (var deleteStock = new SqlCommand(
                    "DELETE FROM ProductStock WHERE ProductID = @Id", conn))
                {
                    deleteStock.Parameters.AddWithValue("@Id", id);
                    deleteStock.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand("DELETE FROM Product WHERE ProductID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    try
                    {
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException("Товар не найден.");
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        throw new InvalidOperationException(
                            "Нельзя удалить товар: есть связанные записи в системе.", ex);
                    }
                }
            }
        }

        private static object ToDb(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }
    }
}
