using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LssTraining.Web.Pages.Participants;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Trainer")]
public class ImportModel(IParticipantRepository repository, ILogger<ImportModel> logger) : PageModel
{
    private const long MaxFileSize = 2 * 1024 * 1024;

    public ImportResult? Result { get; private set; }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync(IFormFile? file, bool skipDuplicates, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a CSV file.");
            return Page();
        }

        if (file.Length > MaxFileSize || !Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "File must be a CSV format and under 2 MB.");
            return Page();
        }

        var result = new ImportResult();
        using var reader = new StreamReader(file.OpenReadStream());
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        var headers = headerLine is null ? [] : ParseCsvLine(headerLine.TrimStart('\uFEFF')).Select(x => x.Trim().ToLowerInvariant()).ToArray();

        if (headers.Length < 2 || !headers.Contains("employeeid") || !headers.Contains("fullname"))
        {
            ModelState.AddModelError(string.Empty, "Headers must include EmployeeId and FullName.");
            return Page();
        }

        var positions = headers.Select((value, index) => (value, index)).ToDictionary(x => x.value, x => x.index);
        var employeeIdsInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lineNumber = 1;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var columns = ParseCsvLine(line);
            var employeeId = GetValue(columns, positions, "employeeid").Trim();
            var fullName = GetValue(columns, positions, "fullname").Trim();

            if (employeeId.Length == 0 || fullName.Length == 0)
            {
                result.Errors.Add($"Row {lineNumber}: EmployeeId and FullName are required.");
                continue;
            }

            if (!employeeIdsInFile.Add(employeeId))
            {
                result.Errors.Add($"Row {lineNumber}: Duplicate Employee ID in file.");
                continue;
            }

            var email = GetValue(columns, positions, "email").Trim();
            if (email.Length > 0 && !new EmailAddressAttribute().IsValid(email))
            {
                result.Errors.Add($"Row {lineNumber}: Invalid email format.");
                continue;
            }

            if (await repository.GetByEmployeeIdAsync(employeeId, cancellationToken) is not null)
            {
                if (skipDuplicates)
                {
                    result.Skipped++;
                }
                else
                {
                    result.Errors.Add($"Row {lineNumber}: Employee ID is already registered.");
                }
                continue;
            }

            try
            {
                var phase = NullIfEmpty(GetValue(columns, positions, "statusphase"));
                if (string.IsNullOrWhiteSpace(phase))
                {
                    phase = NullIfEmpty(GetValue(columns, positions, "phase"));
                }

                await repository.CreateAsync(new Participant
                {
                    EmployeeId = employeeId,
                    FullName = fullName,
                    Department = NullIfEmpty(GetValue(columns, positions, "department")),
                    Position = NullIfEmpty(GetValue(columns, positions, "position")),
                    StatusPhase = phase
                }, User.Identity?.Name, cancellationToken);

                result.Created++;
            }
            catch (InvalidOperationException ex)
            {
                result.Errors.Add($"Row {lineNumber}: {ex.Message}");
            }
        }

        Result = result;
        logger.LogInformation("Import completed: {Created} created, {Skipped} skipped, {Errors} errors", result.Created, result.Skipped, result.Errors.Count);
        return Page();
    }

    private static string GetValue(IReadOnlyList<string> columns, IReadOnlyDictionary<string, int> positions, string name)
    {
        return positions.TryGetValue(name, out var index) && index < columns.Count ? columns[index] : string.Empty;
    }

    private static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        var quoted = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"')
                {
                    value.Append('"');
                    i++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (character == ',' && !quoted)
            {
                values.Add(value.ToString());
                value.Clear();
            }
            else
            {
                value.Append(character);
            }
        }

        values.Add(value.ToString());
        return values;
    }

    public sealed class ImportResult
    {
        public int Created { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; } = [];
    }
}
