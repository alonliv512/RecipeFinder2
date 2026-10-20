using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Model;

namespace DAL
{
    public class UserDB
    {
        private readonly string connectionString;

        public UserDB(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private User ReadUser(System.Data.Common.DbDataReader reader)
        {
            return new User(
                Convert.ToInt32(reader["UserID"]),
                reader["Username"].ToString(),
                reader["PasswordHash"].ToString(),
                Convert.ToInt32(reader["RoleID"]),
                reader["RoleName"].ToString(),
                Convert.ToBoolean(reader["IsActive"])
            );
        }

        public async Task<int> AddUserAsync(User user)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"INSERT INTO Users (Username, PasswordHash, RoleID, IsActive) 
                             VALUES (@Username, @PasswordHash, @RoleID, @IsActive);
                             SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@RoleID", user.RoleID == 0 ? 2 : user.RoleID);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            var id = await command.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            long count = (long)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            // אם בעתיד תרצה להוסיף שדה אימייל בטבלה, אפשר לממש כאן. כרגע נחזיר false.
            await Task.CompletedTask;
            return false;
        }

        public async Task<User> CheckLoginAsync(string username, string passwordHash)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"SELECT u.UserID, u.Username, u.PasswordHash, u.RoleID, r.RoleName, u.IsActive 
                             FROM Users u
                             JOIN Roles r ON u.RoleID = r.RoleID
                             WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash AND u.IsActive = TRUE";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadUser(reader);
            }
            return null;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"SELECT u.UserID, u.Username, u.PasswordHash, u.RoleID, r.RoleName, u.IsActive 
                             FROM Users u
                             JOIN Roles r ON u.RoleID = r.RoleID
                             WHERE u.UserID = @UserID";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadUser(reader);
            }
            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"SELECT u.UserID, u.Username, u.PasswordHash, u.RoleID, r.RoleName, u.IsActive 
                             FROM Users u
                             JOIN Roles r ON u.RoleID = r.RoleID";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(ReadUser(reader));
            }
            return users;
        }

        public async Task<int> UpdateUserRoleAsync(int userId, int newRoleId)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "UPDATE Users SET RoleID = @RoleID WHERE UserID = @UserID";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@RoleID", newRoleId);
            command.Parameters.AddWithValue("@UserID", userId);

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
            command.Parameters.AddWithValue("@UserID", userId);

            return await command.ExecuteNonQueryAsync();
        }
    }
}