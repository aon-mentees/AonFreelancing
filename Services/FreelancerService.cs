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

        public async Task<Freelancer?> FindFreelancerWithEducation(long freelancerId)
        {
            return await _mainAppContext.Users.OfType<Freelancer>()
                .Include(f => f.Education)
                .FirstOrDefaultAsync(f => f.Id == freelancerId);
        }
        public async Task<Education?> FindFreelancerEducation(long educationId)
        {
            return await _mainAppContext.Educations
                .FirstOrDefaultAsync(c => c.Id == educationId);
        }
        public async Task<bool> FindExistingFreelancerEducationAsync(long id, string institution, string degree)
        {
            return await _mainAppContext.Educations.AsNoTracking()
                .AnyAsync(e => e.Id == id && e.Institution == institution && e.Degree == degree);

        }
        public async Task<bool> FindExistingFreelancerCertificationAsync(long id, string name, string issure,DateTime? expiryDate)
        {
            return await _mainAppContext.Certifications.AsNoTracking()
                .AnyAsync(e => e.Id == id && e.Name == name && e.Issuer == issure && expiryDate > DateTime.Now);

        }
        public async Task<Freelancer>? FindFreelancerByIdAsync(long id)
        {
            return await _mainAppContext.Users.OfType<Freelancer>()
                .FirstOrDefaultAsync(f=>f.Id==id);

        }

    }
}
