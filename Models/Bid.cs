using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    public class Bid
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        
        public Project Project { get; set; }
        public long FreelancerId { get; set; }
        public Freelancer Freelancer { get; set; }
        public decimal ProposedPrice { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        public Bid() { }

        Bid(BidInputDto inputDto,long freelancerId,long projectId)
        {
            FreelancerId = freelancerId;
            ProjectId = projectId;
            ProposedPrice = inputDto.ProposedPrice;
            Notes = inputDto.Notes;
            Status = Constants.BIDS_STATUS_PENDING;
            SubmittedAt = DateTime.Now;
        }

        public static Bid FromInputDTO(BidInputDto inputDto,long freelancerId,long projectId) =>new Bid(inputDto,freelancerId,projectId);
    }
}
