using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/clients")]
    [ApiController]
    public class ClientsController(MainAppContext mainAppContext, UserManager<User> userManager, AuthService authService, RoleManager<ApplicationRole> roleManager)
        : BaseController
    {
        // before your review, you should i have skill issue here cuz the time is now 11:56 PM :)
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPatch]
        public async Task<IActionResult> UpdateClientAsync([FromBody] ClientUpdateDTO clientUpdateDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var storedUser = await userManager.GetUserAsync(HttpContext.User);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Authenticated user not found"));

            if (storedUser is not Client client)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Authenticated user is not a client"));

            if (!string.IsNullOrEmpty(clientUpdateDTO.Name))
                client.Name = clientUpdateDTO.Name;

            if (!string.IsNullOrEmpty(clientUpdateDTO.CompanyName))
                client.CompanyName = clientUpdateDTO.CompanyName;

            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Client updated successfully"));
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
