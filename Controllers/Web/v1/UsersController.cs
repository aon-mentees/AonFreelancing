using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Web.v1
{
    [Authorize]
    [Route("api/web/v1/users")]
    [ApiController]
    public class UsersController(MainAppContext mainAppContext, RoleManager<ApplicationRole> roleManager)
        : BaseController
    {
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute]long id)
        {
FreelancerResponseDTO? storedFreelancerDTO = await mainAppContext.Users.OfType<Freelancer>()
                                                                                    .Include(f=>f.Skills)
                                                                                    .Where(f => f.Id == id)
                                                                                    .Select(f => FreelancerResponseDTO.FromFreelancer(f))
                                                                                    .FirstOrDefaultAsync();

            if (storedFreelancerDTO != null)
                return Ok(new ApiResponse<FreelancerResponseDTO>
                {
                    IsSuccess = true,
                    Results = storedFreelancerDTO,
                    Errors = null
                });


            ClientResponseDTO? storedClientDTO = await mainAppContext.Users
                .OfType<Client>()
                .Where(c => c.Id == id)
                .Include(c => c.Projects)
                .Select(c => new ClientResponseDTO
                 {
                     Id = c.Id,
                     Name = c.Name,
                     Username = c.UserName ?? string.Empty, 
                     PhoneNumber = c.PhoneNumber ?? string.Empty,
                     UserType = Constants.USER_TYPE_CLIENT,
                     IsPhoneNumberVerified = c.PhoneNumberConfirmed,
                     Role = new RoleResponseDTO { Name = Constants.USER_TYPE_CLIENT },
                     Projects = c.Projects.Select(p =>  new ProjectDetailsDTO
                     {
                         Id = p.Id,
                         Description = p.Description,
                         EndDate = p.EndDate,
                         StartDate = p.StartDate,
                         Name = p.Title,
                     }),
                     CompanyName = c.CompanyName,

                 }).FirstOrDefaultAsync();


            if (storedClientDTO != null)
                return Ok(CreateSuccessResponse(storedClientDTO));

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "NotFound"));

        }
        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedUserId = Convert.ToInt64(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
            //User? authenticatedUser = await userManager.GetUserAsync(HttpContext.User);

            var storedProjects = await mainAppContext.Projects.AsNoTracking()
                                                                   .Include(p => p.Freelancer)
                                                                   .Include(p => p.Tasks)
                                                                   .Where(p => p.ClientId == authenticatedUserId || p.FreelancerId == authenticatedUserId)
                                                                   .ToListAsync();
            var storedTasks = storedProjects.SelectMany(p => p.Tasks)
                                            .ToList();
            return Ok(CreateSuccessResponse(new UserStatisticsDTO(ProjectsStatisticsDTO.FromProjects(storedProjects),
                                                                  TasksStatisticsDTO.FromTasks(storedTasks)
                                                                  )
            ));

        }
    }
   
}
