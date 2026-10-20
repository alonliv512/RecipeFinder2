using MySql.Data.MySqlClient;

namespace DAL;

public static class DatabaseInitializer
{
    private const string DatabaseName = "RecipeFinderDB";

    public static async Task InitializeAsync(string baseConnectionString)
    {
        await CreateDatabaseAsync(baseConnectionString);

        string dbConnectionString = baseConnectionString + $"Database={DatabaseName};";
        await CreateTablesAsync(dbConnectionString);
        await SeedInitialDataAsync(dbConnectionString);
    }

    private static async Task CreateDatabaseAsync(string baseConnectionString)
    {
        using var connection = new MySqlConnection(baseConnectionString);
        await connection.OpenAsync();

        string query = $"CREATE DATABASE IF NOT EXISTS `{DatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        using var command = new MySqlCommand(query, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task CreateTablesAsync(string connectionString)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string sqlScript = @"
            CREATE TABLE IF NOT EXISTS Roles (
                RoleID INT AUTO_INCREMENT PRIMARY KEY,
                RoleName VARCHAR(30) NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Users (
                UserID INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(50) NOT NULL UNIQUE,
                PasswordHash VARCHAR(255) NOT NULL,
                RoleID INT NOT NULL,
                IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Categories (
                CategoryID INT AUTO_INCREMENT PRIMARY KEY,
                CategoryName VARCHAR(50) NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Ingredients (
                IngredientID INT AUTO_INCREMENT PRIMARY KEY,
                IngredientName VARCHAR(50) NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Recipes (
                RecipeID INT AUTO_INCREMENT PRIMARY KEY,
                Title VARCHAR(150) NOT NULL,
                Instructions TEXT NOT NULL,
                PreparationTime INT NOT NULL,
                Servings INT NOT NULL,
                ImageUrl VARCHAR(255) NULL,
                UserID INT NOT NULL,
                CategoryID INT NOT NULL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
                FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
            );

            CREATE TABLE IF NOT EXISTS RecipeIngredients (
                RecipeID INT NOT NULL,
                IngredientID INT NOT NULL,
                Amount DECIMAL(8,2) NOT NULL,
                Unit VARCHAR(30) NOT NULL,
                PRIMARY KEY (RecipeID, IngredientID),
                FOREIGN KEY (RecipeID) REFERENCES Recipes(RecipeID) ON DELETE CASCADE,
                FOREIGN KEY (IngredientID) REFERENCES Ingredients(IngredientID) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Reviews (
                ReviewID INT AUTO_INCREMENT PRIMARY KEY,
                RecipeID INT NOT NULL,
                UserID INT NOT NULL,
                Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
                Comment TEXT NULL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (RecipeID) REFERENCES Recipes(RecipeID) ON DELETE CASCADE,
                FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Favorites (
                UserID INT NOT NULL,
                RecipeID INT NOT NULL,
                SavedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (UserID, RecipeID),
                FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
                FOREIGN KEY (RecipeID) REFERENCES Recipes(RecipeID) ON DELETE CASCADE
            );
        ";

        using var cmd = new MySqlCommand(sqlScript, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task SeedInitialDataAsync(string connectionString)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        // הכנסת תפקידים ראשוניים
        string checkRolesQuery = "SELECT COUNT(*) FROM Roles;";
        using (var checkCmd = new MySqlCommand(checkRolesQuery, connection))
        {
            long count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
            if (count == 0)
            {
                string insertRoles = "INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Registered User');";
                using var insertCmd = new MySqlCommand(insertRoles, connection);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }

        // הכנסת קטגוריות ראשוניות
        string checkCatQuery = "SELECT COUNT(*) FROM Categories;";
        using (var checkCmd = new MySqlCommand(checkCatQuery, connection))
        {
            long count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
            if (count == 0)
            {
                string insertCategories = "INSERT INTO Categories (CategoryName) VALUES ('חלבי'), ('בשרי'), ('פרווה'), ('טבעוני'), ('קינוחים'), ('מהיר להכנה');";
                using var insertCmd = new MySqlCommand(insertCategories, connection);
                await insertCmd.ExecuteNonQueryAsync();
            }
        }
    }
}
