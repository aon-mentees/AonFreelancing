using System.Security.Claims;
using AonFreelancing.Services;
using Microsoft.AspNetCore.SignalR;

namespace AonFreelancing.Hubs;

public abstract class BaseHub<T> : Hub<T> where T : class
{
    protected readonly InMemorySignalRUserConnectionService _userConnectionService;
    protected readonly AuthService _authService;

    public BaseHub(InMemorySignalRUserConnectionService userConnectionService, AuthService authService)
    {
        _userConnectionService = userConnectionService;
        _authService = authService;
    }

    public override async Task OnConnectedAsync()
    {
        //notice that this gets the claims identity from hub context not from http context
        var claimsIdentity = Context.User.Identity as ClaimsIdentity;
        long userId = _authService.GetUserId(claimsIdentity);
        _userConnectionService.Add(userId, Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        //notice that this gets the claims identity from hub context not from http context
        var claimsIdentity = Context.User.Identity as ClaimsIdentity;
        long userId = _authService.GetUserId(claimsIdentity);
        _userConnectionService.Remove(userId, Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}