namespace AonFreelancing.Models.DTOs
{
    public class CommentOutputDTO
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public string CommenterName { get; set; }
        public long CommenterId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ImageUrl { get; set; }

        public CommentOutputDTO(Comment comment, string commenterName, string imagesBaseUrl)
        {
            Id = comment.Id;
            Content = comment.Content;
            CommenterName = commenterName;
            CommenterId = comment.UserId;
            CreatedAt = comment.CreatedAt;
            ImageUrl = comment.ImageUrl != null ? $"{imagesBaseUrl}/{comment.ImageUrl}" : null;
        }
    }
}
