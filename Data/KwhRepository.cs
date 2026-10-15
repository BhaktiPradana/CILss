using LssTraining.Web.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                query += " AND (MeterName = @MeterName OR LTRIM(RTRIM(MeterName)) = @MeterNameTrimmed)";
            }

            query += " ORDER BY LogTime DESC, MeterName ASC";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                if (!string.IsNullOrEmpty(meterName))
                {
                    cmd.Parameters.AddWithValue("@MeterName", meterName);
                    cmd.Parameters.AddWithValue("@MeterNameTrimmed", meterName.Trim());
                }

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new KwhRecord
                    {
                        Timestamp = reader.GetDateTime(0),
                        MeterName = reader.GetString(1).Trim(),
                        Usage_kWh = reader.GetDecimal(2)
                    });
                }
            }
            catch
            {
                // Return empty list on connection error; caller will fallback to preview data
                return new List<KwhRecord>();
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
                query += " AND (MeterName = @MeterName OR LTRIM(RTRIM(MeterName)) = @MeterNameTrimmed)";
            }

            query += " ORDER BY LogDate DESC, MeterName ASC";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                if (!string.IsNullOrEmpty(meterName))
                {
                    cmd.Parameters.AddWithValue("@MeterName", meterName);
                    cmd.Parameters.AddWithValue("@MeterNameTrimmed", meterName.Trim());
                }

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new KwhRecord
                    {
                        Timestamp = reader.GetDateTime(0),
                        MeterName = reader.GetString(1).Trim(),
                        Usage_kWh = reader.GetDecimal(2)
                    });
                }
            }
            catch
            {
                // Return empty list on connection error; caller will fallback to preview data
                return new List<KwhRecord>();
            }

            return result;
        }

        public async Task<List<string>> GetMeterNamesAsync()
        {
            var meters = new List<string>();
            string query = "SELECT DISTINCT MeterName FROM Daily_kWh_Data ORDER BY MeterName";

            try
            {
                using var conn = (SqlConnection)_connectionFactory.CreateConnection();
                using var cmd = new SqlCommand(query, conn);
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    meters.Add(reader.GetString(0).Trim());
                }
            }
            catch
            {
                // Fallback to all catalog meters if DB unavailable
                return ElectricityCatalog.AllMeters.Select(m => m.MeterName).ToList();
            }

            if (meters.Count == 0)
            {
                return ElectricityCatalog.AllMeters.Select(m => m.MeterName).ToList();
            }

            return meters;
        }

        public static List<KwhRecord> GeneratePreviewData(DateTime startDate, DateTime endDate, string viewMode = "Daily")
        {
            var list = new List<KwhRecord>();
            var rnd = new Random(42);
            var meters = ElectricityCatalog.AllMeters;

            if (viewMode == "Hourly")
            {
                var current = startDate.Date.AddHours(8);
                var max = endDate.Date.AddDays(1).AddSeconds(-1);
                if (max > current.AddDays(2)) max = current.AddDays(2); // Limit preview hours

                while (current <= max)
                {
                    foreach (var m in meters)
                    {
                        decimal baseVal = m.Unit == "V" ? 380m + (decimal)(rnd.NextDouble() * 5 - 2.5) : (decimal)(rnd.NextDouble() * 45 + 5);
                        if (m.PanelType == "SSB" || m.PanelType == "MDB") baseVal *= 3.5m;

                        list.Add(new KwhRecord
                        {
                            Timestamp = current,
                            MeterName = m.MeterName,
                            Usage_kWh = Math.Round(baseVal, 2),
                            Floor = m.Floor
                        });
                    }
                    current = current.AddHours(2);
                }
            }
            else
            {
                var cur = startDate.Date;
                var max = endDate.Date;
                while (cur <= max)
                {
                    foreach (var m in meters)
                    {
                        decimal baseVal = m.Unit == "V" ? 380.2m + (decimal)(rnd.NextDouble() * 2 - 1) : (decimal)(rnd.NextDouble() * 320 + 80);
                        if (m.PanelType == "SSB" || m.PanelType == "MDB") baseVal *= 3.2m;

                        list.Add(new KwhRecord
                        {
                            Timestamp = cur,
                            MeterName = m.MeterName,
                            Usage_kWh = Math.Round(baseVal, 2),
                            Floor = m.Floor
                        });
                    }
                    cur = cur.AddDays(1);
                }
            }

            return list;
        }
    }
}