using System;
using System.Collections.Generic;
using Microsoft.ML.Data;

namespace LssTraining.Web.Models
{
    /// <summary>
    /// Input data schema for ML.NET TimeSeries Anomaly Detection.
    /// </summary>
    public class EnergyAnomalyData
    {
        public DateTime Timestamp { get; set; }
        public float Value { get; set; }
    }

    /// <summary>
    /// Output prediction schema produced by ML.NET TimeSeries anomaly transformers.
    /// </summary>
    public class EnergyAnomalyPrediction
    {
        // Vector: [0] = Alert (1.0 = Anomaly, 0.0 = Normal), [1] = RawScore, [2] = P-Value
        [VectorType(3)]
        public double[] Prediction { get; set; } = new double[3];
    }

    /// <summary>
    /// Detailed anomaly result enriched with business context, severity, and recommendations.
    /// </summary>
    public class EnergyAnomalyResult
    {
        public string MeterName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal ActualKwh { get; set; }
        public decimal ExpectedKwh { get; set; }
        public decimal DeviationKwh => ActualKwh - ExpectedKwh;
        public decimal DeviationPercent => ExpectedKwh > 0 ? ((ActualKwh - ExpectedKwh) / ExpectedKwh) * 100m : 0m;
        public double AnomalyScore { get; set; }
        public double PValue { get; set; }
        public double ConfidencePercent => Math.Clamp(Math.Round((1.0 - PValue) * 100.0, 1), 50.0, 99.9);
        public string AnomalyType { get; set; } = "SpikeSurge"; // SpikeSurge, OffHoursLeakage, AbnormalDrop, ShiftAnomaly
        public string Severity { get; set; } = "WARNING"; // CRITICAL, WARNING, INFO
        public string Explanation { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public bool IsOffHours { get; set; }
    }

    /// <summary>
    /// Summary of ML Anomaly Detection results across all distribution boards.
    /// </summary>
    public class EnergyAnomalySummary
    {
        public int TotalAnomaliesDetected { get; set; }
        public int CriticalAnomaliesCount { get; set; }
        public int WarningAnomaliesCount { get; set; }
        public int OffHoursLeakagesCount { get; set; }
        public decimal TotalEstimatedWastedKwh { get; set; }
        public DateTime AnalysisTimestamp { get; set; } = DateTime.Now;
        public List<EnergyAnomalyResult> TopAnomalies { get; set; } = new();
        public HashSet<string> AnomalousMeterNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
