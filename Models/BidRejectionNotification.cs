namespace AonFreelancing.Models
{
    public class BidRejectionNotification : Notification
    {
        public long ProjectId { get; set; }
        public long RejectorId { get; set; }
        public string RejectorName { get; set; }
        public long BidId { get; set; } // spare

        public BidRejectionNotification() { }
        public BidRejectionNotification(string title, string message, long receiverId, string? image, long projectId, long rejectorId, string rejectorName, long bidId)
            : base(title, message, receiverId, image)
        {
            ProjectId = projectId;
            RejectorId = rejectorId;
            RejectorName = rejectorName;
            BidId = bidId;
        }
    }
}
