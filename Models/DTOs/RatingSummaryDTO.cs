namespace AonFreelancing.Models.DTOs
{
    public class RatingSummaryDTO
    {
        public double AverageRating { get; set; }
        public string HighRating { get; set; }
        public string MidRating { get; set; }
        public string LowRating { get; set; }
        public int TotalRating { get; set; }

        public RatingSummaryDTO(double averageRating, string highRating, string midRating, string lowRating, int totalRating)
        {
            AverageRating = averageRating;
            HighRating = highRating;
            MidRating = midRating;
            LowRating = lowRating;
            TotalRating = totalRating;
        }
    }
}
