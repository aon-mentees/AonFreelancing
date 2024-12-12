namespace AonFreelancing.Models.DTOs
{
    public class RatingOutputDTO
    {
        public long Id { get; set; }
        public long RaterUserId { get; set; }
        public long RatedUserId { get; set; }
        public double RatingValue { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public RatingOutputDTO(Rating rating) 
        { 
            Id = rating.Id;
            RaterUserId = rating.RaterUserId;
            RatedUserId = rating.RatedUserId;
            RatingValue = rating.RatingValue;
            Comment = rating.Comment;
            CreatedAt = rating.CreatedAt;
        }
        
    }
}
