using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Models
{
    [Table("Ratings")]
    public class Rating
    {
        [Key]
        public long Id { get; set; }

        public long RaterUserId { get; set; }
        public long RatedUserId { get; set; }
        public double RatingValue { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Rating() { }
        public Rating(RatingInputDTO ratingInputDTO, long raterUserId)
        {
            RaterUserId = raterUserId;
            RatedUserId = ratingInputDTO.RatedUserId;
            RatingValue = ratingInputDTO.RatingValue;
            Comment = ratingInputDTO.Comment;
            CreatedAt = DateTime.Now;
        }

        //public static Rating FromRatingDTO() {  return new Rating(); }
    }
}