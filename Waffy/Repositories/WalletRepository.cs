using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Waffy.Models;


public class WalletRepository
{
    private readonly IConfiguration _config;

    public WalletRepository(IConfiguration config)
    {
        _config = config;
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }

    public async Task<IEnumerable<Wallet>> GetAllAsync()
    {
        using var db = CreateConnection();

        string sql = @"
            SELECT 
                W.Id,
                W.UserId,
                U.FullName AS UserName,
                U.Email AS UserEmail,
                W.Balance,
                W.Status,
                W.CreatedAt,
                W.UpdatedAt
            FROM Wallets W
            INNER JOIN Users U ON W.UserId = U.Id
            ORDER BY W.Id DESC";

        return await db.QueryAsync<Wallet>(sql);
    }

    public async Task<Wallet?> GetByIdAsync(int id)
    {
        using var db = CreateConnection();

        string sql = @"SELECT * FROM Wallets WHERE Id = @Id";
        return await db.QueryFirstOrDefaultAsync<Wallet>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Wallet wallet)
    {
        using var db = CreateConnection();

        string sql = @"
            INSERT INTO Wallets (UserId, Balance, Status, CreatedAt, UpdatedAt)
            VALUES (@UserId, @Balance, @Status, @CreatedAt, @UpdatedAt)";

        return await db.ExecuteAsync(sql, wallet);
    }

    public async Task<int> UpdateAsync(Wallet wallet)
    {
        using var db = CreateConnection();

        string sql = @"
            UPDATE Wallets
            SET
                UserId = @UserId,
                Balance = @Balance,
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        return await db.ExecuteAsync(sql, wallet);
    }

    public async Task<int> DeleteAsync(int id)
    {
        using var db = CreateConnection();

        string sql = @"DELETE FROM Wallets WHERE Id = @Id";
        return await db.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<IEnumerable<User>> GetUsersWithoutWalletAsync()
    {
        using var db = CreateConnection();

        string sql = @"
            SELECT U.*
            FROM Users U
            WHERE NOT EXISTS (
                SELECT 1
                FROM Wallets W
                WHERE W.UserId = U.Id
            )
            ORDER BY U.FullName";

        return await db.QueryAsync<User>(sql);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using var db = CreateConnection();

        string sql = @"SELECT * FROM Users ORDER BY FullName";
        return await db.QueryAsync<User>(sql);
    }

    public async Task<bool> UserAlreadyHasWalletAsync(int userId)
    {
        using var db = CreateConnection();

        string sql = @"SELECT COUNT(1) FROM Wallets WHERE UserId = @UserId";
        int count = await db.ExecuteScalarAsync<int>(sql, new { UserId = userId });

        return count > 0;
    }
}