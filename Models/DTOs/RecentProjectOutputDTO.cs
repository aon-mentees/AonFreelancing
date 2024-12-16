namespace AonFreelancing.Models.DTOs
{
    public class RecentProjectOutputDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Budget { get; set; }
        RecentProjectOutputDTO(Project project, string imageBaseUrl)
        {
            Id = project.Id;
            Title = project.Title;
            Description = project.Description;
            Budget = project.Budget;
            if (project.ImageFileName != null)
                ImageUrl = $"{imageBaseUrl}/{project.ImageFileName}";
        }

        public static RecentProjectOutputDTO FromProject(Project project, string imageBaseUrl) => new RecentProjectOutputDTO(project, imageBaseUrl);
    }
}
