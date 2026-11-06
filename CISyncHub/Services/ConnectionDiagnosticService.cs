using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace CISyncHub.Services;

public class ConnectionDiagnosticService
{
    private readonly Action<string> _logger;

    public ConnectionDiagnosticService(Action<string> logger)
    {
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync(string connStr, string label, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger($"Testing connection to [{label}]...");
            using var conn = new SqlConnection(connStr);
            var connectTask = conn.OpenAsync(cancellationToken);
            if (await Task.WhenAny(connectTask, Task.Delay(8000, cancellationToken)) == connectTask)
            {
                await connectTask;
                _logger($"✅ Connection to [{label}] SUCCESSFUL.");
                return true;
            }
            else
            {
                _logger($"❌ Connection to [{label}] TIMED OUT (8s).");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger($"❌ Connection to [{label}] FAILED: {ex.Message}");
            return false;
        }
    }
}

