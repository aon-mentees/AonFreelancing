namespace AonFreelancing.Models.DTOs
{
    public class ProjectLikeOutputDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long LikerId { get; set; }
        public string LikerName { get; set; }
        public DateTime CreatedAt { get; set; }

        private ProjectLikeOutputDTO(ProjectLike projectLike)
        {
            Id = projectLike.Id;
            LikerId = projectLike.LikerId;
            ProjectId = projectLike.ProjectId;
            CreatedAt = projectLike.CreatedAt;
            LikerName = projectLike.LikerName;
        }
        public static ProjectLikeOutputDTO FromProjectLike(ProjectLike projectLike) => new ProjectLikeOutputDTO(projectLike);
    }
}
