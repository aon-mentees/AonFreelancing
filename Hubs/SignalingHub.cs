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
        await _signalingService.SendOfferAsync(recipientUserId, offerJson);
    }

    public async Task SendAnswer(long recipientUserId, string answerJson)
    {
        await _signalingService.SendAnswerAsync(recipientUserId, answerJson);
    }

    public async Task SendIceCandidate(long recipientUserId, string iceCandidateJson)
    {
        await _signalingService.SendIceCandidateAsync(recipientUserId, iceCandidateJson);
    }
}