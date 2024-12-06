namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class LikeNotificationOutputDTO
    {
        public long Id { get; set; }
        public long LikeId { get; set; }
        public long ReceiverId { get; set; }
        public string LikerName { get; set; }
        //TODO: Add profile image (nigga diyar shall do it)
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        LikeNotificationOutputDTO(LikeNotification likeNotification)
        {
            Id = likeNotification.Id;
            LikeId = likeNotification.LikeId;
            ReceiverId = likeNotification.ReceiverId;
            LikerName = likeNotification.LikerName;
            Message = likeNotification.Message;
            IsRead = likeNotification.IsRead;
            CreatedAt = likeNotification.CreatedAt;

        }
        public static LikeNotificationOutputDTO FromLikeNotification(LikeNotification likeNotification) => new LikeNotificationOutputDTO(likeNotification);
    }

}
