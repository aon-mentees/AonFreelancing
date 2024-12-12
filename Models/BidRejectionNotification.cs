namespace AonFreelancing.Models
{
    public class BidRejectionNotification : Notification
    {
        public long ProjectId { get; set; }
        public long RejectorId { get; set; }
        public string RejectorName { get; set; }
        public long BidId { get; set; } // spare

        public BidRejectionNotification() { }
        public BidRejectionNotification(string title, string message, long receiverId, long projectId, long rejectorId, string rejectorName, long bidId)
            : base(title, message, receiverId)
        {
            ProjectId = projectId;
            RejectorId = rejectorId;
            RejectorName = rejectorName;
            BidId = bidId;
        }
    }
}
