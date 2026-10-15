using LssTraining.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LssTraining.Web.Data
{
    public class DistributionBoardRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DistributionBoardRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // ── Distribution Boards (from MasterDBs & Daily_kWh_Data fallback) ────────

        public async Task<List<DistributionBoard>> GetAllAsync()
        {
            var result = new List<DistributionBoard>();

            try
            {
                // Query from MasterDBs first
                string query = "SELECT DBID, DBName FROM MasterDBs ORDER BY DBName";

                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(0);
                        string dbName = reader.GetString(1);
                        result.Add(CreateBoardFromDbName(id, dbName));
                    }
                }

                // Fallback: If MasterDBs has no records, check Daily_kWh_Data for distinct MeterName
                if (result.Count == 0)
                {
                    string fallbackQuery = "SELECT DISTINCT MeterName FROM Daily_kWh_Data ORDER BY MeterName";
                    using var fallbackCmd = new SqlCommand(fallbackQuery, conn);
                    using var fallbackReader = await fallbackCmd.ExecuteReaderAsync();
                    int id = 1;
                    while (await fallbackReader.ReadAsync())
                    {
                        string dbName = fallbackReader.GetString(0);
                        result.Add(CreateBoardFromDbName(id++, dbName));
                    }
                }
            }
            catch
            {
                // Fallback to catalog when DB is offline
                int id = 1;
                foreach (var meter in ElectricityCatalog.AllMeters)
                {
                    result.Add(new DistributionBoard
                    {
                        DBID = id++,
                        DBName = meter.MeterName,
                        Location = $"{meter.Building} - {meter.Floor}",
                        Building = meter.Building,
                        Floor = meter.Floor,
                        PanelType = meter.PanelType,
                        IsActive = true
                    });
                }
            }

            if (result.Count == 0)
            {
                int id = 1;
                foreach (var meter in ElectricityCatalog.AllMeters)
                {
                    result.Add(new DistributionBoard
                    {
                        DBID = id++,
                        DBName = meter.MeterName,
                        Location = $"{meter.Building} - {meter.Floor}",
                        Building = meter.Building,
                        Floor = meter.Floor,
                        PanelType = meter.PanelType,
                        IsActive = true
                    });
                }
            }

            return result;
        }

        public async Task<List<string>> GetBuildingsAsync()
        {
            var boards = await GetAllAsync();
            return boards.Select(b => b.Building)
                         .Where(b => !string.IsNullOrWhiteSpace(b))
                         .Distinct()
                         .OrderBy(b => b)
                         .ToList();
        }

        public async Task<List<string>> GetFloorsAsync()
        {
            var boards = await GetAllAsync();
            return boards.Select(b => b.Floor)
                         .Where(f => !string.IsNullOrWhiteSpace(f))
                         .Distinct()
                         .OrderBy(f => f)
                         .ToList();
        }

        public async Task<List<string>> GetBoardNamesAsync()
        {
            var boards = await GetAllAsync();
            return boards.Select(b => b.DBName)
                         .Distinct()
                         .OrderBy(b => b)
                         .ToList();
        }

        // ── Machines (from MasterMachines) ───────────────────

        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            var result = new List<Machine>();
            string query = "SELECT MachCode, MachName FROM MasterMachines ORDER BY MachName";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Machine
                    {
                        MachCode = reader.GetString(0),
                        MachName = reader.GetString(1)
                    });
                }
            }
            catch
            {
                return new List<Machine>();
            }
            return result;
        }

        public async Task<List<Machine>> GetUnmappedMachinesAsync()
        {
            var result = new List<Machine>();
            string query = @"
                SELECT m.MachCode, m.MachName 
                FROM MasterMachines m
                LEFT JOIN MasterMachineConnections c ON m.MachCode = c.MachCode
                WHERE c.MachCode IS NULL
                ORDER BY m.MachName";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Machine
                    {
                        MachCode = reader.GetString(0),
                        MachName = reader.GetString(1)
                    });
                }
            }
            catch
            {
                return new List<Machine>();
            }
            return result;
        }

        // ── DB-Machine Connections (from MasterMachineConnections) ─────

        public async Task<List<DbMachineMapping>> GetMappingsByBoardNameAsync(string boardName)
        {
            var result = new List<DbMachineMapping>();
            string query = @"
                SELECT c.ConnectionID, m.MachCode, m.MachName, c.SubDBName, c.DBName, c.Remarks
                FROM MasterMachineConnections c
                INNER JOIN MasterMachines m ON m.MachCode = c.MachCode
                WHERE c.DBName = @BoardName OR c.SubDBName = @BoardName
                ORDER BY m.MachName, c.ConnectionID";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BoardName", boardName);
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string mainDb = reader.GetString(4);
                    string? subDb = reader.IsDBNull(3) ? null : reader.GetString(3);
                    string? rem = reader.IsDBNull(5) ? null : reader.GetString(5);

                    result.Add(new DbMachineMapping
                    {
                        ConnectionID = reader.GetInt32(0),
                        MachCode = reader.GetString(1),
                        MachName = reader.GetString(2),
                        SubDBName = subDb,
                        DBName = mainDb,
                        Remarks = rem,
                        DbPrimary = mainDb,
                        DbSecondary = subDb,
                        DbTertiary = rem
                    });
                }
            }
            catch
            {
                return new List<DbMachineMapping>();
            }

            return result;
        }

        public async Task<List<DbMachineMapping>> GetAllMappingsAsync()
        {
            var result = new List<DbMachineMapping>();
            string query = @"
                SELECT c.ConnectionID, m.MachCode, m.MachName, c.SubDBName, c.DBName, c.Remarks
                FROM MasterMachineConnections c
                INNER JOIN MasterMachines m ON m.MachCode = c.MachCode
                ORDER BY m.MachName, c.ConnectionID";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string mainDb = reader.GetString(4);
                    string? subDb = reader.IsDBNull(3) ? null : reader.GetString(3);
                    string? rem = reader.IsDBNull(5) ? null : reader.GetString(5);

                    result.Add(new DbMachineMapping
                    {
                        ConnectionID = reader.GetInt32(0),
                        MachCode = reader.GetString(1),
                        MachName = reader.GetString(2),
                        SubDBName = subDb,
                        DBName = mainDb,
                        Remarks = rem,
                        DbPrimary = mainDb,
                        DbSecondary = subDb,
                        DbTertiary = rem
                    });
                }
            }
            catch
            {
                return new List<DbMachineMapping>();
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetMachineCountsPerBoardAsync()
        {
            var dict = new Dictionary<string, int>();
            string query = @"
                SELECT DBName, COUNT(DISTINCT MachCode) AS MachineCount
                FROM MasterMachineConnections
                GROUP BY DBName";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    dict[reader.GetString(0)] = reader.GetInt32(1);
                }
            }
            catch
            {
                return new Dictionary<string, int>();
            }

            return dict;
        }

        public async Task SaveMappingAsync(string machCode, string dbName, string? subDbName = null, string? remarks = null, string? dbPrimary = null, string? dbSecondary = null, string? dbTertiary = null)
        {
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            string mainDb = !string.IsNullOrWhiteSpace(dbPrimary) ? dbPrimary : dbName;
            string? subDb = !string.IsNullOrWhiteSpace(subDbName) ? subDbName : (!string.IsNullOrWhiteSpace(dbSecondary) ? dbSecondary : null);
            string? rem = !string.IsNullOrWhiteSpace(remarks) ? remarks : (!string.IsNullOrWhiteSpace(dbTertiary) ? dbTertiary : "Primary Connection");

            string insertQuery = @"
                INSERT INTO MasterMachineConnections (MachCode, SubDBName, DBName, Remarks)
                VALUES (@MachCode, @SubDBName, @DBName, @Remarks)";

            using var cmd = new SqlCommand(insertQuery, conn);
            cmd.Parameters.AddWithValue("@MachCode", machCode);
            cmd.Parameters.AddWithValue("@SubDBName", (object?)subDb ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DBName", mainDb);
            cmd.Parameters.AddWithValue("@Remarks", (object?)rem ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();

            // If DbTertiary is provided as an extra DB, save it as a separate row or in Remarks
            if (!string.IsNullOrWhiteSpace(dbTertiary) && dbTertiary != subDb && dbTertiary != mainDb)
            {
                using var extraCmd = new SqlCommand(insertQuery, conn);
                extraCmd.Parameters.AddWithValue("@MachCode", machCode);
                extraCmd.Parameters.AddWithValue("@SubDBName", DBNull.Value);
                extraCmd.Parameters.AddWithValue("@DBName", dbTertiary);
                extraCmd.Parameters.AddWithValue("@Remarks", "Tertiary Connection");
                await extraCmd.ExecuteNonQueryAsync();
            }
        }

        public Task AddMappingAsync(string machCode, string mainDb, string? subDb = null, string? dbTertiary = null, string? remarks = null)
            => SaveMappingAsync(machCode, mainDb, subDb, remarks, mainDb, subDb, dbTertiary);

        public async Task DeleteMappingAsync(int connectionId)
        {
            string query = "DELETE FROM MasterMachineConnections WHERE ConnectionID = @ConnectionID";

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ConnectionID", connectionId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteMappingByMachCodeAsync(string machCode)
        {
            string query = "DELETE FROM MasterMachineConnections WHERE MachCode = @MachCode";

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MachCode", machCode);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ── Helper Parser ─────────────────────────────────────

        public static DistributionBoard CreateBoardFromDbName(int dbid, string dbName)
        {
            var resolved = ElectricityCatalog.ResolveMeter(dbName);
            return new DistributionBoard
            {
                DBID = dbid,
                DBName = dbName,
                Location = $"{resolved.Building} - {resolved.Floor}",
                Building = resolved.Building,
                Floor = resolved.Floor,
                PanelType = resolved.PanelType,
                IsActive = true
            };
        }
    }
}
