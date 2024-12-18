namespace AonFreelancing.Models.DTOs
{
    public class WorkExperienceOutputDTO
    {
        public long Id { get; set; }
        public string EmployerName { get; set; } 
        public string JobTitle { get; set; }
        public string EmploymentType { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public WorkExperienceOutputDTO(WorkExperience workExperience)
        {
            Id = workExperience.Id;
            EmployerName = workExperience.EmployerName;
            JobTitle = workExperience.JobTitle;
            EmploymentType = workExperience.EmploymentType;
            IsCurrent = workExperience.IsCurrent;
            StartDate = workExperience.StartDate;
            EndDate = workExperience.EndDate;
        }

        public static WorkExperienceOutputDTO FromWorkExperience(WorkExperience workExperience) => new WorkExperienceOutputDTO(workExperience);
    }
}
