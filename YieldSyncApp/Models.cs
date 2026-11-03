using System;

namespace YieldSyncApp;

public class YieldRecord
{
    public string DeviceCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal FirstPassYield { get; set; }
    public decimal OverallYield { get; set; }
    public string Division { get; set; } = string.Empty;
    public double TotalQtyIn { get; set; }
    public double TotalQtyOut { get; set; }
    public double TotalDefectFirst { get; set; }
    public double TotalDefectFinal { get; set; }
    public string SourceServer { get; set; } = string.Empty;
    public string SourceDatabase { get; set; } = string.Empty;
}

public class SyncSourceConfig
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

public class SyncResult
{
    public string Division { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int RecordsExtracted { get; set; }
    public int RecordsUpserted { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}

