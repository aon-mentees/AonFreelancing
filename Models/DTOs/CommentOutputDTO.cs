namespace AonFreelancing.Models.DTOs
{
    public class CommentOutputDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string CommenterName { get; set; }
        public long CommenterId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ImageUrl { get; set; }
    }
}
