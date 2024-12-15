using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("ProjectLikes")]
    public class ProjectLike
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long LikerId {  get; set; }
        public string LikerName { get; set; }
        public DateTime CreatedAt {  get; set; }
        public User LikerUser{ get; set; }
        public ProjectLike(long likerId, long projectId,string likerName)
        {
            LikerId = likerId;
            ProjectId = projectId;
            CreatedAt = DateTime.Now;
            LikerName = likerName;
        }

    }
}
