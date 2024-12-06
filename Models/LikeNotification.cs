using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("LikeNotifications")]
    public class LikeNotification
    {
        [Key]
        public long Id { get; set; }
        public long LikeId {  get; set; }
        public long ReceiverId { get; set; }
        public string LikerName { get; set; }
        public string Message { get; set; }
        //TODO: Add profile image (nigga diyar shall do it)
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }


        public LikeNotification() { }
        public LikeNotification(long likeId,long receiverId, string likerName, string message, bool isRead)
        {
            LikeId = likeId;
            ReceiverId = receiverId;
            LikerName = likerName;
            Message = message;
            IsRead = isRead;
            CreatedAt = DateTime.Now;
        }

    }
}
