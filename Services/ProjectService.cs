using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Requests;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

            await SaveProjectChangesAsync();
        }
        public async Task RejectProjectBidAsync(Bid bid)
        {
            bid.Status = Constants.BIDS_STATUS_REJECTED;
            bid.RejectedAt = DateTime.Now;
            await SaveProjectChangesAsync();
        }
        public async Task SaveProjectChangesAsync()
        {
            await _mainAppContext.SaveChangesAsync();
        }

        // Check if User 1 Worked With User 2 
        public async Task<bool> IsUser1WorkedWithUser2Async(long userId1, long userId2)
        {
            return await _mainAppContext.Projects.AnyAsync(p => (p.ClientId == userId1 && p.FreelancerId == userId2) ||
                                                                (p.ClientId == userId2 && p.FreelancerId == userId1));
        }
    }
}
