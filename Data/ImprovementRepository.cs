using LssTraining.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LssTraining.Web.Data
{
    public class ImprovementRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private static readonly List<MachineImprovement> _inMemoryFallback = new();

        public ImprovementRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            SeedInitialFallbackData();
        }

        public async Task<List<MachineImprovement>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<MachineImprovement>();
            string query = @"
                SELECT i.Id, i.MachCode, m.MachName, i.ImprovementTitle, i.Description, 
                       i.ImplementationDate, i.BaselineKwhPerDay, i.TargetKwhPerDay, 
                       i.ActualKwhPerDay, i.Status, i.CreatedBy, i.CreatedAt
                FROM MachineImprovements i
                INNER JOIN MasterMachines m ON m.MachCode = i.MachCode
                ORDER BY i.ImplementationDate DESC";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                await conn.OpenAsync(cancellationToken);
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(new MachineImprovement
                    {
                        Id = reader.GetInt32(0),
                        MachCode = reader.GetString(1),
                        MachName = reader.GetString(2),
                        ImprovementTitle = reader.GetString(3),
                        Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                        ImplementationDate = reader.GetDateTime(5),
                        BaselineKwhPerDay = reader.GetDecimal(6),
                        TargetKwhPerDay = reader.GetDecimal(7),
                        ActualKwhPerDay = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                        Status = reader.GetString(9),
                        CreatedBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                        CreatedAt = reader.GetDateTime(11)
                    });
                }
                return result;
            }
            catch
            {
                // Fallback to memory store if DB table does not exist yet
                return _inMemoryFallback.OrderByDescending(x => x.ImplementationDate).ToList();
            }
        }

        public async Task CreateAsync(MachineImprovement item, CancellationToken cancellationToken = default)
        {
            string insertQuery = @"
                IF OBJECT_ID(N'dbo.MachineImprovements', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.MachineImprovements (
                        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        MachCode nvarchar(50) NOT NULL,
                        ImprovementTitle nvarchar(250) NOT NULL,
                        Description nvarchar(1000) NULL,
                        ImplementationDate date NOT NULL,
                        BaselineKwhPerDay decimal(18,2) NOT NULL DEFAULT 0,
                        TargetKwhPerDay decimal(18,2) NOT NULL DEFAULT 0,
                        ActualKwhPerDay decimal(18,2) NULL,
                        Status nvarchar(30) NOT NULL DEFAULT 'In Progress',
                        CreatedBy nvarchar(160) NULL,
                        CreatedAt datetime2(0) NOT NULL DEFAULT SYSUTCDATETIME()
                    );
                END;

                INSERT INTO dbo.MachineImprovements (MachCode, ImprovementTitle, Description, ImplementationDate, BaselineKwhPerDay, TargetKwhPerDay, ActualKwhPerDay, Status, CreatedBy)
                VALUES (@MachCode, @ImprovementTitle, @Description, @ImplementationDate, @BaselineKwhPerDay, @TargetKwhPerDay, @ActualKwhPerDay, @Status, @CreatedBy);";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@MachCode", item.MachCode);
                cmd.Parameters.AddWithValue("@ImprovementTitle", item.ImprovementTitle);
                cmd.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ImplementationDate", item.ImplementationDate.Date);
                cmd.Parameters.AddWithValue("@BaselineKwhPerDay", item.BaselineKwhPerDay);
                cmd.Parameters.AddWithValue("@TargetKwhPerDay", item.TargetKwhPerDay);
                cmd.Parameters.AddWithValue("@ActualKwhPerDay", (object?)item.ActualKwhPerDay ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", item.Status);
                cmd.Parameters.AddWithValue("@CreatedBy", (object?)item.CreatedBy ?? DBNull.Value);

                await conn.OpenAsync(cancellationToken);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch
            {
                item.Id = _inMemoryFallback.Count + 1;
                item.CreatedAt = DateTime.UtcNow;
                _inMemoryFallback.Add(item);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            string deleteQuery = "DELETE FROM MachineImprovements WHERE Id = @Id";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(deleteQuery, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync(cancellationToken);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch
            {
                _inMemoryFallback.RemoveAll(x => x.Id == id);
            }
        }

        private static void SeedInitialFallbackData()
        {
            if (_inMemoryFallback.Count == 0)
            {
                _inMemoryFallback.AddRange(new[]
                {
                    new MachineImprovement
                    {
                        Id = 1,
                        MachCode = "ACT1",
                        MachName = "CAP TRANSFER",
                        ImprovementTitle = "VFD Frequency Inverter Retrofit & Motor Efficiency Upgrade",
                        Description = "Installed VFD drive to regulate motor speed during idle cycles and upgraded motor winding for higher efficiency.",
                        ImplementationDate = DateTime.Today.AddDays(-30),
                        BaselineKwhPerDay = 145.50m,
                        TargetKwhPerDay = 110.00m,
                        ActualKwhPerDay = 108.20m,
                        Status = "Completed",
                        CreatedBy = "Energy Team"
                    },
                    new MachineImprovement
                    {
                        Id = 2,
                        MachCode = "AD01",
                        MachName = "AUTO DISPENSING #1",
                        ImprovementTitle = "Pneumatic Pressure Optimization & Smart Idle Sleep Mode",
                        Description = "Adjusted air pressure regulator from 7.0 bar to 5.8 bar and configured automatic standby timer after 5 minutes of inactivity.",
                        ImplementationDate = DateTime.Today.AddDays(-15),
                        BaselineKwhPerDay = 98.00m,
                        TargetKwhPerDay = 75.00m,
                        ActualKwhPerDay = 78.50m,
                        Status = "Completed",
                        CreatedBy = "Maintenance Team"
                    },
                    new MachineImprovement
                    {
                        Id = 3,
                        MachCode = "AD02",
                        MachName = "AUTO DISPENSING #2",
                        ImprovementTitle = "High-Efficiency Servo Drive System Replacement",
                        Description = "Replacing legacy induction motor with precision direct-drive servo system to eliminate friction losses.",
                        ImplementationDate = DateTime.Today.AddDays(-5),
                        BaselineKwhPerDay = 105.00m,
                        TargetKwhPerDay = 80.00m,
                        ActualKwhPerDay = null,
                        Status = "In Progress",
                        CreatedBy = "Kaizen Committee"
                    }
                });
            }
        }
    }
}
