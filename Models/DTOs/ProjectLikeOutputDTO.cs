namespace AonFreelancing.Models.DTOs
{
    public class ProjectLikeOutputDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long LikerId { get; set; }
        public string LikerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProfilePicture { get; set; }

        private ProjectLikeOutputDTO(ProjectLike projectLike, string imageBaseUrl)
        {
            Id = projectLike.Id;
            LikerId = projectLike.LikerId;
            ProjectId = projectLike.ProjectId;
            CreatedAt = projectLike.CreatedAt;
            LikerName = projectLike.LikerName;
            if (projectLike.LikerUser.ProfilePicture != null)
                ProfilePicture = $"{imageBaseUrl}/{projectLike.LikerUser.ProfilePicture}";
        }
        public static ProjectLikeOutputDTO FromProjectLike(ProjectLike projectLike, string imageBaseUrl) => new ProjectLikeOutputDTO(projectLike, imageBaseUrl);
    }
}
