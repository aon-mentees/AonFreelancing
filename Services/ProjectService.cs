using AonFreelancing.Contexts;
using AonFreelancing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class ProjectService
    {

        private readonly MainAppContext _mainAppContext;

        public ProjectService(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        // Check if User 1 Worked With User 2 
        public async Task<bool> IsUser1WorkedWithUser2Async(long userId1,  long userId2)
        {
            return await _mainAppContext.Projects.AnyAsync(p => (p.ClientId == userId1 && p.FreelancerId == userId2) ||
                                                                (p.ClientId == userId2 && p.FreelancerId == userId1));
        }    
    }
}
