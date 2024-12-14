using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class CommentService(MainAppContext mainAppContext)
    {
        public async Task<PaginatedResult<Comment>> GetProjectCommentsAsync(long projectId, int page, int pageSize, string imagesBaseUrl)
        {
            List<Comment> commentOut = await mainAppContext.Comments
                                        .Where(c => c.ProjectId == projectId)
                                        .OrderByDescending(c => c.CreatedAt)
                                        .Include(c => c.User)
                                        .Skip(page * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            int totalComments = await CountCommentsForProjectAsync(projectId);
            return new PaginatedResult<Comment>(totalComments, commentOut);
        }

        public async Task SaveCommentAsync(Comment comment)
        {
            await mainAppContext.Comments.AddAsync(comment);
            await mainAppContext.SaveChangesAsync();
        }

        public async Task<int> CountCommentsForProjectAsync(long projectId)
        {
            return await mainAppContext.Comments.CountAsync(c => c.ProjectId == projectId);
        }
    }
}
