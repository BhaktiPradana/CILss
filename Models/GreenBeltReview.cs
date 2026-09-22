namespace LssTraining.Web.Models;

public sealed class GreenBeltReview
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public int ReviewNumber { get; set; }
    public string ReviewLabel => $"R{ReviewNumber}";
    public string Status { get; set; } = "Not Started";
    public string? DmaicStage { get; set; }
    public string? TrainerComment { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
