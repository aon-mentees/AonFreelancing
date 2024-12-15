using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Models
{
    public class Comment
    {
        public long Id { get; set; }                      
        public string Content { get; set; }       
        public DateTime CreatedAt { get; set; }
        public long ProjectId { get; set; }
        public Project Project { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public string? ImageUrl { get; set; }

        public Comment() { }
        public Comment(CommentInputDTO commentInputDTO, long projectId, long userId) 
        {
            Content = commentInputDTO.Content;
            ProjectId = projectId;
            UserId = userId;
            CreatedAt = DateTime.Now;
        }
    }
}
