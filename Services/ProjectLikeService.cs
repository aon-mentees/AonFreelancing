using AonFreelancing.Contexts;
using AonFreelancing.Models;

namespace AonFreelancing.Services
{
    public class ProjectLikeService(MainAppContext mainAppContext,LikeNotificationService likeNotificationService)
    {
        public async Task<ProjectLike> LikeProjectAsync(long userId, long projectId, string name)
        {
            ProjectLike newProjectLike = new ProjectLike(userId, projectId, name);
            await mainAppContext.AddAsync(newProjectLike);
            await mainAppContext.SaveChangesAsync();
            return newProjectLike;
        }
        public async Task UnlikeProjectAsync(ProjectLike projectLike)
        {
            mainAppContext.ProjectLikes.Remove(projectLike);
            await likeNotificationService.DeleteWithrojectLikeAsync(projectLike);
        }

    }
}
