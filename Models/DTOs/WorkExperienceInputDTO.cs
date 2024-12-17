using static AonFreelancing.Utilities.Constants;

namespace AonFreelancing.Models.DTOs
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class WorkExperienceInputDTO : IValidatableObject
    {
        [Required(ErrorMessage = "JobTitle is required.")]
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "Company is required.")]
        public string EmployerName { get; set; }

        [Required(ErrorMessage = "EmploymentType is required.")]
        [AllowedValues(EMPLOYMENTTYPE_FULLTIME, EMPLOYMENTTYPE_PARTTIME, 
            EMPLOYMENTTYPE_CONTRACT, EMPLOYMENTTYPE_INTERNSHIP, 
            ErrorMessage = "EmploymentType must be one of the following: Full-time, Part-time, Contract, Internship.")]
        public string EmploymentType { get; set; }

        public bool IsCurrent { get; set; }

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return WorkExperienceValidator.Validate(this);
        }
    }
}
