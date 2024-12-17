using AonFreelancing.Contexts;
using AonFreelancing.Models.Responses;
using AonFreelancing.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class ProfileService(MainAppContext mainAppContext)
    {
        public async Task<Client?> FindClientAsync(long clientId)
        {
            return await mainAppContext.Users.OfType<Client>().FirstOrDefaultAsync(c=>c.Id == clientId);
        }
        public async Task<PaginatedResult<Project>> FindClientActivitiesAsync(long clientId, int pageNumber, int pageSize)
        {
            var query = mainAppContext.Projects.AsNoTracking().Where(p => p.ClientId == clientId)       
                                                              .Include(p => p.ProjectLikes)
                                                              .Include(p=>p.Comments)
                                                              .AsQueryable();

            List<Project>? storedProjects = await query.OrderByDescending(p => p.CreatedAt)
                                                       .Skip(pageNumber * pageSize)
                                                       .Take(pageSize)
                                                       .ToListAsync();
          
            return new PaginatedResult<Project>(await query.CountAsync(), storedProjects);
        }
    }
}
