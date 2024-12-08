using AonFreelancing.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {
        public long Id { get; set; }
        public string Title { get; set; }

        public string? Description { get; set; }

        public long ClientId { get; set; } //FK

        // Belongs to a client
        [ForeignKey("ClientId")] 
        public Client? Client { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PriceType { get; set; }
        public int Duration { get; set; }
        public decimal Budget { get; set; }
        public string QualificationName { get; set; }
        public string Status { get; set; }
        public long? FreelancerId { get; set; }
        [ForeignKey("FreelancerId")]
        public Freelancer? Freelancer { get; set; }
        public List<Bid> Bids { get; set; } = new List<Bid>();
        public string? ImageFileName { get; set; }

        public List<TaskEntity>? Tasks { get; set; }
        public List<ProjectLike>? ProjectLikes { get; set; }

        public Project() { }
        Project(ProjectInputDto inputDto,long clientId)
        {
            ClientId = clientId;
            Title = inputDto.Title;
            Description = inputDto.Description;
            QualificationName = inputDto.QualificationName;
            Duration = inputDto.Duration;
            Budget = inputDto.Budget;
            PriceType = inputDto.PriceType;
            CreatedAt = DateTime.Now;
        }
        public static Project FromInputDTO(ProjectInputDto inputDto, long clientId) => new Project(inputDto, clientId);

    }
}
