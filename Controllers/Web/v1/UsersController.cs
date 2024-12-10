using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
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
    public class UsersController(MainAppContext mainAppContext, AuthService authService, RoleManager<ApplicationRole> roleManager)
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

        // I need your opinion on updating about for user :)
        // Add it here? Or add it to every controller?
        [HttpPatch("about")]
        public async Task<IActionResult> UpdateAboutAsync([FromBody] UpdateUsersAboutDTO updateUsersAboutDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedUserId = authService.GetUserId(identity);

            var user = await mainAppContext.Users.FindAsync(authenticatedUserId);
            if (user == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User not found"));

            user.About = updateUsersAboutDTO.About;
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse(new { Message = "Users About section updated successfully" }));
        }
    }
}
