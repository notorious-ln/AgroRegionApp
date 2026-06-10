using System;
using System.Configuration;
using System.Data.SqlClient;
using AgroRegionApp.Models;

namespace AgroRegionApp.Data
{
    public enum AuthFailureReason
    {
        InvalidCredentials,
        Blocked,
        DatabaseError
    }

    public sealed class AuthResult
    {
        public bool Success { get; set; }
        public AuthFailureReason? FailureReason { get; set; }
        public string ErrorMessage { get; set; }
        public AuthenticatedUser User { get; set; }
    }

    public static class AuthService
    {
        private const string AuthQuery = @"
SELECT ua.AccountID,
       ua.PasswordHash,
       ua.IsBlocked,
       ur.RoleName,
       e.EmployeeID,
       e.LastName,
       e.FirstName,
       e.MiddleName
FROM UserAccount ua
INNER JOIN UserRole ur ON ua.RoleID = ur.RoleID
LEFT JOIN Employee e ON e.AccountID = ua.AccountID
WHERE ua.Login = @Login";

        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["AgroCompany"]?.ConnectionString;

        public static bool TestConnection(out string message)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    message = "Подключено к MS SQL Server";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Нет подключения к MS SQL Server";
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }
        }

        public static AuthResult Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password))
            {
                return new AuthResult
                {
                    Success = false,
                    FailureReason = AuthFailureReason.InvalidCredentials
                };
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                using (var command = new SqlCommand(AuthQuery, connection))
                {
                    command.Parameters.AddWithValue("@Login", login.Trim());
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return new AuthResult
                            {
                                Success = false,
                                FailureReason = AuthFailureReason.InvalidCredentials
                            };
                        }

                        var isBlocked = reader["IsBlocked"] != DBNull.Value && Convert.ToBoolean(reader["IsBlocked"]);
                        if (isBlocked)
                        {
                            return new AuthResult
                            {
                                Success = false,
                                FailureReason = AuthFailureReason.Blocked
                            };
                        }

                        var storedHash = reader["PasswordHash"] as string;
                        if (!PasswordHasher.Verify(password, storedHash))
                        {
                            return new AuthResult
                            {
                                Success = false,
                                FailureReason = AuthFailureReason.InvalidCredentials
                            };
                        }

                        return new AuthResult
                        {
                            Success = true,
                            User = new AuthenticatedUser
                            {
                                AccountId = Convert.ToInt32(reader["AccountID"]),
                                EmployeeId = reader["EmployeeID"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(reader["EmployeeID"]),
                                Login = login.Trim(),
                                RoleName = reader["RoleName"] as string ?? string.Empty,
                                DisplayName = FormatDisplayName(
                                    reader["LastName"] as string,
                                    reader["FirstName"] as string,
                                    reader["MiddleName"] as string,
                                    login.Trim())
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    FailureReason = AuthFailureReason.DatabaseError,
                    ErrorMessage = ex.Message
                };
            }
        }

        private static string FormatDisplayName(string lastName, string firstName, string middleName, string login)
        {
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                var initials = string.Empty;
                if (!string.IsNullOrWhiteSpace(firstName))
                    initials += firstName[0] + ".";
                if (!string.IsNullOrWhiteSpace(middleName))
                    initials += middleName[0] + ".";
                return string.IsNullOrEmpty(initials)
                    ? lastName
                    : $"{lastName} {initials}";
            }

            return login;
        }
    }
}
