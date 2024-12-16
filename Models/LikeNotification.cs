using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("LikeNotifications")]
    public class LikeNotification : Notification
    {
        public long ProjectId { get; set; }
        public long LikerId {  get; set; }
        public string LikerName { get; set; }

        public LikeNotification() { }

        public LikeNotification(string title, string message, long receiverId, string? image, long projectId, long likerId,string likerName)
        : base(title, message, receiverId, image)
        {
            ProjectId = projectId;
            LikerName = likerName;
            LikerId = likerId;
        }
    }
}
