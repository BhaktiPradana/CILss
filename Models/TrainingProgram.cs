namespace LssTraining.Web.Models;

public static class TrainingNames
{
    public const string WhiteBelt = "Training White Belt";
    public const string YellowBelt = "Training & Certification Yellow Belt";
    public const string GreenBelt = "Training & Certification Green Belt + TOC";
    public const string BlackBelt = "Certified Black Belt";
}

public sealed class TrainingProgram
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsGreenBelt { get; set; }
}

public sealed class TrainingModule
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetHours { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
}
