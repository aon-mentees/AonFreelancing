using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class EducationInputDTO
    {
        [Required(ErrorMessage = "Institution cannot be null.")]
        public string Institution { get; set; }

        [Required(ErrorMessage = "Degree cannot be null.")]
        public string Degree { get; set; }

        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
    }
}
