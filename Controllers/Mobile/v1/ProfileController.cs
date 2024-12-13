using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/users")]
    [ApiController]
    public class ProfileController(MainAppContext mainAppContext, AuthService authService)
        : BaseController
    {
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute] long id)
        {
            FreelancerResponseDTO? storedFreelancerDTO = await mainAppContext.Users
                .OfType<Freelancer>()
                .Include(f => f.Skills)
                .Where(f => f.Id == id)
                .Select(f => FreelancerResponseDTO.FromFreelancer(f))
                .FirstOrDefaultAsync();

            if (storedFreelancerDTO != null)
                return Ok(CreateSuccessResponse(storedFreelancerDTO));

            ClientResponseDTO? storedClientDTO = await mainAppContext.Users
                .OfType<Client>()
                .Where(c => c.Id == id)
                .Include(c => c.Projects)
                .Select(c => ClientResponseDTO.FromClient(c))
                .FirstOrDefaultAsync();

            if (storedClientDTO != null)
                return Ok(CreateSuccessResponse(storedClientDTO));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "NotFound"));
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserStatisticsAsync()
        {
            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            var storedProjects = await mainAppContext.Projects
                .AsNoTracking()
                .Include(p => p.Freelancer)
                .Include(p => p.Tasks)
                .Where(p => p.ClientId == authenticatedUserId || p.FreelancerId == authenticatedUserId)
                .ToListAsync();

            var storedTasks = storedProjects.SelectMany(p => p.Tasks).ToList();

            return Ok(CreateSuccessResponse(new UserStatisticsDTO(ProjectsStatisticsDTO.FromProjects(storedProjects),
                                                                  TasksStatisticsDTO.FromTasks(storedTasks))));
        }
    }

}
