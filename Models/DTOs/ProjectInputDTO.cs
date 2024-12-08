using AonFreelancing.Attributes;
using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDto
    {
        [Required]
        [MaxLength(512, ErrorMessage ="Title is too long.")]
        public string Title { get; set; }

        [MaxLength(1024,ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        [Required]
        [AllowedValues(["uiux", "mobile", "frontend", "backend", "fullstack"])]
        public string QualificationName { get; set; }

        [Required]
        [Range(1, 365)]
        public int Duration { get; set; } //Number of days

        [Required]
        [AllowedValues(Constants.PROJECT_PRICETYPE_PERHOUR, Constants.PROJECT_PRICETYPE_FIXED, ErrorMessage ="Price type is invalid.")]
        public string? PriceType { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Budget { get; set; }

        [MaxFileSize(1024 * 1024 * 5)]
        [AllowedFileExtensions([".jpg", ".jpeg", ".png"])]
        public IFormFile? ImageFile { get; set; }
    }
}
