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


    }
}
