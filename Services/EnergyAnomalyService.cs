using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TimeSeries;

namespace LssTraining.Web.Services
{
    public interface IEnergyAnomalyService
    {
        EnergyAnomalySummary AnalyzeAnomalies(
            IEnumerable<KwhRecord> records, 
            string? buildingFilter = null, 
            string? floorFilter = null,
            int maxTopAnomalies = 15);
    }

    public class EnergyAnomalyService : IEnergyAnomalyService
    {
        private readonly MLContext _mlContext;

        public EnergyAnomalyService()
        {
            _mlContext = new MLContext(seed: 42);
        }

        /// <summary>
        /// Analyzes a collection of electricity usage records across distribution boards using ML.NET
        /// to detect spikes, phantom off-hours leakages, and baseline anomalies.
        /// </summary>
        public EnergyAnomalySummary AnalyzeAnomalies(
            IEnumerable<KwhRecord> records, 
            string? buildingFilter = null, 
            string? floorFilter = null,
            int maxTopAnomalies = 15)
        {
            var summary = new EnergyAnomalySummary();
            if (records == null || !records.Any())
                return summary;

            var allDetected = new List<EnergyAnomalyResult>();

            // Group records by clean MeterName
            var groupedMeters = records
                .GroupBy(r => r.MeterName.Trim(), StringComparer.OrdinalIgnoreCase);

            foreach (var group in groupedMeters)
            {
                string meterName = group.Key;
                var orderedData = group.OrderBy(r => r.Timestamp).ToList();

                if (orderedData.Count < 4)
                    continue; // Skip series with insufficient data points

                // Resolve catalog metadata
                var cardInfo = ElectricityCardCatalog.FindItem(meterName);
                string displayName = cardInfo?.DisplayName ?? meterName;
                string bld = cardInfo?.Building ?? ElectricityCatalog.ResolveMeter(meterName).Building;
                string flr = cardInfo?.Floor ?? group.First().Floor;

                // Apply optional filters if specified
                if (!string.IsNullOrEmpty(buildingFilter) && !bld.Equals(buildingFilter, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!string.IsNullOrEmpty(floorFilter) && !flr.Equals(floorFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Run ML.NET Anomaly Detection on this series
                var meterAnomalies = DetectMeterAnomalies(meterName, displayName, bld, flr, orderedData);
                allDetected.AddRange(meterAnomalies);
            }

            // Rank anomalies by severity and confidence
            var orderedAnomalies = allDetected
                .OrderByDescending(a => a.Severity == "CRITICAL" ? 2 : (a.Severity == "WARNING" ? 1 : 0))
                .ThenByDescending(a => Math.Abs(a.DeviationKwh))
                .ThenByDescending(a => a.ConfidencePercent)
                .ToList();

            summary.TotalAnomaliesDetected = orderedAnomalies.Count;
            summary.CriticalAnomaliesCount = orderedAnomalies.Count(a => a.Severity == "CRITICAL");
            summary.WarningAnomaliesCount = orderedAnomalies.Count(a => a.Severity == "WARNING");
            summary.OffHoursLeakagesCount = orderedAnomalies.Count(a => a.AnomalyType == "OffHoursLeakage");
            summary.TotalEstimatedWastedKwh = orderedAnomalies
                .Where(a => a.DeviationKwh > 0)
                .Sum(a => a.DeviationKwh);

            summary.TopAnomalies = orderedAnomalies.Take(maxTopAnomalies).ToList();
            summary.AnomalousMeterNames = new HashSet<string>(
                orderedAnomalies.Select(a => a.MeterName), 
                StringComparer.OrdinalIgnoreCase
            );

            return summary;
        }

        private List<EnergyAnomalyResult> DetectMeterAnomalies(
            string meterName,
            string displayName,
            string building,
            string floor,
            List<KwhRecord> series)
        {
            var results = new List<EnergyAnomalyResult>();
            if (series.Count < 4) return results;

            // 1. Prepare ML.NET input series
            var timeSeriesInputs = series.Select(s => new EnergyAnomalyData
            {
                Timestamp = s.Timestamp,
                Value = (float)s.Usage_kWh
            }).ToList();

            // Calculate rolling statistics for baseline expectations
            var values = series.Select(s => (double)s.Usage_kWh).ToList();
            double avg = values.Average();
            double variance = values.Sum(v => Math.Pow(v - avg, 2)) / Math.Max(1, values.Count - 1);
            double stdDev = Math.Sqrt(variance);

            bool mlRanSuccessfully = false;

            // 2. Execute ML.NET TimeSeries IID Spike Detection if series has at least 8 data points
            if (series.Count >= 8)
            {
                try
                {
                    var dataView = _mlContext.Data.LoadFromEnumerable(timeSeriesInputs);
                    int historyLength = Math.Clamp(series.Count / 4, 3, 30);

                    var pipeline = _mlContext.Transforms.DetectIidSpike(
                        outputColumnName: nameof(EnergyAnomalyPrediction.Prediction),
                        inputColumnName: nameof(EnergyAnomalyData.Value),
                        confidence: 95.0,
                        pvalueHistoryLength: historyLength
                    );

                    var model = pipeline.Fit(dataView);
                    var transformed = model.Transform(dataView);

                    var predictions = _mlContext.Data
                        .CreateEnumerable<EnergyAnomalyPrediction>(transformed, reuseRowObject: false)
                        .ToList();

                    for (int i = 0; i < series.Count; i++)
                    {
                        var pred = predictions[i];
                        // pred.Prediction: [0] = isAlert (1 or 0), [1] = rawScore, [2] = pValue
                        if (pred.Prediction != null && pred.Prediction.Length >= 3 && pred.Prediction[0] == 1.0)
                        {
                            var rec = series[i];
                            var item = BuildAnomalyItem(
                                meterName, displayName, building, floor,
                                rec.Timestamp, rec.Usage_kWh, (decimal)avg, (decimal)stdDev,
                                pred.Prediction[1], pred.Prediction[2]);

                            results.Add(item);
                        }
                    }

                    mlRanSuccessfully = true;
                }
                catch
                {
                    // Fall back to statistical Z-Score if ML.NET transform encounters numerical edge case
                    mlRanSuccessfully = false;
                }
            }

            // 3. Fallback or Supplementary Statistical Z-Score & Off-hours Heuristic
            if (!mlRanSuccessfully || results.Count == 0)
            {
                for (int i = 0; i < series.Count; i++)
                {
                    var rec = series[i];
                    double val = (double)rec.Usage_kWh;
                    double zScore = stdDev > 0 ? (val - avg) / stdDev : 0;
                    bool isOffHours = IsOffHours(rec.Timestamp);

                    // Threshold: Z-Score > 2.2 (approx 97.2% confidence) or off-hours heavy surge
                    if (zScore > 2.2 || (isOffHours && val > avg * 1.5 && val > 5.0))
                    {
                        double pValue = Math.Max(0.001, Math.Exp(-0.5 * zScore * zScore) / (zScore * Math.Sqrt(2 * Math.PI) + 0.001));
                        var item = BuildAnomalyItem(
                            meterName, displayName, building, floor,
                            rec.Timestamp, rec.Usage_kWh, (decimal)avg, (decimal)stdDev,
                            zScore, pValue);

                        results.Add(item);
                    }
                }
            }

            return results;
        }

        private static EnergyAnomalyResult BuildAnomalyItem(
            string meterName,
            string displayName,
            string building,
            string floor,
            DateTime timestamp,
            decimal actualKwh,
            decimal baselineAvg,
            decimal stdDev,
            double anomalyScore,
            double pValue)
        {
            bool isOffHours = IsOffHours(timestamp);
            decimal expectedKwh = Math.Round(baselineAvg, 2);
            decimal devPercent = expectedKwh > 0 ? Math.Round(((actualKwh - expectedKwh) / expectedKwh) * 100m, 1) : 0m;

            // Classify anomaly type
            string type;
            if (isOffHours && actualKwh > expectedKwh * 1.35m)
                type = "OffHoursLeakage";
            else if (actualKwh > expectedKwh * 1.25m)
                type = "SpikeSurge";
            else if (actualKwh < expectedKwh * 0.4m)
                type = "AbnormalDrop";
            else
                type = "ShiftAnomaly";

            // Classify severity
            string severity;
            if (pValue < 0.015 || devPercent > 60m || (type == "OffHoursLeakage" && actualKwh > 30m))
                severity = "CRITICAL";
            else if (pValue < 0.05 || devPercent > 25m || type == "OffHoursLeakage")
                severity = "WARNING";
            else
                severity = "INFO";

            // Generate actionable explanations
            string explanation;
            string recommendation;

            if (type == "OffHoursLeakage")
            {
                explanation = $"Phantom load detected: {actualKwh:N1} kWh consumed during non-operational period ({timestamp:dddd, HH:mm}), which is +{devPercent:N0}% above typical idle baseline.";
                recommendation = "Conduct Gemba walk; verify if machinery, exhaust blowers, or lighting remained powered on after production shutdown.";
            }
            else if (type == "SpikeSurge")
            {
                explanation = $"Abnormal demand surge of {actualKwh:N1} kWh detected (Normal expected: ~{expectedKwh:N1} kWh, spike +{devPercent:N0}%).";
                recommendation = "Inspect motor windings, compressor cycling, and check for concurrent machine start-up causing instantaneous peak draw.";
            }
            else if (type == "AbnormalDrop")
            {
                explanation = $"Sudden drop to {actualKwh:N1} kWh during scheduled operating hours (Baseline: {expectedKwh:N1} kWh).";
                recommendation = "Verify breaker status, unmonitored line stoppage, or possible metering communication outage.";
            }
            else
            {
                explanation = $"Consumption deviated from standard baseline ({actualKwh:N1} kWh vs ~{expectedKwh:N1} kWh expected).";
                recommendation = "Monitor panel temperature and inspect distribution board breakers for phase imbalance.";
            }

            return new EnergyAnomalyResult
            {
                MeterName = meterName,
                DisplayName = displayName,
                Building = building,
                Floor = floor,
                Timestamp = timestamp,
                ActualKwh = actualKwh,
                ExpectedKwh = expectedKwh,
                AnomalyScore = anomalyScore,
                PValue = pValue,
                AnomalyType = type,
                Severity = severity,
                Explanation = explanation,
                Recommendation = recommendation,
                IsOffHours = isOffHours
            };
        }

        private static bool IsOffHours(DateTime dt)
        {
            // Sunday is full off-day
            if (dt.DayOfWeek == DayOfWeek.Sunday) return true;
            // Saturday afternoon/night
            if (dt.DayOfWeek == DayOfWeek.Saturday && dt.Hour >= 14) return true;
            // Late night shifts / idle hours
            if (dt.Hour >= 23 || dt.Hour < 6) return true;
            return false;
        }
    }
}
