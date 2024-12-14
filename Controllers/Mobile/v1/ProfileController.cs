using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.DTOs.NoftificationDTOs;
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
    [Route("api/mobile/v1/profiles")]
    [ApiController]
    public class ProfileController(MainAppContext mainAppContext, AuthService authService, NotificationService notificationService)
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

        [HttpPatch("about")]
        public async Task<IActionResult> UpdateAboutAsync([FromBody] UserAboutInputDTO userAboutInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            long authonticatedUser = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            
            User? storedUser = await mainAppContext.Users.FindAsync(authonticatedUser);

            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User not found"));

            storedUser.About = userAboutInputDTO.About;
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Users About section updated successfully"));
        }
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            var storedNotifications = await notificationService.FindNotificationsForUserAsync(authenticatedUserId);
            var notificationOutputDTOs = ToNotificationOutputDTOs(storedNotifications);

            //this method calls must be placed after loading the notifications to their corresponding DTOs, prevent prematurely modifying the IsRead property of notificationOutputDTO.
            await notificationService.MarkNotificationsAsReadAsync(storedNotifications);

            return Ok(CreateSuccessResponse(notificationOutputDTOs));
        }
        private static List<NotificationOutputDTO> ToNotificationOutputDTOs(List<Notification> notifications)
        {
            var notificationOutputDTOs = new List<NotificationOutputDTO>();
            foreach (var notification in notifications)
            {
                switch (notification)
                {
                    case LikeNotification likeNotification:
                        notificationOutputDTOs.Add(LikeNotificationOutputDTO.FromLikeNotification(likeNotification));
                        break;
                    case BidRejectionNotification bidRejectionNotification:
                        notificationOutputDTOs.Add(BidRejectionNotificationOutputDTO.FromRejectionNotification(bidRejectionNotification));
                        break;
                    case BidApprovalNotification bidApprovalNotification:
                        notificationOutputDTOs.Add(BidApprovalNotificationOutputDTO.FromApprovalNotification(bidApprovalNotification));
                        break;
                    case SubmitBidNotification bidSubmissionNotification:
                        notificationOutputDTOs.Add(BidSubmissionNotificationOutputDTO.FromSubmitBidNotification(bidSubmissionNotification));
                        break;
                }
            }
            return notificationOutputDTOs;
        }
    }

}
