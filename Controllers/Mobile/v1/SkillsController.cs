using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/skills")]
    [ApiController]
    public class SkillsController(MainAppContext mainAppContext, SkillsService skillsService, FreelancerService freelancerService,BlacklistService blacklistService) : BaseController
    {
        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost]
        public async Task<IActionResult> CreateSkill([FromBody] SkillInputDTO skillInputDTO)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            if (await blacklistService.IsTokenBlacklisted(token) == true)
                return Forbid();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedUserId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

            bool isSkillExistsForFreelancer = await mainAppContext.Skills.AsNoTracking().AnyAsync(s => s.FreelancerId == authenticatedUserId && s.Name == skillInputDTO.Name);

            if (isSkillExistsForFreelancer)
                return Conflict(CreateErrorResponse("409", "you already have this skill in your profile"));

            Skill? newSkill = Skill.FromInputDTO(skillInputDTO, authenticatedUserId);
            await mainAppContext.Skills.AddAsync(newSkill);
            await mainAppContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created);
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(long id)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            if (await blacklistService.IsTokenBlacklisted(token) == true)
                return Forbid();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedUserId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

            Skill? storedSkill = await skillsService.FindSkillByIdAsync(id);
            if (storedSkill == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Skill not found"));
            if (authenticatedUserId != storedSkill.FreelancerId)
                return Forbid();

            mainAppContext.Skills.Remove(storedSkill);
            await mainAppContext.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{freelancerId}/skills")]
        public async Task<IActionResult> GetSkillsByFreelancerIdAsync(long freelancerId, int page = 0, int pageSize = Constants.SKILLS_DEFAULT_PAGE_SIZE)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            if (await blacklistService.IsTokenBlacklisted(token) == true)
                return Forbid();

            if (await freelancerService.FindFreelancerByIdAsync(freelancerId) == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "freelancer not found"));
            PaginatedResult<Skill> paginatedSkills = await skillsService.FindSkillsByFreelancerIdAsync(freelancerId, page, pageSize);
            List<SkillOutputDTO> skillOutputDTOs = paginatedSkills.Result.Select(s => SkillOutputDTO.FromSkill(s)).ToList();
            PaginatedResult<SkillOutputDTO> paginatedSkillsOutputDTO = new PaginatedResult<SkillOutputDTO>(paginatedSkills.Total, skillOutputDTOs);

            return Ok(CreateSuccessResponse(paginatedSkillsOutputDTO));
        }
    }
}
