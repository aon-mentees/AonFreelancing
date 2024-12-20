using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class FreelancerService : MainDbService
    {
        public FreelancerService(MainAppContext mainAppContext) : base(mainAppContext) { }

        public async Task<Freelancer?> FindFreelancerByIdAsync(long freelancerId)
        {
            return await _mainAppContext.Users.OfType<Freelancer>()
                                                                   .AsNoTracking()
                                                                   .Include(f => f.Projects)
                                                                   .Where(f => f.Id == freelancerId && !f.IsDeleted)
                                                                   .FirstOrDefaultAsync();
        }
        public async Task<bool> IsFreelancerExistsAsync(long freelancerId)
        {
            return await _mainAppContext.Users.OfType<Freelancer>()
                .AnyAsync(f => f.Id == freelancerId && !f.IsDeleted);
        }
        //Certifications
        public async Task<PaginatedResult<Certification>> FindCertificationByFreelancerIdAsync(long freelancerId, int pageNumber, int pageSize)
        {
            List<Certification> storedCertifications = await _mainAppContext.Certifications
               .Where(c => c.FreelancerId == freelancerId)
               .Skip(pageNumber * pageSize)
               .Take(pageSize)
               .ToListAsync();

            return new PaginatedResult<Certification>(await CountCertificationsByFreelancerIdAsync(freelancerId), storedCertifications);
        }

        public async Task<Certification?> FindFreelancerCertificationAsync(long certificationId)
        {
            return await _mainAppContext.Certifications
                .FirstOrDefaultAsync(c => c.Id == certificationId);
        }

        public async Task<int> CountCertificationsByFreelancerIdAsync(long freelancerId)
        {
            return await _mainAppContext.Certifications.CountAsync(c => c.FreelancerId == freelancerId);
        }

        public async Task<bool> FindExistingFreelancerCertificationAsync(long freelancerId, string name, string issuer, DateTime? expiryDate)
        {
            return await _mainAppContext.Certifications.AsNoTracking()
                .AnyAsync(c => c.FreelancerId == freelancerId && c.Name == name && c.Issuer == issuer && expiryDate > DateTime.Now);
        }

        //Educations 

        public async Task<PaginatedResult<Education>> FindEducationByFreelancerIdAsync(long freelancerId, int pageNumber, int pageSize)
        {
            List<Education> storedEducations = await _mainAppContext.Educations
               .Where(e => e.freelancerId == freelancerId)
               .Skip(pageNumber * pageSize)
               .Take(pageSize)
               .ToListAsync();

            return new PaginatedResult<Education>(await CountEducationsByFreelancerIdAsync(freelancerId), storedEducations);
        }

        public async Task<int> CountEducationsByFreelancerIdAsync(long freelancerId)
        {
            return await _mainAppContext.Educations.CountAsync(e => e.freelancerId == freelancerId);
        }

        public async Task<Education?> FindFreelancerEducationAsync(long educationId)
        {
            return await _mainAppContext.Educations
                .FirstOrDefaultAsync(e => e.Id == educationId);
        }

        public async Task<bool> FindExistingFreelancerEducationAsync(long freelancerId, string institution, string degree)
        {
            return await _mainAppContext.Educations.AsNoTracking()
                .AnyAsync(e => e.freelancerId == freelancerId && e.Institution == institution && e.Degree == degree);
        }

        //WorkExperiences
        public async Task<PaginatedResult<WorkExperience>> FindWorkExperienceByFreelancerIdAsync(long freelancerId, int pageNumber, int pageSize)
        {
            List<WorkExperience> storedWorkExperiences = await _mainAppContext.WorkExperiences
                .Where(w => w.FreelancerId == freelancerId)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<WorkExperience>(await CountWorkExperiencesByFreelancerIdAsync(freelancerId), storedWorkExperiences);
        }

        public async Task<int> CountWorkExperiencesByFreelancerIdAsync(long freelancerId)
        {
            return await _mainAppContext.WorkExperiences.CountAsync(w => w.FreelancerId == freelancerId);
        }

        public async Task<WorkExperience?> FindFreelancerWorkExperienceAsync(long workExperienceId)
        {
            return await _mainAppContext.WorkExperiences
                .FirstOrDefaultAsync(w => w.Id == workExperienceId);
        }

        public async Task<bool> FindExistingFreelancerWorkExperienceAsync(long freelancerId, string jobTitle, string employmentType, string employerName)
        {
            return await _mainAppContext.WorkExperiences.AsNoTracking()
                .AnyAsync(w => w.FreelancerId == freelancerId && w.JobTitle == jobTitle && w.EmploymentType == employmentType && w.EmployerName == employerName);
        }
    }
}
