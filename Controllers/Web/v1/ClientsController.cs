using System.Reflection.Metadata;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AonFreelancing.Services;

namespace AonFreelancing.Controllers.Web.v1
{
    [Authorize]
    [Route("api/web/v1/clients")]
    [ApiController]
    public class ClientsController(MainAppContext mainAppContext,
                                    ActivitiesService activitiesService,
                                    UserService userService) : BaseController
    {
        [HttpGet("{id}/activities")]
        public async Task<IActionResult> GetActivitiesAsync(long id)
        {
            var storedUser = await userService.FindByIdAsync(id);
            if(storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Not Found"));
            var isClient = await userService.IsClient(storedUser);
            if(!isClient)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Not a client"));
            var responseDTO = activitiesService.ClientActivities(id);
            return Ok(CreateSuccessResponse(responseDTO));
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
