
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AonFreelancing.Services;
using AonFreelancing.Models.Responses;
using System.Security.Claims;
using static AonFreelancing.Utilities.Constants;
using Microsoft.AspNetCore.Identity;
namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/clients")]
    [ApiController]
    public class ClientsController(
        MainAppContext mainAppContext, ActivitiesService activitiesService, UserService userService,
        UserManager<User> userManager, ProjectService projectService, RatingService ratingService,
        AuthService authService, RoleManager<ApplicationRole> roleManager,ClientService clientService) : BaseController
    { 
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPatch]
        public async Task<IActionResult> UpdateClientAsync([FromBody] ClientUpdateDTO clientUpdateDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            long clientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            var storedClient = await clientService.FindClientByIdAsync(clientId);
            if (storedClient == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Authenticated user not found"));

            storedClient.Name = clientUpdateDTO.Name;
            storedClient.CompanyName = clientUpdateDTO.CompanyName;

            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Client updated successfully"));
        }     
   
        [HttpGet("{id}/activities")]
        public async Task<IActionResult> GetActivitiesAsync(long id)
        {
            var storedUser = await userService.FindByIdAsync(id);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Not Found"));
            var isClient = await userService.IsClient(storedUser);
            if(!isClient)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Not a client"));
            var responseDTO = activitiesService.ClientActivities(id);
            return Ok(CreateSuccessResponse(responseDTO));
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpGet("recent-projects")]
        public async Task<IActionResult> GetRecentProjectsAsync(int page = 0, int pageSize = Constants.RECENT_PROJECTS_DEFAULT_PAGE_SIZE)
        {
            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";

            PaginatedResult<Project> paginatedProjects = await projectService.FindProjectsByClientId(authenticatedClientId, page, pageSize);
            List<RecentProjectOutputDTO> recentProjectOutputDTOs = paginatedProjects.Result.Select(p=>RecentProjectOutputDTO.FromProject(p, imagesBaseUrl)).ToList();

            return Ok(CreateSuccessResponse(new PaginatedResult<RecentProjectOutputDTO>(paginatedProjects.Total, recentProjectOutputDTOs)));
        }

        [Authorize(Roles = USER_TYPE_CLIENT)]
        [HttpGet("freelancers-worked-with")]
        public async Task<IActionResult> GetFreelancersWorkedWith(int page = 0, int pageSize = FREELANCERS_WORKED_WITH_DEFAULT_PAGE_SIZE)
        {
            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            var storedProjects = await mainAppContext.Projects.Include(p => p.Freelancer)
                                                               .OrderByDescending(p => p.EndDate)
                                                               .Where(p => p.ClientId == authenticatedClientId)
                                                               .Where(p => p.FreelancerId != null)
                                                               .Where(p => !p.IsDeleted)
                                                               .ToListAsync();

            PaginatedResult<FreelancerWorkedWithOutDTO> paginatedFreelancerWorkedWithDTO = new PaginatedResult<FreelancerWorkedWithOutDTO>();
            HashSet<long> freelancerIds = [];
            foreach (var project in storedProjects)
            {
                if (project.Freelancer == null || project.FreelancerId == null || freelancerIds.Contains(project.FreelancerId.Value))
                    continue;
                double rating = await ratingService.GetAverageRatingForUserAsync(project.FreelancerId.Value);
                paginatedFreelancerWorkedWithDTO.Total++;
                paginatedFreelancerWorkedWithDTO.Result.Add(new FreelancerWorkedWithOutDTO(project.Freelancer, rating, project.EndDate, imagesBaseUrl));
                freelancerIds.Add(project.FreelancerId.Value);
            }
            paginatedFreelancerWorkedWithDTO.Result = paginatedFreelancerWorkedWithDTO.Result.Skip(page* pageSize)
                                                                                             .Take(pageSize)
                                                                                             .ToList();
            return Ok(CreateSuccessResponse(paginatedFreelancerWorkedWithDTO));
        }
        //private readonly MainAppContext _mainAppContext;
        //private readonly UserManager<User> _userManager;
        //public ClientsController(
        //    MainAppContext mainAppContext,
        //    UserManager<User> userManager
        //    )
        //{
        //    _mainAppContext = mainAppContext;
        //    _userManager = userManager;
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAllAsync([FromQuery] string? Mode)
        //{
        //    var ClientList = new List<ClientDTO>();
        //    if (Mode == null || Mode == "basic")
        //    {
        //        ClientList = await _userManager.Users.OfType<Client>()
        //         .Include(c => c.Projects)
        //          .Select(c => new ClientDTO
        //          {
        //              Id = c.Id,
        //              CompanyName = c.CompanyName,
        //              Name = c.Name,
        //              Username = c.UserName
        //          })
        //         .ToListAsync();
        //    }
        //    if (Mode == "r")
        //    {
        //        ClientList = await _userManager.Users.OfType<Client>()
        //        .Include(c => c.Projects)
        //         .Select(c => new ClientDTO
        //         {
        //             Id = c.Id,
        //             CompanyName = c.CompanyName,
        //             Name = c.Name,
        //             Username = c.UserName,
        //             Projects = c.Projects.Select(p => new ProjectOutDTO
        //             {
        //                 Id = p.Id,
        //                 Title = p.Title,
        //                 Description = p.Description,

        //             })
        //         })
        //        .ToListAsync();
        //    }

        //    return Ok(CreateSuccessResponse(ClientList));
        //}


    }
}
