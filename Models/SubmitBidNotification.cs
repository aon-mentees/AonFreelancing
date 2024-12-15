namespace AonFreelancing.Models
{
    public class SubmitBidNotification:Notification
    {

        public long ProjectId { get; set; }
        public long FreelancerId { get; set; }
        public string FreelancerName { get; set; }

        public SubmitBidNotification() { }

        public SubmitBidNotification(string title, string message, long receiverId, string? image, long projectId, long freelancerId, string freelancerName)
        : base(title, message, receiverId, image)
        {
            ProjectId = projectId;
            FreelancerId =freelancerId;
            FreelancerName = freelancerName;
        }
    }
}
