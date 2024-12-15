namespace AonFreelancing.Models.DTOs
{
    public class BidOutputDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }

        //public ProjectOutDTO Project { get; set; }
        //public long FreelancerId { get; set; }
        public FreelancerShortOutDTO Freelancer { get; set; }
        public decimal ProposedPrice { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public DateTime? ApprovedAt { get; set; }

        BidOutputDTO(Bid bid, string imageBaseUrl)
        {
            Id = bid.Id;
            //FreelancerId = bid.FreelancerId;
            ProjectId = bid.ProjectId;
            Freelancer = FreelancerShortOutDTO.FromFreelancer(bid.Freelancer,imageBaseUrl);
            ProposedPrice = bid.ProposedPrice;
            Notes = bid.Notes;
            Status = bid.Status;
            SubmittedAt = bid.SubmittedAt;
            ApprovedAt = bid.ApprovedAt;
        }
        public static BidOutputDTO FromBid(Bid bid, string imageBaseUrl) => new BidOutputDTO(bid, imageBaseUrl);

    }
}
