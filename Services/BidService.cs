using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class BidService(MainAppContext mainAppContext)
    {
        public async Task<PaginatedResult<Bid>> FindByProjectIdWithFreelancer(long projectId, int pageNumber, int pageSize)
        {
            List<Bid> storedBids = await mainAppContext.Bids.Include(b => b.Freelancer)
                                                            .Where(b => b.ProjectId == projectId)
                                                            .OrderByDescending(b => b.SubmittedAt)
                                                            .Skip(pageNumber * pageSize)
                                                            .Take(pageSize)
                                                            .ToListAsync();

            return new PaginatedResult<Bid>(await CountByProjectId(projectId), storedBids);
        }
        public async Task<int> CountByProjectId(long projectId)
        {
            return await mainAppContext.Bids.CountAsync(b => b.ProjectId == projectId);
        }
    }
}
