namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class BidSubmissionNotificationOutputDTO: NotificationOutputDTO
    {

        public long ProjectId { get; set; }
        public long FreelnacerId { get; set; }
        public string FreelancerName { get; set; }
     

        BidSubmissionNotificationOutputDTO(SubmitBidNotification submitBidNotification)
        : base(submitBidNotification.Id, submitBidNotification.Title, submitBidNotification.Message, submitBidNotification.IsRead, submitBidNotification.CreatedAt)
        {
            ProjectId = submitBidNotification.ProjectId;
            FreelnacerId = submitBidNotification.FreelancerId;
            FreelancerName = submitBidNotification.FreelancerName;
        }
        public static BidSubmissionNotificationOutputDTO FromSubmitBidNotification(SubmitBidNotification submitBidNotification) => new BidSubmissionNotificationOutputDTO(submitBidNotification);
    }
}
