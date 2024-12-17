namespace AonFreelancing.Models.DTOs
{
    public class RatingSummaryDTO
    {
        public double AverageRating { get; set; }
        public double HighRating { get; set; }
        public double MidRating { get; set; }
        public double LowRating { get; set; }
        public int TotalRating { get; set; }

        public RatingSummaryDTO(double averageRating, double highRating, double midRating, double lowRating, int totalRating)
        {
            AverageRating = averageRating;
            HighRating = highRating;
            MidRating = midRating;
            LowRating = lowRating;
            TotalRating = totalRating;
        }
    }
}
