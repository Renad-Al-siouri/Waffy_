using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly string _connectionString;

        public LocationRepository(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("WaffyConnection")!;
        }

        public async Task<IEnumerable<Governorate>> GetAllGovernoratesAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Governorate>(
                "SELECT Id, NameEn FROM Governorates ORDER BY NameEn");
        }

        public async Task<IEnumerable<District>> GetAllDistrictsAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<District>(
                "SELECT Id, GovernorateId, NameEn FROM Districts ORDER BY NameEn");
        }
    }
}