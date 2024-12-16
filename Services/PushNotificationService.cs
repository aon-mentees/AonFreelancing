using AonFreelancing.Hubs;
using AonFreelancing.Interfaces;
using AonFreelancing.Models.DTOs.NoftificationDTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace AonFreelancing.Services
{
    public class PushNotificationService
    {
        readonly IHubContext<NotificationsHub, INotificationsClient> _iNotificationsHubContext;
        readonly InMemorySignalRUserConnectionService _inMemorySignalRUserConnectionService;
        public PushNotificationService(IHubContext<NotificationsHub, INotificationsClient> notificationHubContext, InMemorySignalRUserConnectionService inMemorySignalRUserConnectionService)
        {
            _iNotificationsHubContext = notificationHubContext;
            _inMemorySignalRUserConnectionService = inMemorySignalRUserConnectionService;
        }

        public async Task SendLikeNotification(LikeNotificationOutputDTO likeNotificationDTO,long receiverId)
        {
            var connections = _inMemorySignalRUserConnectionService.GetConnections(receiverId);
            if (!connections.IsNullOrEmpty())
                await _iNotificationsHubContext.Clients.Clients(connections).GetLikeNotification(likeNotificationDTO);
        }

        public async Task SendApprovalNotification(BidApprovalNotificationOutputDTO bidApprovalNotification, long receiverId)
        {
            var connections = _inMemorySignalRUserConnectionService.GetConnections(receiverId);
            if (!connections.IsNullOrEmpty())
                await _iNotificationsHubContext.Clients.Clients(connections).GetBidApprovalNotification(bidApprovalNotification);
        }
        public async Task SendRejectionNotification(BidRejectionNotificationOutputDTO bidRejectionNotification, long receiverId)
        {
            var connections = _inMemorySignalRUserConnectionService.GetConnections(receiverId);
            if (!connections.IsNullOrEmpty())
                await _iNotificationsHubContext.Clients.Clients(connections).GetBidRejectionNotification(bidRejectionNotification);
        }
        public async Task SendSubmitBidNotification(BidSubmissionNotificationOutputDTO submitBidNotificationOutDTO, long receiverId)
        {
            var connections = _inMemorySignalRUserConnectionService.GetConnections(receiverId);
            if (!connections.IsNullOrEmpty())
                await _iNotificationsHubContext.Clients.Clients(connections).GetBidSubmissionNotification(submitBidNotificationOutDTO);
        }

        public async Task SendProfileVisitNotification(ProfileVisitNotificationOutputDTO profileVisitNotificationDTO, long receiverId)
        {
            var connections = _inMemorySignalRUserConnectionService.GetConnections(receiverId);
            if (!connections.IsNullOrEmpty())
                await _iNotificationsHubContext.Clients.Clients(connections).GetProfileVisitNotification(profileVisitNotificationDTO);
        }

        public async Task SendCommentNotification(CommentNotificationOutputDTO commentNotificationOutputDTO, long receiverId)
        {
            var connections = _inMemorySignalRUserConnectionService.GetConnections(receiverId);
            if (!connections.IsNullOrEmpty())
                await _iNotificationsHubContext.Clients.Clients(connections).GetCommentNotification(commentNotificationOutputDTO);
        }

    }
}
