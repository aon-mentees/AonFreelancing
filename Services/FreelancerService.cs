using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class FreelancerService(MainAppContext mainAppContext)
        : MainDbService(mainAppContext)
    {
        public async Task<Freelancer?> FindFreelancerWithCertifications(long freelancerId)
        {
            return await _mainAppContext.Users.OfType<Freelancer>()
                .Include(f => f.Certifications)
                .FirstOrDefaultAsync(f => f.Id == freelancerId);
        }

        public async Task<Certification?> FindFreelancerCertification(long certificationId)
        {
            return await _mainAppContext.Certifications
                .FirstOrDefaultAsync(c => c.Id == certificationId);
        } 

        public async Task<Freelancer?>FindFreelancerById(long id)
        {

            return await _mainAppContext.Users.OfType<Freelancer>().FirstOrDefaultAsync(f => f.Id == id);
        }
    }
}
