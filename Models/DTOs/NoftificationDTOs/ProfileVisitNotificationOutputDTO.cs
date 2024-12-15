namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class ProfileVisitNotificationOutputDTO : NotificationOutputDTO
    {
        public long VisitorId { get; set; }
        public string VisitorName { get; set; }

        public ProfileVisitNotificationOutputDTO(ProfileVisitNotification profileVisitNotification)
            : base(profileVisitNotification.Id, profileVisitNotification.Title, profileVisitNotification.Message, profileVisitNotification.IsRead, profileVisitNotification.CreatedAt, profileVisitNotification.Image)
        {
            VisitorId = profileVisitNotification.VisitorId;
            VisitorName = profileVisitNotification.VisitorName;
        }

        public static ProfileVisitNotificationOutputDTO FromVisitNotification(ProfileVisitNotification profileVisitNotification)
            => new ProfileVisitNotificationOutputDTO(profileVisitNotification);
    }
}
