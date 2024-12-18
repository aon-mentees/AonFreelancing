using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
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
        public async Task<PaginatedResult<Project>> FindProjectsByClientId(long clientId, int pageNumber, int pageSize)
        {
            List<Project> storedProjects = await _mainAppContext.Projects.Where(p => p.ClientId == clientId && !p.IsDeleted)
                                                                        .OrderByDescending(p => p.CreatedAt)
                                                                        .Skip(pageNumber * pageSize)
                                                                        .Take(pageSize)
                                                                        .ToListAsync();
            return new PaginatedResult<Project>(await CountProjectsByClientIdAsync(clientId), storedProjects);
        }
        public async Task<List<Project>> FindProjectWithFreelancerAndTasks(long authenticatedUserId)
        {
          return  await _mainAppContext.Projects
                .AsNoTracking()
                .Include(p => p.Freelancer)
                .Include(p => p.Tasks)
                .Where(p => p.ClientId == authenticatedUserId || p.FreelancerId == authenticatedUserId)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }
        public async Task<Project?> FindProjectTasks(long authenticatedUserId)
        {
            return await _mainAppContext.Projects.Include(p => p.Tasks)
                                                        .Where( p => p.Id == authenticatedUserId)
                                                        .Where( p => !p.IsDeleted)
                                                        .FirstOrDefaultAsync();
        }
        public async Task<int> CountProjectsByClientIdAsync(long clientId)
        {
            return await _mainAppContext.Projects.CountAsync(p => p.ClientId == clientId && !p.IsDeleted);
        }
        public async Task<Project?> FindProjectWithBidsAsync(long projectId)
        {
            return await _mainAppContext.Projects.Where(p => p.Id == projectId && !p.IsDeleted)
                                                 .Include(p => p.Bids)
                                                 .FirstOrDefaultAsync();
        }
        public async Task<Bid?> FindBidsAsync(Project project, long bidId)
        {
            return project.Bids.Where(b => b.Id == bidId && b.Status!=Constants.BIDS_STATUS_REJECTED).FirstOrDefault();
        }
        public async Task ApplyBidAsync(Bid? bid)
        {
            await _mainAppContext.AddAsync(bid);
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task ApproveProjectBidAsync(Bid bid,Project project)
        {
            bid.Status = Constants.BIDS_STATUS_APPROVED;
            bid.ApprovedAt = DateTime.Now;
            project.Status = Constants.PROJECT_STATUS_IN_PROGRESS;
            project.StartDate = DateTime.Now;
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
            return await _mainAppContext.Projects.AnyAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<PaginatedResult<Project>> FindClientFeedAsync(string queryString, List<string> qualificationNames, int pageNumber, int pageSize)
        {
            var query = _mainAppContext.Projects.AsNoTracking().Include(p => p.Client).AsQueryable();

            if (!string.IsNullOrEmpty(queryString))
                query = query.Where(p => p.Title.ToLower().Contains(queryString));
            if (!qualificationNames.IsNullOrEmpty())
                query = query.Where(p => qualificationNames.Contains(p.QualificationName));

            List<Project> storedProjects = await query.Where(p => !p.IsDeleted)
                                                        .OrderByDescending(p => p.CreatedAt)
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

            List<Project>? storedProjects = await query.Where(p => !p.IsDeleted)
                                                       .OrderByDescending(p => p.CreatedAt)
                                                       .Skip(pageNumber * pageSize)
                                                       .Take(pageSize)
                                                       .ToListAsync();
            //total count is - 1 instead of the actual value it intended; to keep it non disclosed.
            return new PaginatedResult<Project>(-1, storedProjects);
        }
      
        // Check if User 1 Worked With User 2 
        public async Task<bool> IsUser1WorkedWithUser2Async(long userId1, long userId2)
        {
            return await _mainAppContext.Projects.AnyAsync(p => (p.ClientId == userId1 && p.FreelancerId == userId2) ||
                                                                (p.ClientId == userId2 && p.FreelancerId == userId1));
        }

        public async Task<Project?> FindProjectAsync(long projectId)
        {
            return await _mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
        }

        public async Task<PaginatedResult<Project>> FindProjectsByClientIdWithTasksAndClient(long clientId, int pageNumber, int pageSize, string status)
        {
            List<Project> storedProjects = await _mainAppContext.Projects.AsNoTracking()
                                                                           .Include(p => p.Tasks)
                                                                           .Include(p => p.Client)
                                                                           .Where(p => p.ClientId == clientId)
                                                                           .Where(p => p.Status.Contains(status))
                                                                           .Where(p => !p.IsDeleted)
                                                                           .Skip(pageNumber * pageSize)
                                                                           .Take(pageSize)
                                                                           .ToListAsync();
            int totalCount = await _mainAppContext.Projects.CountAsync(p => p.ClientId == clientId && !p.IsDeleted);
            return new PaginatedResult<Project>(totalCount, storedProjects);
        }
        public async Task<PaginatedResult<Project>> FindProjectsByFreelancerIdWithTasksAndClient(long freelancerId, int pageNumber, int pageSize, string status)
        {
            List<Project> storedProjects = await _mainAppContext.Projects.AsNoTracking()
                                                                           .Include(p => p.Tasks)
                                                                           .Include(p=>p.Client)
                                                                           .Where(p => p.FreelancerId == freelancerId)
                                                                           .Where(p => p.Status.Contains(status))
                                                                           .Where(p => !p.IsDeleted)
                                                                           .Skip(pageNumber * pageSize)
                                                                           .Take(pageSize)
                                                                           .ToListAsync();
            int totalCount = await _mainAppContext.Projects.CountAsync(p => p.FreelancerId == freelancerId && !p.IsDeleted);
            return new PaginatedResult<Project>(totalCount, storedProjects);
        }

        public async Task CompleteProjectAsync(Project project)
        {
            project.Status = Constants.PROJECT_STATUS_COMPLETED;
            project.EndDate = DateTime.Now;

            _mainAppContext.Projects.Update(project);
            await _mainAppContext.SaveChangesAsync();
        }
    }
}
