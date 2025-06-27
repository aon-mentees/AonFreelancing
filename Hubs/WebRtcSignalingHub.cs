using System.Security.Claims;
using AonFreelancing.Interfaces;
using AonFreelancing.Services;
using Microsoft.IdentityModel.Tokens;

namespace AonFreelancing.Hubs;

public class WebRtcSignalingHub : BaseHub<ISignalingClient>
{
    private readonly WebRtcSignalingService _webRtcSignalingService;

    public WebRtcSignalingHub(InMemorySignalRUserConnectionService userConnectionService, AuthService authService,
        WebRtcSignalingService webRtcSignalingService) : base(
        userConnectionService, authService)
    {
        _webRtcSignalingService = webRtcSignalingService;
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