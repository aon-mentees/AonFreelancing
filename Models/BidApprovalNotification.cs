namespace AonFreelancing.Models
{
    public class BidApprovalNotification : Notification
    {
        public long ProjectId { get; set; }
        public long ApproverId { get; set; }
        public string ApproverName { get; set; }
        public long BidId { get; set; } // spare

        public BidApprovalNotification() { }
        public BidApprovalNotification(string title, string message, long receiverId,string? image, long projectId, long approverId, string approverName, long bidId)
            : base(title, message, receiverId, image) 
        {
            ProjectId = projectId;
            ApproverId = approverId;
            ApproverName = approverName;
            BidId = bidId;
        }
    }
}
