using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    [JsonDerivedType(typeof(LikeNotificationOutputDTO))]
    [JsonDerivedType(typeof(BidRejectionNotificationOutputDTO))]
    [JsonDerivedType(typeof(BidApprovalNotificationOutputDTO))]
    [JsonDerivedType(typeof(BidSubmissionNotificationOutputDTO))]
    [JsonDerivedType(typeof(TaskApprovalNotificationOutputDTO))]
    [JsonDerivedType(typeof(TaskRejectionNotificationOutputDTO))]
    [JsonDerivedType(typeof(ProfileVisitNotificationOutputDTO))]
    public class NotificationOutputDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Image {  get; set; }

        public NotificationOutputDTO(long id, string title, string message, bool isRead, DateTime createdAt, string? image)
        {
            Id = id;
            Title = title;
            Message = message;
            IsRead = isRead;
            CreatedAt = createdAt;
            Image = image;
        }

    }
}
