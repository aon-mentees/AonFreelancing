using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Responses;
using AonFreelancing.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AonFreelancing.Services
{
    public class BidService(MainAppContext mainAppContext)
    {
        public async Task<PaginatedResult<Bid>> FindBidsByProjectIdWithFreelancerAsync(long projectId, int pageNumber, int pageSize)
        {
            List<Bid> storedBids = await mainAppContext.Bids.Include(b => b.Freelancer)
                                                            .Include(b => b.Project)
                                                            .Where(b => b.ProjectId == projectId&& b.Status!=Constants.BIDS_STATUS_REJECTED 
                                                            && !b.Project.IsDeleted && !b.Freelancer.IsDeleted )
                                                            .OrderByDescending(b => b.SubmittedAt)
                                                            .Skip(pageNumber * pageSize)
                                                            .Take(pageSize)
                                                            .ToListAsync();

            //storedBids = storedBids.Where(b => !b.Project.IsDeleted).ToList();
            //if (storedBids.IsNullOrEmpty())
            //    return new PaginatedResult<Bid>();

            return new PaginatedResult<Bid>(await CountByProjectId(projectId), storedBids);
        }
        public async Task<int> CountByProjectId(long projectId)
        {
            return await mainAppContext.Bids.CountAsync(b => b.ProjectId == projectId);
        }
    }
}
