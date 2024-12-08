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
        //TODO: Add profile image (nigga diyar shall do it)


        public LikeNotification() { }

        public LikeNotification(string title, string message, long receiverId, long projectId, long likerId,string likerName)
        : base(title, message, receiverId)
        {
            ProjectId = projectId;
            LikerName = likerName;
            LikerId = likerId;
        }
    }
}
