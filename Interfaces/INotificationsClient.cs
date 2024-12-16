using AonFreelancing.Models.DTOs.NoftificationDTOs;

namespace AonFreelancing.Interfaces
{ 
    /// <summary>
    /// This interface provides strongly-typed methods that exist at the Client side but these don't exist on the server.
    /// </summary>
    public interface INotificationsClient
    {
        Task GetLikeNotification(LikeNotificationOutputDTO likeNotification);
        Task GetBidApprovalNotification(BidApprovalNotificationOutputDTO bidApprovalNotification);
        Task GetBidRejectionNotification(BidRejectionNotificationOutputDTO bidRejectionNotification);
        Task GetBidSubmissionNotification(BidSubmissionNotificationOutputDTO submitBidNotification);
        Task GetProfileVisitNotification(ProfileVisitNotificationOutputDTO profileVisitNotification);

        Task GetCommentNotification(CommentNotificationOutputDTO commentNotification);
    }
}
