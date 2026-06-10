using System.Data.SqlClient;

namespace AgroRegionApp.Data
{
    internal static class Db
    {
        public static SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(AuthService.ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
