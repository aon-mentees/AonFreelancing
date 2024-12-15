using AonFreelancing.Models.DTOs.NoftificationDTOs;
using AonFreelancing.Models;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using static System.Net.Mime.MediaTypeNames;

public class ProfileService
{
    private readonly PushNotificationService _pushNotificationService;
    private readonly UserService _userService;

    public ProfileService(PushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    public async Task VisitProfileAsync(long visitorId, long receiverId)
    {
        var visitorName = await _userService.FindByIdAsync(visitorId);

        var profileVisitNotification = new ProfileVisitNotification(
            Constants.PROFILE_VISIT_NOTIFICATION_TITLE,
            string.Format(Constants.PROFILE_VISIT_NOTIFICATION_MESSAGE_FORMAT, visitorName),
            receiverId,
            string.Empty, // low level code --> for lilo 
            visitorId,
            visitorName.Name

        );

        var notificationDTO = new ProfileVisitNotificationOutputDTO(profileVisitNotification);
        await _pushNotificationService.SendProfileVisitNotification(notificationDTO, receiverId);
    }
}
