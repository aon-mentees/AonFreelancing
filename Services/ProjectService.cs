using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AonFreelancing.Services
{
    public class ProjectService
    {
        private readonly MainAppContext _mainAppContext;
        private readonly OtpManager _otpManager;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JwtService _jwtService;
        public ProjectService(MainAppContext mainAppContext, OtpManager otpManager, UserManager<User> userManager, IConfiguration configuration, JwtService jwtService, RoleManager<ApplicationRole> roleManager)
        {
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _userManager = userManager;
            _configuration = configuration;
            _jwtService = jwtService;
            _roleManager = roleManager;
        }
        //public async Task<PaginatedResult<Project>> ()
        public async Task<Project?> FindProjectWithBidsAsync(long projectId)
        {
            return await _mainAppContext.Projects.Where(p => p.Id == projectId)
                                                 .Include(p => p.Bids)
                                                 .FirstOrDefaultAsync();
        }
        public async Task<Bid?> FindBidsAsync(Project project, long bidId)
        {
            return project.Bids.Where(b => b.Id == bidId && b.Status!=Constants.BIDS_STATUS_REJECTED).FirstOrDefault();
        }
        public async Task ApproveProjectBidAsync(Bid bid,Project project)
        {
            bid.Status = Constants.BIDS_STATUS_APPROVED;
            bid.ApprovedAt = DateTime.Now;
            project.Status = Constants.PROJECT_STATUS_CLOSED;
            project.FreelancerId = bid.FreelancerId;
            project.Bids.ForEach(b =>
            {
                if (b.Status != Constants.BIDS_STATUS_APPROVED)
                {
                    b.Status = Constants.BIDS_STATUS_REJECTED;
                    b.RejectedAt = bid.ApprovedAt;
                    _mainAppContext.Update(b);
                }
            });
            _mainAppContext.Projects.Update(project);
            _mainAppContext.Bids.Update(bid);
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task RejectProjectBidAsync(Bid bid)
        {
            bid.Status = Constants.BIDS_STATUS_REJECTED;
            bid.RejectedAt = DateTime.Now;
            _mainAppContext.Bids.Update(bid);
            await _mainAppContext.SaveChangesAsync();
        }

        public async Task<bool> IsProjectExistsAsync(long id)
        {
            return await _mainAppContext.Projects.AnyAsync(p => p.Id == id);
        }

        public async Task<PaginatedResult<Project>> FindClientFeedAsync(string queryString, List<string> qualificationNames, int pageNumber, int pageSize)
        {
            var query = _mainAppContext.Projects.AsNoTracking().Include(p => p.Client).AsQueryable();

            if (!string.IsNullOrEmpty(queryString))
                query = query.Where(p => p.Title.ToLower().Contains(queryString));
            if (!qualificationNames.IsNullOrEmpty())
                query = query.Where(p => qualificationNames.Contains(p.QualificationName));

            List<Project> storedProjects = await query.OrderByDescending(p => p.CreatedAt)
                                                        .Skip(pageNumber * pageSize)
                                                        .Take(pageSize)
                                                        .ToListAsync();
            //total count is - 1 instead of the actual value it intended; to keep it non disclosed.
            return new PaginatedResult<Project>(-1, storedProjects);
        }
        public async Task<PaginatedResult<Project>> FindFreelancerFeedAsync(string queryString, List<string> qualificationNames,
                                                                            PriceRange priceRange, int? duration,
                                                                            int pageNumber, int pageSize)
        {
            var query = _mainAppContext.Projects.AsNoTracking().Include(p => p.Client).AsQueryable();

            if (!string.IsNullOrEmpty(queryString))
                query = query.Where(p => p.Title.ToLower().Contains(queryString));
            if (!qualificationNames.IsNullOrEmpty())
                query = query.Where(p => qualificationNames.Contains(p.QualificationName));
            if (duration.HasValue)
                query = query.Where(p => p.Duration >= duration.Value);
            if (priceRange.MinPrice != null && priceRange.MaxPrice != null)
                query = query.Where(p => p.Budget >= priceRange.MinPrice && p.Budget <= priceRange.MaxPrice);

            List<Project>? storedProjects = await query.OrderByDescending(p => p.CreatedAt)
                                                       .Skip(pageNumber * pageSize)
                                                       .Take(pageSize)
                                                       .ToListAsync();
            //total count is - 1 instead of the actual value it intended; to keep it non disclosed.
            return new PaginatedResult<Project>(-1, storedProjects);
        }
    }
}
