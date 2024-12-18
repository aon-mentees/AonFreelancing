using AonFreelancing.Models.DTOs;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("WorkExperiences")]
    public class WorkExperience
    {
        public long Id { get; set; }
        public Freelancer Freelancer { get; set; }
        public long FreelancerId { get; set; }
        public string JobTitle { get; set; }
        public string EmployerName { get; set; }
        public string EmploymentType { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public WorkExperience() { }
        WorkExperience(long freelancerId, WorkExperienceInputDTO inputDTO)
        {
            FreelancerId = freelancerId;
            JobTitle = inputDTO.JobTitle;
            EmployerName = inputDTO.EmployerName;
            EmploymentType = inputDTO.EmploymentType; ;
            IsCurrent = inputDTO.IsCurrent;
            StartDate = inputDTO.StartDate;
            EndDate = inputDTO.EndDate;
        }
        public static WorkExperience FromWorkExperienceinputDTO(WorkExperienceInputDTO inputDTO, long freelancerId) => new WorkExperience(freelancerId, inputDTO);

    }
}
