using System.Security.Claims;
using AonFreelancing.Interfaces;
using AonFreelancing.Models;
using AonFreelancing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace AonFreelancing.Hubs;

public class WebRtcSignalingHub : BaseHub<ISignalingClient>
{
    private readonly WebRtcSignalingService _webRtcSignalingService;

    public WebRtcSignalingHub(InMemorySignalRUserConnectionService userConnectionService, AuthService authService,
        WebRtcSignalingService webRtcSignalingService, UserService userService) : base(
        userConnectionService, authService, userService)
    {
        _webRtcSignalingService = webRtcSignalingService;
    }

    public async Task SendCallInvitation(long recipientUserId)
    {
        long callerUserId = _authService.GetUserId((Context.User.Identity as ClaimsIdentity));
        User? storedCallerUser = await _userService.FindByIdAsync(callerUserId);
        if (storedCallerUser == null)
            throw new HubException("UserNotFound: Caller does not exist (maybe deleted)");
        string imagesBaseUrl = $"{Context.GetHttpContext()?.Request.Scheme}://{Context.GetHttpContext()?.Request.Host}/images";
        string profilePictureUrl = $"{imagesBaseUrl}/{storedCallerUser.ProfilePicture}";

        await _webRtcSignalingService.SendCallInvitationAsync(callerUserId, recipientUserId, storedCallerUser.Name,
            profilePictureUrl);
    }

    public async Task SendCallAccepted(long recipientUserId)
    {
        await _webRtcSignalingService.SendCallAcceptedAsync(recipientUserId);
    }
    public async Task SendCallRejected(long recipientUserId)
    {
        await _webRtcSignalingService.SendCallRejectedAsync(recipientUserId);
    }

    public async Task SendOffer(long recipientUserId, string offerJson)
    {
        long senderUserId = _authService.GetUserId((Context.User.Identity as ClaimsIdentity));
        await _webRtcSignalingService.SendOfferAsync(senderUserId, recipientUserId, offerJson);
    }

    public async Task SendAnswer(long recipientUserId, string answerJson)
    {
        long senderUserId = _authService.GetUserId((Context.User.Identity as ClaimsIdentity));
        await _webRtcSignalingService.SendAnswerAsync(senderUserId, recipientUserId, answerJson);
    }

    public async Task SendIceCandidate(long recipientUserId, string iceCandidateJson)
    {
        long senderUserId = _authService.GetUserId((Context.User.Identity as ClaimsIdentity));
        await _webRtcSignalingService.SendIceCandidateAsync(senderUserId, recipientUserId, iceCandidateJson);
    }
}