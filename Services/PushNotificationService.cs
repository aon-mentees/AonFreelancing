using AonFreelancing.Hubs;
using AonFreelancing.Interfaces;
using AonFreelancing.Models.DTOs.NoftificationDTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace AonFreelancing.Services
{
    public class PushNotificationService
    {
        readonly IHubContext<NotificationsHub, INotificationsClient> _INotificationsHubContext;
        readonly InMemoryUserConnectionService _inMemoryUserConnectionService;
        public PushNotificationService(IHubContext<NotificationsHub, INotificationsClient> notificationHubContext, InMemoryUserConnectionService inMemoryUserConnectionService)
        {
            _INotificationsHubContext = notificationHubContext;
            _inMemoryUserConnectionService = inMemoryUserConnectionService;
        }

        public async Task SendLikeNotification(LikeNotificationOutputDTO likeNotificationDTO)
        {
            var connections = _inMemoryUserConnectionService.GetConnections(likeNotificationDTO.ReceiverId);
            if (!connections.IsNullOrEmpty())
                await _INotificationsHubContext.Clients.Clients(connections).GetLikeNotification(likeNotificationDTO);
        }


    }
}
