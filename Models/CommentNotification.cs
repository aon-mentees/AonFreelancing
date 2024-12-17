using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("CommentNotifications")]
    public class CommentNotification : Notification
    {
        public string CommenterName { get; set; }
        public long ProjectId { get; set; }
        public long CommenterId { get; set; }

        public CommentNotification() { }
        public CommentNotification(string title, string message, long receiverId, string? image, string commenterName, long projectId, long commenterId)
            : base(title, message, receiverId, image)
        {
            CommenterName = commenterName;
            ProjectId = projectId;
            CommenterId = commenterId;
        }
    }
}
