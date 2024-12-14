using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class CommentService(MainAppContext mainAppContext)
    {
        public async Task<List<CommentOutDTO?>> GetProjectCommentsAsync(long projectId, int page, int pageSize, string imagesBaseUrl)
        {
            return await mainAppContext.Comments
                                        .Where(c => c.ProjectId == projectId)
                                        .OrderByDescending(c => c.CreatedAt)
                                        .Include(c => c.User)
                                        .Skip(page * pageSize)
                                        .Take(pageSize)
                                        .Select(c => (CommentOutDTO?)new CommentOutDTO(c, c.User.Name, imagesBaseUrl))
                                        .ToListAsync();
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
