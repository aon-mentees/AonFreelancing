using AonFreelancing.Interfaces;
using AonFreelancing.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AonFreelancing.Hubs
{
    public class NotificationsHub : Hub<INotificationsClient>
    {
        readonly InMemorySignalRUserConnectionService _inMemorySignalRUserConnectionService;
        readonly AuthService _authService;
        public NotificationsHub(InMemorySignalRUserConnectionService userConnectionService, AuthService authService)
        {
            _inMemorySignalRUserConnectionService = userConnectionService;
            _authService = authService;
        }
        public override async Task OnConnectedAsync()
        {
            //notice that this gets the claims identity from hub context not from http context
            var claimsIdentity = Context.User.Identity as ClaimsIdentity;
            long userId = _authService.GetUserId(claimsIdentity);
            _inMemorySignalRUserConnectionService.Add(userId, Context.ConnectionId);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //notice that this gets the claims identity from hub context not from http context
            var claimsIdentity = Context.User.Identity as ClaimsIdentity;
            long userId = _authService.GetUserId(claimsIdentity);
            _inMemorySignalRUserConnectionService.Remove(userId, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}

