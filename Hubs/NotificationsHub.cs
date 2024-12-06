using AonFreelancing.Interfaces;
using AonFreelancing.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AonFreelancing.Hubs
{
    public class NotificationsHub : Hub<INotificationsClient>
    {
        readonly InMemoryUserConnectionService _inMemorySignalRUserConnectionsService;
        readonly AuthService _authService;
        public NotificationsHub(InMemoryUserConnectionService userConnectionService, AuthService authService)
        {
            _inMemorySignalRUserConnectionsService = userConnectionService;
            _authService = authService;
        }
        public override async Task OnConnectedAsync()
        {
            //notice that this gets the claims identity from hub context not from http context
            var claimsIdentity = Context.User.Identity as ClaimsIdentity;
            long userId = _authService.GetUserId(claimsIdentity);
            _inMemorySignalRUserConnectionsService.Add(userId, Context.ConnectionId);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //notice that this gets the claims identity from hub context not from http context
            var claimsIdentity = Context.User.Identity as ClaimsIdentity;
            long userId = _authService.GetUserId(claimsIdentity);
            _inMemorySignalRUserConnectionsService.Remove(userId, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}

