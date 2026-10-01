using LssTraining.Web.Models;
using Microsoft.Data.SqlClient; 
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace LssTraining.Web.Data
{
    public class KwhRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public KwhRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<KwhRecord>> GetHourlyDataAsync(DateTime startDate, DateTime endDate, string? meterName = "")
        {
            var result = new List<KwhRecord>();
            string query = @"
                SELECT LogTime, MeterName, kWh_Usage 
                FROM Hourly_kWh_Data
                WHERE LogTime >= @StartDate AND LogTime <= @EndDate";

            if (!string.IsNullOrEmpty(meterName))
            {
                query += " AND MeterName = @MeterName";
            }

            query += " ORDER BY LogTime DESC, MeterName ASC";
            using (var conn = (SqlConnection)_connectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                if (!string.IsNullOrEmpty(meterName))
                {
                    cmd.Parameters.AddWithValue("@MeterName", meterName);
                }

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new KwhRecord
                        {
                            Timestamp = reader.GetDateTime(0),
                            MeterName = reader.GetString(1),
                            Usage_kWh = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return result;
        }

        public async Task<List<KwhRecord>> GetDailyDataAsync(DateTime startDate, DateTime endDate, string? meterName = "")
        {
            var result = new List<KwhRecord>();
            string query = @"
                SELECT LogDate, MeterName, Total_kWh 
                FROM Daily_kWh_Data
                WHERE LogDate >= CAST(@StartDate AS DATE) AND LogDate <= CAST(@EndDate AS DATE)";

            if (!string.IsNullOrEmpty(meterName))
            {
                query += " AND MeterName = @MeterName";
            }

            query += " ORDER BY LogDate DESC, MeterName ASC";

            // FIX 3: Cast to SqlConnection
            using (var conn = (SqlConnection)_connectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                if (!string.IsNullOrEmpty(meterName))
                {
                    cmd.Parameters.AddWithValue("@MeterName", meterName);
                }

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new KwhRecord
                        {
                            Timestamp = reader.GetDateTime(0),
                            MeterName = reader.GetString(1),
                            Usage_kWh = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return result;
        }

        public async Task<List<string>> GetMeterNamesAsync()
        {
            var meters = new List<string>();
            string query = "SELECT DISTINCT MeterName FROM Daily_kWh_Data ORDER BY MeterName";

            using (var conn = (SqlConnection)_connectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        meters.Add(reader.GetString(0));
                    }
                }
            }
            return meters;
        }
    }
}