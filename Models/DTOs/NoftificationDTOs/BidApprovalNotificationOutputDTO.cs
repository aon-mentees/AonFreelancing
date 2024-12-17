namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class BidApprovalNotificationOutputDTO : NotificationOutputDTO
    {
        public long ProjectId { get; set; }
        public long ApproverId { get; set; }
        public string ApproverName { get; set; }
        public long BidId { get; set; } // spare

        BidApprovalNotificationOutputDTO(BidApprovalNotification bidApprovalNotification) : base(bidApprovalNotification.Id, bidApprovalNotification.Title, bidApprovalNotification.Message, bidApprovalNotification.IsRead, bidApprovalNotification.CreatedAt, bidApprovalNotification.Image)
        {
            ProjectId = bidApprovalNotification.ProjectId;
            ApproverId = bidApprovalNotification.ApproverId;
            ApproverName = bidApprovalNotification.ApproverName;
            BidId = bidApprovalNotification.BidId;
        }
        public static BidApprovalNotificationOutputDTO FromBidApprovalNotification(BidApprovalNotification bidApprovalNotification) => new BidApprovalNotificationOutputDTO(bidApprovalNotification);

    }
}
