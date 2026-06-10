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
