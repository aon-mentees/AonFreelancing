
using AonFreelancing.Contexts;
using AonFreelancing.Hubs;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.DTOs.NoftificationDTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Web.v1
{
    [Authorize]
    [Route("api/web/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, FileStorageService fileStorageService, UserManager<User> userManager,
                                    ProjectLikeService projectLikeService, AuthService authService, PushNotificationService pushNotificationService,
                                    NotificationService notificationService, ProjectService projectService) : BaseController
    {
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromForm] ProjectInputDto projectInputDto)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            Project? newProject = Project.FromInputDTO(projectInputDto, authenticatedUserId);

            if (projectInputDto.ImageFile != null)
                newProject.ImageFileName = await fileStorageService.SaveAsync(projectInputDto.ImageFile);

            await mainAppContext.Projects.AddAsync(newProject);
            await mainAppContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjectDetailsAsync), new { id = newProject.Id }, null);
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpGet("client-feed")]
        public async Task<IActionResult> GetClientFeedAsync(
            [FromQuery] List<string>? qualificationNames, [FromQuery] int page = 0,
            [FromQuery] int pageSize = Constants.PROJECTS_DEFAULT_PAGE_SIZE, [FromQuery] string qur = ""
        )
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            string normalizedQuery = qur.ToLower().Replace(" ", "").Trim();
            PaginatedResult<Project> paginatedProjects = await projectService.FindClientFeedAsync(normalizedQuery, qualificationNames ?? [], page, pageSize);
            List<ProjectOutDTO> projectOutDTOs = paginatedProjects.Result.Select(p => ProjectOutDTO.FromProject(p, imagesBaseUrl)).ToList();
            PaginatedResult<ProjectOutDTO> paginatedProjectsDTO = new PaginatedResult<ProjectOutDTO>(paginatedProjects.Total, projectOutDTOs);

            foreach (var p in projectOutDTOs)
            {
                p.IsLiked = await projectLikeService.IsUserLikedProjectAsync(authenticatedUserId, p.Id);
                PaginatedResult<ProjectLike> paginatedLikes = await projectLikeService.FindLikesForProjectAsync(p.Id, 0, Constants.LIKES_DEFAULT_PAGE_SIZE);
                p.PaginatedLikes = new PaginatedResult<ProjectLikeOutputDTO>(paginatedLikes.Total, paginatedLikes.Result.Select(ProjectLikeOutputDTO.FromProjectLike).ToList());
            }
            return Ok(CreateSuccessResponse(paginatedProjectsDTO));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpGet("freelancer-feed")]
        public async Task<IActionResult> GetProjectFeedAsync(
            [FromQuery(Name = "specializations")] List<string>? qualificationNames,
            [FromQuery(Name = "timeline")] int? duration,
            [FromQuery] PriceRange priceRange,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = Constants.PROJECTS_DEFAULT_PAGE_SIZE,
            [FromQuery] string qur = ""
        )
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            string normalizedQuery = qur.ToLower().Replace(" ", "").Trim();
            PaginatedResult<Project> paginatedProjects = await projectService.FindFreelancerFeedAsync(normalizedQuery, qualificationNames ?? [], priceRange, duration, page, pageSize);
            List<ProjectOutDTO> projectOutDTOs = paginatedProjects.Result.Select(p => ProjectOutDTO.FromProject(p, imagesBaseUrl)).ToList();
            PaginatedResult<ProjectOutDTO> paginatedProjectsDTO = new PaginatedResult<ProjectOutDTO>(paginatedProjects.Total, projectOutDTOs);

            foreach (var p in projectOutDTOs)
            {
                p.IsLiked = await projectLikeService.IsUserLikedProjectAsync(authenticatedUserId, p.Id);
                PaginatedResult<ProjectLike> paginatedLikes = await projectLikeService.FindLikesForProjectAsync(p.Id, 0, Constants.LIKES_DEFAULT_PAGE_SIZE);
                p.PaginatedLikes = new PaginatedResult<ProjectLikeOutputDTO>(paginatedLikes.Total, paginatedLikes.Result.Select(ProjectLikeOutputDTO.FromProjectLike).ToList());
            }
            return Ok(CreateSuccessResponse(paginatedProjectsDTO));
        }


        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost("{projectId}/bids")]
        public async Task<IActionResult> SubmitBidAsync(long projectId, [FromBody] BidInputDto bidInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            long authenticatedFreelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = mainAppContext.Projects.Where(p => p.Id == projectId).Include(p => p.Bids).FirstOrDefault();

            if (storedProject == null)
                return NotFound(CreateErrorResponse("404", "project not found"));
            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return Conflict(CreateErrorResponse("409", "cannot submit a bid for project that is not available for bids"));
            if (storedProject.Budget <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse("400", "proposed price must be less than the project's budget"));
            if (storedProject.Bids.Any() && storedProject.Bids.OrderBy(b => b.ProposedPrice).First().ProposedPrice <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse("400", "proposed price must be less than earlier proposed prices"));

            Bid? newBid = Bid.FromInputDTO(bidInputDTO, authenticatedFreelancerId, projectId);
            await mainAppContext.AddAsync(newBid);
            await mainAppContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created);
        }


        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPut("{projectId}/bids/{bidId}/approve")]
        public async Task<IActionResult> ApproveBidAsync([FromRoute] long projectId, [FromRoute] long bidId)
        {

            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = await projectService.FindProjectWithBidsAsync(projectId);

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found."));
            if (authenticatedClientId != storedProject.ClientId)
                return Forbid();
            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "Project status is not Available."));

            Bid? storedBid = storedProject.Bids.FirstOrDefault(b => b.Id == bidId);
            if (storedBid == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Bid not found"));
            if (storedBid.Status != Constants.BIDS_STATUS_PENDING)
                return Forbid();

            await projectService.ApproveProjectBidAsync(storedBid, storedProject);

            return Ok(CreateSuccessResponse("Bid approved."));
        }
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPut("{projectId}/bids/{bidId}/reject")]
        public async Task<IActionResult> RejectBidAsync([FromRoute] long projectId, [FromRoute] long bidId)
        {

            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            string nameOfAuthenticatedClient = authService.GetNameOfUser((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = await projectService.FindProjectWithBidsAsync(projectId);

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found."));

            if (authenticatedClientId != storedProject.ClientId)
                return Forbid();

            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "Project status is not Available."));

            Bid? storedBid = await projectService.FindBidsAsync(storedProject, bidId);
            if (storedBid == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Bid not found."));

            // Notification
            string notificationMessage = string.Format(Constants.BID_REJECTION_NOTIFICATION_MESSAGE_FORMAT, nameOfAuthenticatedClient, storedProject.Title);
            string notificationTitle = Constants.BID_REJECTION_NOTIFICATION_TITLE;
            var rejectionNotification = new BidRejectionNotification(notificationTitle, notificationMessage, storedBid.FreelancerId, projectId, authenticatedClientId, nameOfAuthenticatedClient, bidId);
            await projectService.RejectProjectBidAsync(storedBid);

            await notificationService.CreateAsync(rejectionNotification);
            await pushNotificationService.SendRejectionNotification(
                BidRejectionNotificationOutputDTO.FromRejectionNotification(rejectionNotification),
                rejectionNotification.ReceiverId);
            return Ok(CreateSuccessResponse("Bid rejected."));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectDetailsAsync(long id)
        {
            var storedProject = await mainAppContext.Projects.Where(p => p.Id == id)
                                                        .Include(p => p.Tasks)
                                                        .Include(p => p.Bids)
                                                        .ThenInclude(b => b.Freelancer)
                                                        .FirstOrDefaultAsync();

            if (storedProject == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            int numberOfCompletedTasks = storedProject.Tasks.Where(t => t.Status == Constants.TASK_STATUS_DONE).ToList().Count;
            decimal totalNumberOFTasks = storedProject.Tasks.Count;
            decimal percentage = 0;
            if (totalNumberOFTasks > 0)
                percentage = (numberOfCompletedTasks / totalNumberOFTasks) * 100;

            var orderedBids = storedProject.Bids
                .OrderByDescending(b => b.ProposedPrice)
                .Select(b => BidOutputDTO.FromBid(b));

            return Ok(CreateSuccessResponse(new
            {
                storedProject.Id,
                storedProject.Title,
                storedProject.Status,
                storedProject.Budget,
                storedProject.Duration,
                storedProject.Description,
                Percentage = percentage,
                Bids = orderedBids
            }));
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTaskAsync(long projectId, [FromBody] TaskInputDTO taskInputDTO)
        {
            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = await mainAppContext.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found"));
            if (authenticatedClientId != storedProject.ClientId)
                return Forbid();
            if (storedProject.Status != Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "project is not status closed yet"));


            TaskEntity? newTask = TaskEntity.FromInputDTO(taskInputDTO, projectId);
            await mainAppContext.Tasks.AddAsync(newTask);
            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Task created successfully."));
        }

        [HttpPost("{projectId}/likes")]
        public async Task<IActionResult> LikeOrUnLikeProject([FromRoute] long projectId, [AllowedValues(Constants.PROJECT_LIKE_ACTION, Constants.PROJECT_UNLIKE_ACTION)] string action)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            var claimsIdentity = (ClaimsIdentity)HttpContext.User.Identity;
            long authenticatedUserId = authService.GetUserId(claimsIdentity);
            string authenticatedLikerName = authService.GetNameOfUser(claimsIdentity);
            Project? storedProject = await mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "project not found"));
            ProjectLike? storedLike = await projectLikeService.Find(authenticatedUserId, projectId);
            if (storedLike != null && action == Constants.PROJECT_LIKE_ACTION)
                return Conflict(CreateErrorResponse("409", "you cannot like the same project twice"));

            if (storedLike != null && action == Constants.PROJECT_UNLIKE_ACTION)
                return await UnLikeProjectAsync(storedLike);
            if (storedLike == null && action == Constants.PROJECT_LIKE_ACTION)
                return await LikeProjectAsync(storedProject, authenticatedUserId, authenticatedLikerName);

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "no like found to be deleted"));
        }

        [HttpGet("{projectId}/likes/count")]
        public async Task<IActionResult> GetProjectLikesCount([FromRoute] long projectId)
        {
            if (!await projectService.IsProjectExistsAsync(projectId))
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found"));
            return Ok(CreateSuccessResponse(new {likesCount = await projectLikeService.CountLikesForProjectAsync(projectId) }));
        }

        [HttpGet("{projectId}/likes")]
        public async Task<IActionResult> GetLikesForProject([FromRoute] long projectId, [FromQuery] int page = 0, [FromQuery] int pageSize = 15)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            if (!await projectService.IsProjectExistsAsync(projectId))
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found"));

            var paginatedLikes = await projectLikeService.FindLikesForProjectAsync(projectId, page, pageSize);
            var paginatedLikesDTO = new PaginatedResult<ProjectLikeOutputDTO>(paginatedLikes.Total, paginatedLikes.Result.Select(ProjectLikeOutputDTO.FromProjectLike).ToList());
            return Ok(CreateSuccessResponse(paginatedLikesDTO));
        }

        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksByProjectIdAsync([FromRoute] long projectId,
                                                                  [AllowedValues(Constants.TASK_STATUS_TO_DO,Constants.TASK_STATUS_DONE,Constants.TASK_STATUS_IN_PROGRESS,Constants.TASK_STATUS_IN_REVIEW,ErrorMessage = $"status should be one of the values: '{Constants.TASK_STATUS_TO_DO}', '{Constants.TASK_STATUS_DONE}', '{Constants.TASK_STATUS_IN_PROGRESS}', '{Constants.TASK_STATUS_IN_REVIEW}', or empty")]
                                                                    [FromQuery] string status = "")
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long authenticatedUserId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Project? storedProject = await mainAppContext.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "project not found"));
            if (authenticatedUserId != storedProject.ClientId && authenticatedUserId != storedProject.FreelancerId)
                return Forbid();

            List<TaskOutputDTO> storedTasksDTOs = await mainAppContext.Tasks.AsNoTracking()
                                                            .Where(t => t.ProjectId == projectId && t.Status.Contains(status))
                                                            .Select(t => TaskOutputDTO.FromTask(t))
                                                            .ToListAsync();
            return Ok(CreateSuccessResponse(storedTasksDTOs));
        }
        private async Task<IActionResult> LikeProjectAsync(Project storedProject, long likerId, string likerName)
        {
            await projectLikeService.LikeProjectAsync(likerId, storedProject.Id, likerName);

            string notificationMessage = string.Format(Constants.LIKE_NOTIFICATION_MESSAGE_FORMAT, likerName, storedProject.Title);
            string notificationTitle = Constants.LIKE_NOTIFICATION_TITLE;
            LikeNotification newLikeNotification = new LikeNotification(notificationTitle, notificationMessage, storedProject.ClientId, storedProject.Id, likerId, likerName);

            await notificationService.CreateAsync(newLikeNotification);
            await pushNotificationService.SendLikeNotification(LikeNotificationOutputDTO.FromLikeNotification(newLikeNotification), newLikeNotification.ReceiverId);
            return Ok("Liked Successfully");
        }
        private async Task<IActionResult> UnLikeProjectAsync(ProjectLike storedProjectLike)
        {
            await projectLikeService.UnlikeProjectAsync(storedProjectLike);
            await notificationService.DeleteForLikeAsync(storedProjectLike);
            return Ok("Unliked successfully");
        }

    }
}