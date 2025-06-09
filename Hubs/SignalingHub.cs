using AonFreelancing.Interfaces;
using AonFreelancing.Services;

namespace AonFreelancing.Hubs;

public class SignalingHub : BaseHub<ISignalingClient>
{
    public SignalingHub(InMemorySignalRUserConnectionService userConnectionService, AuthService authService) : base(
        userConnectionService, authService)
    {
    }
}