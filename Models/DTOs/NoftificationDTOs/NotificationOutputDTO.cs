﻿namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class NotificationOutputDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public long ReceiverId { get; set; }

        public NotificationOutputDTO(long id, string title, string message, bool isRead, DateTime createdAt, long receiverId)
        {
            Id = id;
            Title = title;
            Message = message;
            IsRead = isRead;
            CreatedAt = createdAt;
            ReceiverId = receiverId;
        }

    }
}