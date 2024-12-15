using AonFreelancing.Attributes;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.DTOs.NoftificationDTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static AonFreelancing.Utilities.Constants;
namespace AonFreelancing.Controllers.Web.v1
{
    [Authorize]
    [Route("api/web/v1/profiles")]
    [ApiController]
    public class ProfileController(MainAppContext mainAppContext, AuthService authService, NotificationService notificationService, ProjectService projectService, FileStorageService fileStorageService, UserService userService)
        : BaseController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileByIdAsync([FromRoute] long id)
        {
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            FreelancerResponseDTO? storedFreelancerDTO = await mainAppContext.Users
                .OfType<Freelancer>()
                .Include(f => f.Skills)
                .Where(f => f.Id == id)
                .Select(f => FreelancerResponseDTO.FromFreelancer(f,imagesBaseUrl))
                .FirstOrDefaultAsync();

            if (storedFreelancerDTO != null)
                return Ok(CreateSuccessResponse(storedFreelancerDTO));

            ClientResponseDTO? storedClientDTO = await mainAppContext.Users
                .OfType<Client>()
                .Where(c => c.Id == id)
                .Include(c => c.Projects)
                .Select(c => ClientResponseDTO.FromClient(c, imagesBaseUrl))
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
        [HttpGet("projects")]
        public async Task<IActionResult> GetProjectsForUserDashboard([AllowedValues(PROJECT_STATUS_AVAILABLE, PROJECT_STATUS_CLOSED, PROJECT_STATUS_COMPLETED)] string status = "",
                                                                        int page = 0, int pageSize = PROJECTS_DEFAULT_PAGE_SIZE)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var identity = (ClaimsIdentity)HttpContext.User.Identity;
            long authenticatedUserId = authService.GetUserId(identity);
            string authenticatedUserRole = authService.GetUserRole(identity);

            if (authenticatedUserRole == Constants.USER_TYPE_CLIENT)
            {
                PaginatedResult<Project> paginatedProjects = await projectService.FindProjectsByClientIdWithTasksAndClient(authenticatedUserId, page, pageSize, status);
                List<Project> storedProjects = paginatedProjects.Result;
                PaginatedResult<DashboardProjectOutputDTO> paginatedDashboardProjectDTOs = new(paginatedProjects.Total, storedProjects.Select(p => new DashboardProjectOutputDTO(p, CalculateProjectCompletionPercentage(p))).ToList());
                return Ok(CreateSuccessResponse(paginatedDashboardProjectDTOs));
            }
            if (authenticatedUserRole == Constants.USER_TYPE_FREELANCER)
            {
                PaginatedResult<Project> paginatedProjects = await projectService.FindProjectsByFreelancerIdWithTasksAndClient(authenticatedUserId, page, pageSize, status);
                List<Project> storedProjects = paginatedProjects.Result;
                PaginatedResult<DashboardProjectOutputDTO> paginatedDashboardProjectDTOs = new(paginatedProjects.Total, storedProjects.Select(p => new DashboardProjectOutputDTO(p, CalculateProjectCompletionPercentage(p))).ToList());
                return Ok(CreateSuccessResponse(paginatedDashboardProjectDTOs));
            }
            else
                return StatusCode(StatusCodes.Status501NotImplemented, CreateErrorResponse(StatusCodes.Status501NotImplemented.ToString(), "Currently there is no implementation for user roles other than 'client' and 'freelancer'"));
        }
        private string CalculateProjectCompletionPercentage(Project project)
        {
            if (project.Tasks == null)
                return "0%";
            int total = project.Tasks.Count;
            int countOfCompletedTasks = project.Tasks.Count(t => t.Status == Constants.TASK_STATUS_DONE);
            double percentage = (double)countOfCompletedTasks / total * 100;

            return $"{percentage:F2}%";
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
                        notificationOutputDTOs.Add(BidRejectionNotificationOutputDTO.FromBidRejectionNotification(bidRejectionNotification));
                        break;
                    case BidApprovalNotification bidApprovalNotification:
                        notificationOutputDTOs.Add(BidApprovalNotificationOutputDTO.FromBidApprovalNotification(bidApprovalNotification));
                        break;
                    case SubmitBidNotification bidSubmissionNotification:
                        notificationOutputDTOs.Add(BidSubmissionNotificationOutputDTO.FromSubmitBidNotification(bidSubmissionNotification));
                        break;
                }
            }
            return notificationOutputDTOs;
        }
        [HttpPatch("profile-picture")]
        public async Task<IActionResult> CreateProfilePictureAsync([AllowedFileExtensions([JPEG, JPG, PNG]), MaxFileSize(MAX_FILE_SIZE)] IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            User? authenticatedUser = await userService.FindByIdAsync(authenticatedUserId);
            if (authenticatedUser == null)
                return Unauthorized();

            string imageFileName = await fileStorageService.SaveAsync(imageFile);
            if (authenticatedUser.ProfilePicture != DEFAULT_USER_PROFILE_PICTURE)
                fileStorageService.Delete(authenticatedUser.ProfilePicture);

            authenticatedUser.ProfilePicture = imageFileName;
            await userService.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("profile-picture")]
        public async Task<IActionResult> DeleteProfilePictureAsync()
        {
            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            User? authenticatedUser = await userService.FindByIdAsync(authenticatedUserId);
            if (authenticatedUser == null)
                return Unauthorized();
            if (authenticatedUser.ProfilePicture != DEFAULT_USER_PROFILE_PICTURE)
                fileStorageService.Delete(authenticatedUser.ProfilePicture);

            authenticatedUser.ProfilePicture = DEFAULT_USER_PROFILE_PICTURE;
            await userService.SaveChangesAsync();
            return NoContent();
        }
    }

}
