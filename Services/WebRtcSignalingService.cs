using AonFreelancing.Hubs;
using AonFreelancing.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AonFreelancing.Services;

public class WebRtcSignalingService
{
    private readonly IHubContext<WebRtcSignalingHub, ISignalingClient> _signalingHubContext;
    private readonly InMemorySignalRUserConnectionService _connectionService;

    public WebRtcSignalingService(
        IHubContext<WebRtcSignalingHub, ISignalingClient> signalingHubContext,
        InMemorySignalRUserConnectionService connectionService)
    {
        _signalingHubContext = signalingHubContext;
        _connectionService = connectionService;
    }

    public async Task SendCallInvitationAsync(long callerUserId, long recipientUserId, string callerName,
        string callerProfilePictureUrl)
    {
        var connections = _connectionService.GetConnections(recipientUserId);
        if (connections != null && connections.Any())
            await _signalingHubContext.Clients.Clients(connections)
                .ReceiveCallInvitation(callerUserId, callerName, callerProfilePictureUrl);
    }

    public async Task SendCallAcceptedAsync(long recipientUserId)
    {
        var connections = _connectionService.GetConnections(recipientUserId);
        await _signalingHubContext.Clients.Clients(connections)
            .CallAccepted();
    }
    public async Task SendCallRejectedAsync(long recipientUserId)
    {
        var connections = _connectionService.GetConnections(recipientUserId);
        await _signalingHubContext.Clients.Clients(connections)
            .CallRejected();
    }
    public async Task SendOfferAsync(long senderUserId, long recipientUserId, string offerJson)
    {
        var connections = _connectionService.GetConnections(recipientUserId);
        if (connections != null && connections.Any())
            await _signalingHubContext.Clients.Clients(connections)
                .GetOffer(senderUserId, offerJson);
    }

    public async Task SendAnswerAsync(long senderUserId, long recipientUserId, string answerJson)
    {
        var connections = _connectionService.GetConnections(recipientUserId);
        if (connections != null && connections.Any())
            await _signalingHubContext.Clients.Clients(connections)
                .GetAnswer(senderUserId, answerJson);
    }

    public async Task SendIceCandidateAsync(long senderUserId, long recipientUserId, string iceCandidateJson)
    {
        var connections = _connectionService.GetConnections(recipientUserId);
        if (connections != null && connections.Any())
            await _signalingHubContext.Clients.Clients(connections)
                .GetIceCandidate(senderUserId, iceCandidateJson);
    }
}