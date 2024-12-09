using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using AonFreelancing.Utilities;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutDTO
    {
        public long Id { get; set; }
        public int Duration {  get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Qualifications { get; set; }
        public string PriceType { get; set; }
        public string Status {  get; set; }
        public decimal Budget {  get; set; }
        public DateTime CreatedAt { get; set; }
        //public DateTime? StartDate {  get; set; }
        //public DateTime? EndDate { get; set; }
        public string? CreationTime {  get; set; }
        public string ClientName {  get; set; }
        public long ClientId {  get; set; }
        public bool IsLiked {  get; set; }

        public string ImageUrl {  get; set; }

        [JsonPropertyName("likes")]
        public long LikesCount {  get; set; }
        ProjectOutDTO(Project project,string imageBaseUrl)
        {
            Id = project.Id;
            Duration = project.Duration;
            Title = project.Title;
            Description = project.Description;
            Qualifications = project.QualificationName;
            PriceType = project.PriceType;
            Status = project.Status;
            Budget = project.Budget;
            CreatedAt = project.CreatedAt;
            //StartDate = project.StartDate;
            //EndDate = project.EndDate;
            CreationTime = StringOperations.GetTimeAgo(CreatedAt);
            ClientName = project.Client.Name;
            ClientId = project.Client.Id;
            LikesCount = project.ProjectLikes.Count();
            if (project.ImageFileName != null)
                ImageUrl = $"{imageBaseUrl}/{project.ImageFileName}";
        }
        public static ProjectOutDTO FromProject(Project project,string imageBaseUrl) => new ProjectOutDTO(project,imageBaseUrl);

    }
}
