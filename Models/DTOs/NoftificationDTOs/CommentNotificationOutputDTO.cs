namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class CommentNotificationOutputDTO : NotificationOutputDTO
    {
        public long ProjectId { get; set; }
        public long CommenterId { get; set; }
        public string CommenterName { get; set; }

        public CommentNotificationOutputDTO(CommentNotification commentNotification)
            : base(commentNotification.Id, commentNotification.Title, commentNotification.Message, commentNotification.IsRead, commentNotification.CreatedAt, commentNotification.Image)
        {
            ProjectId = commentNotification.ProjectId;
            CommenterId = commentNotification.CommenterId;
            CommenterName = commentNotification.CommenterName;
        }
        public static CommentNotificationOutputDTO FromCommentNotification(CommentNotification commentNotification) => new CommentNotificationOutputDTO(commentNotification);
    }
}
