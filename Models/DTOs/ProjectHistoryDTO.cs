namespace AonFreelancing.Models.DTOs;

public class ProjectHistoryDTO
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    ProjectHistoryDTO(Project project)
    {
        Id = project.Id;
        Title = project.Title;
        Description = project.Description;
        StartDate = project.StartDate;
        EndDate = project.EndDate;
    }
    public static ProjectHistoryDTO FromProject(Project project) => new ProjectHistoryDTO(project);
}