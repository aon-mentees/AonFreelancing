using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class EducationInputDTO
    {
        [Required(ErrorMessage = "Institution cannot be null.")]
        public string Institution { get; set; }

        [Required(ErrorMessage = "Degree cannot be null.")]
        public string Degree { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
