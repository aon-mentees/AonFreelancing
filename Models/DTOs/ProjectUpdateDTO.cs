using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectUpdateDTO
    {
        [Required]
        [MaxLength(512, ErrorMessage = "Title is too long.")]
        public string Title { get; set; }

        [MaxLength(1024, ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        [Required]
        [AllowedValues(["uiux", "mobile", "frontend", "backend", "fullstack"])]
        public string QualificationName { get; set; }

        [Required]
        [Range(1, 365)]
        public int Duration { get; set; } //Number of days

        //[AllowedValues(Constants.PROJECT_PRICETYPE_PERHOUR, Constants.PROJECT_PRICETYPE_FIXED, ErrorMessage ="Price type is invalid.")]
        //public string? PriceType { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Budget { get; set; }
    }
}
