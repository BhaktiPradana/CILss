using ClosedXML.Excel;
using Dapper;
using LssTraining.Web.Data;
using LssTraining.Web.Models;

namespace LssTraining.Web.Services;

public sealed class ParticipantExportService(IDbConnectionFactory connectionFactory, ILogger<ParticipantExportService> logger) : IParticipantExportService
{
    public async Task<byte[]> GenerateExcelExportAsync(CancellationToken cancellationToken = default)
    {
        List<Participant> participants;
        List<EnrollmentExportRow> enrollments;
        List<ModuleExportRow> modules;
        List<GreenBeltReview> reviews;

        try
        {
            using var connection = connectionFactory.CreateConnection();

            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO ParticipantModules (EnrollmentId, ModuleId, ActualHours, Status, IsOverridden, UpdatedAt)
                SELECT e.Id, tm.Id, 0, 'Not Started', 0, SYSUTCDATETIME()
                FROM Enrollments e
                JOIN TrainingModules tm ON tm.ProgramId = e.ProgramId
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ParticipantModules pm
                    WHERE pm.EnrollmentId = e.Id AND pm.ModuleId = tm.Id
                );
                """, cancellationToken: cancellationToken));

            participants = (await connection.QueryAsync<Participant>(new CommandDefinition(
                "SELECT * FROM Participants ORDER BY FullName, Id;",
                cancellationToken: cancellationToken))).ToList();

            enrollments = (await connection.QueryAsync<EnrollmentExportRow>(new CommandDefinition("""
                SELECT e.Id AS EnrollmentId,
                       e.ParticipantId,
                       e.ProgramId,
                       e.Status,
                       e.TocFlag,
                       e.EnrolledAt,
                       e.CertifiedAt,
                       tp.Name AS ProgramName,
                       tp.ShortName,
                       tp.IsGreenBelt
                FROM Enrollments e
                JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
                ORDER BY e.EnrolledAt ASC, e.Id ASC;
                """, cancellationToken: cancellationToken))).ToList();

            modules = (await connection.QueryAsync<ModuleExportRow>(new CommandDefinition("""
                SELECT pm.Id,
                       pm.EnrollmentId,
                       pm.ModuleId,
                       m.Name AS ModuleName,
                       m.TargetHours,
                       pm.ActualHours,
                       pm.Status,
                       pm.IsOverridden,
                       pm.OverrideReason
                FROM ParticipantModules pm
                JOIN TrainingModules m ON m.Id = pm.ModuleId
                ORDER BY m.SortOrder, m.Id;
                """, cancellationToken: cancellationToken))).ToList();

            reviews = (await connection.QueryAsync<GreenBeltReview>(new CommandDefinition("""
                SELECT * FROM GreenBeltReviews ORDER BY ReviewNumber;
                """, cancellationToken: cancellationToken))).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database query failed during Excel export.");
            throw new InvalidOperationException("Failed to query database for Excel export: " + ex.Message, ex);
        }

        using var workbook = new XLWorkbook();

        CreateSummarySheet(workbook, participants, enrollments, modules);

        var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Participant List" };

        foreach (var participant in participants)
        {
            var pEnrollments = enrollments.Where(e => e.ParticipantId == participant.Id).ToList();
            CreateParticipantSheet(workbook, participant, pEnrollments, modules, reviews, usedSheetNames);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void CreateSummarySheet(
        IXLWorkbook workbook,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<EnrollmentExportRow> enrollments,
        IReadOnlyList<ModuleExportRow> modules)
    {
        var sheet = workbook.Worksheets.Add("Participant List");
        sheet.ShowGridLines = true;

        sheet.Cell(1, 1).Value = "ALL PARTICIPANTS DIRECTORY - CILEANSIXSIGMA";
        sheet.Range(1, 1, 1, 9).Merge();
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
        sheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#263B69");
        sheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Row(1).Height = 28;
        var headers = new[]
        {
            "No",
            "Employee ID",
            "Full Name",
            "Department",
            "Position",
            "Status Phase (DMAIC)",
            "Status",
            "Enrolled Programs",
            "Certification / Progress Status"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(3, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F6FED");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
        }
        sheet.Row(3).Height = 24;

        var row = 4;
        var num = 1;
        foreach (var p in participants)
        {
            var pEnrollments = enrollments.Where(e => e.ParticipantId == p.Id).ToList();
            var programNames = pEnrollments.Count > 0
                ? string.Join(", ", pEnrollments.Select(e => e.ProgramName + (e.TocFlag ? " (+ TOC)" : "")))
                : "Not enrolled";

            var pEnrollmentIds = pEnrollments.Select(e => e.EnrollmentId).ToHashSet();
            var pMods = modules.Where(m => pEnrollmentIds.Contains(m.EnrollmentId)).ToList();
            var compModsCount = pMods.Count(m => m.Status == "Completed" || m.Status == "Overridden" || m.ActualHours >= m.TargetHours);
            var totalModsCount = pMods.Count;
            var remainingModsCount = Math.Max(0, totalModsCount - compModsCount);

            var statusDesc = "No Programs";
            if (pEnrollments.Any(e => e.Status == "Certified"))
            {
                var certDates = pEnrollments
                    .Where(e => e.Status == "Certified")
                    .Select(e => e.CertifiedAt.HasValue ? e.CertifiedAt.Value.ToString("dd/MM/yyyy") : "Yes");
                statusDesc = "Certified (" + string.Join(", ", certDates) + ") · Modul Lengkap (" + compModsCount + "/" + totalModsCount + ")";
            }
            else if (pEnrollments.Any(e => e.Status == "Completed") || (totalModsCount > 0 && compModsCount >= totalModsCount))
            {
                statusDesc = "Completed Training · Modul Lengkap (" + compModsCount + "/" + totalModsCount + " · Kurang 0)";
            }
            else if (pEnrollments.Count > 0)
            {
                statusDesc = "In Training · Sampai Modul " + compModsCount + "/" + totalModsCount + " (Kurang " + remainingModsCount + " modul)";
            }

            sheet.Cell(row, 1).Value = num++;
            sheet.Cell(row, 2).Value = p.EmployeeId;
            sheet.Cell(row, 3).Value = p.FullName;
            sheet.Cell(row, 4).Value = p.Department ?? "-";
            sheet.Cell(row, 5).Value = p.Position ?? "-";
            sheet.Cell(row, 6).Value = string.IsNullOrWhiteSpace(p.StatusPhase) ? "Not Started" : p.StatusPhase;
            sheet.Cell(row, 7).Value = p.IsActive ? "Active" : "Inactive";
            sheet.Cell(row, 8).Value = programNames;
            sheet.Cell(row, 9).Value = statusDesc;

            var isEven = row % 2 == 0;
            var fill = isEven ? XLColor.FromHtml("#F8FAFC") : XLColor.White;

            for (var c = 1; c <= 9; c++)
            {
                var cell = sheet.Cell(row, c);
                cell.Style.Fill.BackgroundColor = fill;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            sheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            sheet.Row(row).Height = 20;
            row++;
        }

        sheet.Columns().AdjustToContents();
        sheet.Column(1).Width = 6;
        sheet.Column(2).Width = 14;
        sheet.Column(3).Width = 24;
        sheet.Column(8).Width = 32;
        sheet.Column(9).Width = 26;
    }

    private static void CreateParticipantSheet(
        IXLWorkbook workbook,
        Participant participant,
        IReadOnlyList<EnrollmentExportRow> enrollments,
        IReadOnlyList<ModuleExportRow> allModules,
        IReadOnlyList<GreenBeltReview> allReviews,
        HashSet<string> usedSheetNames)
    {
        var sheetName = GetSafeSheetName(participant.FullName, participant.EmployeeId, usedSheetNames);
        var sheet = workbook.Worksheets.Add(sheetName);
        sheet.ShowGridLines = true;

        sheet.Cell(1, 1).Value = "PARTICIPANT PROFILE & STATUS: " + (participant.FullName ?? "PARTICIPANT").ToUpperInvariant();
        sheet.Range(1, 1, 1, 6).Merge();
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 13;
        sheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
        sheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#263B69");
        sheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Row(1).Height = 26;

        sheet.Cell(3, 1).Value = "IDENTITY & ASSIGNMENT INFORMATION";
        sheet.Range(3, 1, 3, 6).Merge();
        sheet.Cell(3, 1).Style.Font.Bold = true;
        sheet.Cell(3, 1).Style.Font.FontSize = 10;
        sheet.Cell(3, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");

        var profileLabels = new (string Label, string Value, string Col2Label, string Col2Value)[]
        {
            ("Employee ID", participant.EmployeeId ?? "-", "Full Name", participant.FullName ?? "-"),
            ("Department", participant.Department ?? "-", "Position", participant.Position ?? "-"),
            ("Status Phase (DMAIC)", string.IsNullOrWhiteSpace(participant.StatusPhase) ? "Not Started" : participant.StatusPhase, "Total Programs Enrolled", enrollments.Count.ToString())
        };

        var pRow = 4;
        foreach (var item in profileLabels)
        {
            sheet.Cell(pRow, 1).Value = item.Label;
            sheet.Cell(pRow, 1).Style.Font.Bold = true;
            sheet.Cell(pRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            sheet.Cell(pRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Cell(pRow, 1).Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");

            sheet.Cell(pRow, 2).Value = item.Value;
            sheet.Cell(pRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Cell(pRow, 2).Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");

            sheet.Cell(pRow, 4).Value = item.Col2Label;
            sheet.Cell(pRow, 4).Style.Font.Bold = true;
            sheet.Cell(pRow, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            sheet.Cell(pRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Cell(pRow, 4).Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");

            sheet.Cell(pRow, 5).Value = item.Col2Value;
            sheet.Range(pRow, 5, pRow, 6).Merge();
            sheet.Cell(pRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Cell(pRow, 5).Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");

            pRow++;
        }

        var curRow = pRow + 1;

        if (enrollments.Count == 0)
        {
            sheet.Cell(curRow, 1).Value = "Participant is not enrolled in any training programs.";
            sheet.Range(curRow, 1, curRow, 6).Merge();
            sheet.Cell(curRow, 1).Style.Font.Italic = true;
            sheet.Cell(curRow, 1).Style.Font.FontColor = XLColor.Gray;
        }
        else
        {
            foreach (var enr in enrollments)
            {
                sheet.Cell(curRow, 1).Value = "PROGRAM: " + (enr.ProgramName ?? "PROGRAM").ToUpperInvariant();
                sheet.Range(curRow, 1, curRow, 6).Merge();
                sheet.Cell(curRow, 1).Style.Font.Bold = true;
                sheet.Cell(curRow, 1).Style.Font.FontSize = 11;
                sheet.Cell(curRow, 1).Style.Font.FontColor = XLColor.White;
                sheet.Cell(curRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
                sheet.Row(curRow).Height = 22;
                curRow++;

                sheet.Cell(curRow, 1).Value = "Program Status:";
                sheet.Cell(curRow, 1).Style.Font.Bold = true;
                sheet.Cell(curRow, 2).Value = enr.Status ?? "In Progress";
                sheet.Cell(curRow, 3).Value = "Enrolled Date:";
                sheet.Cell(curRow, 3).Style.Font.Bold = true;
                sheet.Cell(curRow, 4).Value = enr.EnrolledAt.ToString("dd/MM/yyyy");
                sheet.Cell(curRow, 5).Value = "Certification Date:";
                sheet.Cell(curRow, 5).Style.Font.Bold = true;
                sheet.Cell(curRow, 6).Value = enr.CertifiedAt.HasValue ? enr.CertifiedAt.Value.ToString("dd/MM/yyyy HH:mm") : "-";

                for (var sc = 1; sc <= 6; sc++)
                {
                    sheet.Cell(curRow, sc).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                    sheet.Cell(curRow, sc).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Cell(curRow, sc).Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                }
                curRow += 2;

                sheet.Cell(curRow, 1).Value = "Training Curriculum Modules";
                sheet.Range(curRow, 1, curRow, 5).Merge();
                sheet.Cell(curRow, 1).Style.Font.Bold = true;
                sheet.Cell(curRow, 1).Style.Font.FontColor = XLColor.White;
                sheet.Cell(curRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#18A889");
                curRow++;

                var modHeaders = new[] { "No", "Module Name", "Target Hours", "Actual Hours", "Module Status" };
                for (var mi = 0; mi < modHeaders.Length; mi++)
                {
                    var mCell = sheet.Cell(curRow, mi + 1);
                    mCell.Value = modHeaders[mi];
                    mCell.Style.Font.Bold = true;
                    mCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6F4F1");
                    mCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    mCell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
                    mCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                curRow++;

                var enrModules = allModules.Where(m => m.EnrollmentId == enr.EnrollmentId).ToList();
                var modIdx = 1;
                foreach (var mod in enrModules)
                {
                    sheet.Cell(curRow, 1).Value = modIdx++;
                    sheet.Cell(curRow, 2).Value = mod.ModuleName ?? "-";
                    sheet.Cell(curRow, 3).Value = mod.TargetHours;
                    sheet.Cell(curRow, 4).Value = mod.ActualHours;
                    sheet.Cell(curRow, 5).Value = mod.Status ?? "Not Started";

                    for (var mc = 1; mc <= 5; mc++)
                    {
                        var cell = sheet.Cell(curRow, mc);
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                    }
                    sheet.Cell(curRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    sheet.Cell(curRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    sheet.Cell(curRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    sheet.Cell(curRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    curRow++;
                }

                curRow++;

                if (enr.IsGreenBelt)
                {
                    sheet.Cell(curRow, 1).Value = "Project Review Gate Status (R0 — R5)";
                    sheet.Range(curRow, 1, curRow, 5).Merge();
                    sheet.Cell(curRow, 1).Style.Font.Bold = true;
                    sheet.Cell(curRow, 1).Style.Font.FontColor = XLColor.White;
                    sheet.Cell(curRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
                    curRow++;

                    var revHeaders = new[] { "Review", "DMAIC Phase", "Review Status", "Review Date", "Trainer Comments / Notes" };
                    for (var ri = 0; ri < revHeaders.Length; ri++)
                    {
                        var rCell = sheet.Cell(curRow, ri + 1);
                        rCell.Value = revHeaders[ri];
                        rCell.Style.Font.Bold = true;
                        rCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
                        rCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rCell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
                        rCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    curRow++;

                    var enrReviews = allReviews.Where(r => r.EnrollmentId == enr.EnrollmentId).OrderBy(r => r.ReviewNumber).ToList();
                    foreach (var rev in enrReviews)
                    {
                        sheet.Cell(curRow, 1).Value = "Review " + (rev.ReviewNumber + 1) + " (R" + rev.ReviewNumber + ")";
                        sheet.Cell(curRow, 2).Value = rev.DmaicStage ?? "-";
                        sheet.Cell(curRow, 3).Value = rev.Status ?? "Not Started";
                        sheet.Cell(curRow, 4).Value = rev.ReviewedAt.HasValue ? rev.ReviewedAt.Value.ToString("dd/MM/yyyy") : "-";
                        sheet.Cell(curRow, 5).Value = rev.TrainerComment ?? "-";

                        for (var rc = 1; rc <= 5; rc++)
                        {
                            var cell = sheet.Cell(curRow, rc);
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                        }
                        sheet.Cell(curRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        sheet.Cell(curRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        sheet.Cell(curRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        sheet.Cell(curRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        curRow++;
                    }

                    curRow++;
                }
            }
        }

        sheet.Columns().AdjustToContents();
        sheet.Column(1).Width = 20;
        sheet.Column(2).Width = 30;
        sheet.Column(3).Width = 16;
        sheet.Column(4).Width = 18;
        sheet.Column(5).Width = 35;
    }

    private static string GetSafeSheetName(string? fullName, string? employeeId, HashSet<string> usedNames)
    {
        var raw = !string.IsNullOrWhiteSpace(fullName) ? fullName : (!string.IsNullOrWhiteSpace(employeeId) ? employeeId : "Participant");
        var sanitized = new string(raw.Select(ch => "\\/?*[]:\0\r\n\t".Contains(ch) ? ' ' : ch).ToArray()).Trim();
        sanitized = sanitized.Trim('\'', ' ');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = !string.IsNullOrWhiteSpace(employeeId) ? employeeId.Trim('\'', ' ') : "Participant";
        }

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "Participant";
        }

        if (sanitized.Length > 25)
        {
            sanitized = sanitized[..25].Trim().Trim('\'', ' ');
        }

        var candidate = sanitized;
        var counter = 2;
        while (usedNames.Contains(candidate))
        {
            var suffix = $" ({counter})";
            var maxBaseLen = Math.Max(1, 31 - suffix.Length);
            var baseName = sanitized.Length > maxBaseLen ? sanitized[..maxBaseLen].Trim().Trim('\'', ' ') : sanitized;
            candidate = $"{baseName}{suffix}";
            counter++;
        }

        usedNames.Add(candidate);
        return candidate;
    }

    private sealed class EnrollmentExportRow
    {
        public int EnrollmentId { get; set; }
        public int ParticipantId { get; set; }
        public int ProgramId { get; set; }
        public string Status { get; set; } = "In Progress";
        public bool TocFlag { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime? CertifiedAt { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public bool IsGreenBelt { get; set; }
    }

    private sealed class ModuleExportRow
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public decimal TargetHours { get; set; }
        public decimal ActualHours { get; set; }
        public string Status { get; set; } = "Not Started";
        public bool IsOverridden { get; set; }
        public string? OverrideReason { get; set; }
    }
}