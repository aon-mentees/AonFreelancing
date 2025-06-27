using AonFreelancing.Interfaces;
using AonFreelancing.Services;

namespace AonFreelancing.Hubs
{
    public class NotificationsHub : BaseHub<INotificationsClient>
    {
        public NotificationsHub(InMemorySignalRUserConnectionService userConnectionService, AuthService authService, UserService userService) :
            base(userConnectionService, authService, userService)
        {
        }
    }
}