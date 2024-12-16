namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class LikeNotificationOutputDTO : NotificationOutputDTO
    {

        public long ProjectId { get; set; }
        public long LikerId {  get; set; }
        public string LikerName { get; set; }

        LikeNotificationOutputDTO(LikeNotification likeNotification)
        : base(likeNotification.Id, likeNotification.Title, likeNotification.Message, likeNotification.IsRead, likeNotification.CreatedAt, likeNotification.Image)
        {
            ProjectId = likeNotification.ProjectId;
            LikerId = likeNotification.LikerId;
            LikerName = likeNotification.LikerName;
        }
        public static LikeNotificationOutputDTO FromLikeNotification(LikeNotification likeNotification) => new LikeNotificationOutputDTO(likeNotification);
    }

}
