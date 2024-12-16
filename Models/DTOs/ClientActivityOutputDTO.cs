using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AonFreelancing.Models.DTOs
{
    public class ClientActivityOutputDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public long? TotalLikes { get; set; }
        public long? TotalComment { get; set; }
        public string CreationTime { get; set; }
        public int Duration { get; set; }
        public string DurationString { get; set; }
        public decimal Budget { get; set; }

        ClientActivityOutputDTO(Project project)
        {
            Id = project.Id;
            Title = project.Title;
            Description = project.Description;
            CreationTime= StringOperations.GetTimeAgo( project.CreatedAt);
            TotalLikes = project.ProjectLikes.Count;
            TotalComment = project.Comments.Count;
            Duration = project.Duration;
            DurationString = StringOperations.ConvertDaysToMonthsAndYears(project.Duration);
            Budget = project.Budget;
        }
        public static ClientActivityOutputDTO FromClientActivity(Project project) => new ClientActivityOutputDTO(project);
    }
   
}
