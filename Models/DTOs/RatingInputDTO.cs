using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class RatingInputDTO
    {
        public long RatedUserId { get; set; }
        [Range(1, 10)]
        public double RatingValue { get; set; }

        public string? Comment { get; set; }
    }
}