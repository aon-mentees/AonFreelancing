using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class SkillsService(MainAppContext mainAppContext)
    {

        public async Task<PaginatedResult<Skill>>FindSkillsByFreelancerIdAsync(long freelancerId, int pageNumber,int pageSize)
        {
           List<Skill> storedSkills = await mainAppContext.Skills.Where(s => s.FreelancerId == freelancerId)
                                              .Skip(pageNumber * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();
            

            return new PaginatedResult<Skill>( await CountSkillsByFreelancerIdAsync(freelancerId), storedSkills);

        }
        public async Task<int> CountSkillsByFreelancerIdAsync(long freelancerId)
        {
            return await mainAppContext.Skills.CountAsync(s=>s.FreelancerId == freelancerId);
        }

    }
}
