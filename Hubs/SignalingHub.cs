using System.Security.Claims;
using AonFreelancing.Interfaces;
using AonFreelancing.Services;
using Microsoft.IdentityModel.Tokens;

namespace AonFreelancing.Hubs;

public class SignalingHub : BaseHub<ISignalingClient>
{
    private readonly SignalingService _signalingService;

    public SignalingHub(InMemorySignalRUserConnectionService userConnectionService, AuthService authService,
        SignalingService signalingService) : base(
        userConnectionService, authService)
    {
        _signalingService = signalingService;
    }

    public async Task SendOffer(long recipientUserId, string offerJson)
    {
        long senderUserId = _authService.GetUserId((Context.User.Identity as ClaimsIdentity));
        await _signalingService.SendOfferAsync(senderUserId, recipientUserId, offerJson);
    }

    public async Task SendAnswer(long recipientUserId, string answerJson)
    {
        long senderUserId = _authService.GetUserId((Context.User.Identity as ClaimsIdentity));
        await _signalingService.SendAnswerAsync(senderUserId, recipientUserId, answerJson);
    }

    public async Task SendIceCandidate(long recipientUserId, string iceCandidateJson)
    {
        long senderUserId = _authService.GetUserId((Context.User.Identity as ClaimsIdentity));
        await _signalingService.SendIceCandidateAsync(senderUserId, recipientUserId, iceCandidateJson);
    }
}