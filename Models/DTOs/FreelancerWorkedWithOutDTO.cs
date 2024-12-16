namespace AonFreelancing.Models.DTOs
{
    public class FreelancerWorkedWithOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastWorkDate { get; set; }
        public double Rating { get; set; }
        public string ProfilePicture { get; set; }
        public FreelancerWorkedWithOutDTO(Freelancer freelancer, double rating, DateTime? lastWorkDate, string imageBaseUrl)
        {
            Id = freelancer.Id;
            Name = freelancer.Name;
            LastWorkDate = lastWorkDate;
            Rating = rating;
            ProfilePicture = $"{imageBaseUrl}/{freelancer.ProfilePicture}";
        }
    }
}
