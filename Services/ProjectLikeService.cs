using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class ProjectLikeService(MainAppContext mainAppContext, NotificationService likeNotificationService)
    {
        public async Task<ProjectLike> LikeProjectAsync(long likerId, long projectId, string likerName)
        {
            ProjectLike newProjectLike = new ProjectLike(likerId, projectId, likerName);
            await mainAppContext.AddAsync(newProjectLike);
            await mainAppContext.SaveChangesAsync();
            return newProjectLike;
        }
        public async Task UnlikeProjectAsync(ProjectLike projectLike)
        {
            mainAppContext.ProjectLikes.Remove(projectLike);
            await mainAppContext.SaveChangesAsync();
        }
        public async Task<bool> IsUserLikedProjectAsync(long likerId, long projectId)
        {
            return await mainAppContext.ProjectLikes.AnyAsync(pl => pl.ProjectId == projectId && pl.LikerId == likerId);
        }
        public async Task<ProjectLike?>Find(long likerId, long projectId)
        {
            return await mainAppContext.ProjectLikes.FirstOrDefaultAsync(pl => pl.ProjectId == projectId && pl.LikerId == likerId);
        }
    }
}
