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
    }

    internal sealed class SupplierRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    internal sealed class ProductRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Variety { get; set; }
        public string Seasonality { get; set; }
    }

    internal static class ReferenceService
    {
        public static List<CustomerRow> GetCustomers()
        {
            var list = new List<CustomerRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(
                "SELECT CustomerID, Name, PhoneNumber, Email, Address FROM Customer ORDER BY Name", conn))
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
                        Address = reader.IsDBNull(4) ? "" : reader.GetString(4)
                    });
                }
            }

            return list;
        }

        public static List<SupplierRow> GetSuppliers()
        {
            var list = new List<SupplierRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(
                "SELECT SupplierID, Name, PhoneNumber, Email FROM Supplier ORDER BY Name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new SupplierRow
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Email = reader.IsDBNull(3) ? "" : reader.GetString(3)
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
                cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone.Trim());
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email.Trim());
                cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(address) ? (object)DBNull.Value : address.Trim());
                cmd.Parameters.AddWithValue("@INN", string.IsNullOrWhiteSpace(inn) ? (object)DBNull.Value : inn.Trim());
                return (int)cmd.ExecuteScalar();
            }
        }

        public static int CreateSupplier(string name, string phone, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Укажите наименование поставщика.");

            const string sql = @"
INSERT INTO Supplier (Name, PhoneNumber, Email)
VALUES (@Name, @Phone, @Email);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone.Trim());
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email.Trim());
                return (int)cmd.ExecuteScalar();
            }
        }

        public static List<ProductRow> GetProducts()
        {
            var list = new List<ProductRow>();
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(
                "SELECT ProductID, Name, Variety, Seasonality FROM Product ORDER BY Name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new ProductRow
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Variety = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Seasonality = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }

            return list;
        }
    }
}
