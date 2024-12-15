using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    public class Notification
    {
        [Key]
        public long Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public long ReceiverId { get; set; }
        public string? Image { get; set; }
        public Notification() { }
        public Notification(string title, string message, long receiverId, string? image)
        {
            Title = title;
            Message = message;
            CreatedAt = DateTime.Now;
            ReceiverId = receiverId;
            Image = image;

        }
    }
}
