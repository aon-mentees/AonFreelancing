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

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/projects")]
    [ApiController]
    public class ProjectsController(MainAppContext mainAppContext, FileStorageService fileStorageService,
        UserManager<User> userManager, ProjectLikeService projectLikeService, AuthService authService,
        ProjectService projectService, NotificationService notificationService, PushNotificationService pushNotificationService,
        BidService bidService, CommentService commentService, UserService userService)
        : BaseController
    {
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPost]
        public async Task<IActionResult> PostProjectAsync([FromForm] ProjectInputDTO projectInputDto)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            Project? newProject = Project.FromInputDTO(projectInputDto, authenticatedClientId);

            if (projectInputDto.ImageFile != null)
                newProject.ImageFileName = await fileStorageService.SaveAsync(projectInputDto.ImageFile);

            await mainAppContext.Projects.AddAsync(newProject);
            await mainAppContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjectDetailsAsync), new { id = newProject.Id }, null);
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectAsync(long id, [FromForm] ProjectInputDTO projectInputDto)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            var storedUser = await userService.FindByIdAsync(id);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            Project? storedProject = await projectService.FindProjectAsync(id);
            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Project not found or not owned by the authenticated client."));

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a deleted project."));

            if (storedProject.Status == Constants.PROJECT_STATUS_CLOSED)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a project that is closed."));

            storedProject.Title = projectInputDto.Title;
            storedProject.Description = projectInputDto.Description;
            storedProject.QualificationName = projectInputDto.QualificationName;
            storedProject.Duration = projectInputDto.Duration;
            storedProject.PriceType = projectInputDto.PriceType;
            storedProject.Budget = projectInputDto.Budget;

            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Project updated successfully"));
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectAsync(long id)
        {
            var storedUser = await userService.FindByIdAsync(id);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Unable to load user."));

            var storedProject = await projectService.FindProjectAsync(id);
            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(),
                    "Project not found or not owned by the authenticated client."));

            if (storedProject.ClientId != storedUser.Id)
                return Forbid();

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a deleted project."));

            // Soft delete the project
            storedProject.IsDeleted = true;
            storedProject.DeletedAt = DateTime.Now;

            await mainAppContext.SaveChangesAsync();
            return Ok(CreateSuccessResponse("Project deleted successfully"));
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
                PaginatedResult<ProjectLike> paginatedLikes = await projectLikeService.FindLikesForProjectWithLikerUserAsync(p.Id, 0, Constants.LIKES_DEFAULT_PAGE_SIZE);
                p.PaginatedLikes = new PaginatedResult<ProjectLikeOutputDTO>(paginatedLikes.Total, paginatedLikes.Result.Select(pl => ProjectLikeOutputDTO.FromProjectLike(pl, imagesBaseUrl)).ToList());

                PaginatedResult<Comment> paginatedComments = await commentService.GetProjectCommentsAsync(p.Id, 0, Constants.COMMENTS_DEFAULT_PAGE_SIZE, imagesBaseUrl);
                p.paginatedComments = new PaginatedResult<CommentOutputDTO>(paginatedComments.Total, paginatedComments.Result.Select(c => new CommentOutputDTO(c, c.User.Name, imagesBaseUrl)).ToList());
            }
            return Ok(CreateSuccessResponse(paginatedProjectsDTO));
        }
        

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpGet("freelancer-feed")]
        public async Task<IActionResult> GetFreelancerFeedAsync(
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
                PaginatedResult<ProjectLike> paginatedLikes = await projectLikeService.FindLikesForProjectWithLikerUserAsync(p.Id, 0, Constants.LIKES_DEFAULT_PAGE_SIZE);
                p.PaginatedLikes = new PaginatedResult<ProjectLikeOutputDTO>(paginatedLikes.Total, paginatedLikes.Result.Select(pl => ProjectLikeOutputDTO.FromProjectLike(pl, imagesBaseUrl)).ToList());

                PaginatedResult<Comment> paginatedComments = await commentService.GetProjectCommentsAsync(p.Id, 0, Constants.COMMENTS_DEFAULT_PAGE_SIZE, imagesBaseUrl);
                p.paginatedComments = new PaginatedResult<CommentOutputDTO>(paginatedComments.Total, paginatedComments.Result.Select(c => new CommentOutputDTO(c, c.User.Name, imagesBaseUrl)).ToList());
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
            string authenticatedFreelancerName = authService.GetNameOfUser((ClaimsIdentity)HttpContext.User.Identity);
            User? authenticatedFreelancer = await userManager.FindByIdAsync(authenticatedFreelancerId.ToString());
            if (authenticatedFreelancer == null)
                return Unauthorized();
            Project? storedProject = await projectService.FindProjectWithBidsAsync(projectId);

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "cannot submit a bid for project that is deleted"));

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found."));
            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "Cannot submit a bid for project that is not available for bids."));
            if (storedProject.Budget <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Proposed price must be less than the project budget."));
            if (storedProject.Bids.Any() && storedProject.Bids.OrderBy(b => b.ProposedPrice).First().ProposedPrice <= bidInputDTO.ProposedPrice)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Proposed price must be less than earlier proposed prices."));


            Bid? newBid = Bid.FromInputDTO(bidInputDTO, authenticatedFreelancerId, projectId);
            await projectService.ApplyBidAsync(newBid);
            await SubmitBidNotification(storedProject, authenticatedFreelancerId, authenticatedFreelancerName, authenticatedFreelancer.ProfilePicture);
            return StatusCode(StatusCodes.Status201Created);
        }
        [HttpGet("{projectId}/bids")]
        public async Task<IActionResult> GetBidsByProjectId(long projectId, int page = 0, int pageSize = Constants.BIDS_DEFAULT_PAGE_SIZE)
        {
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";

            PaginatedResult<Bid> paginatedBids = await bidService.FindByProjectIdWithFreelancer(projectId, page, pageSize);
            List<BidOutputDTO> bidOutputDTOs = paginatedBids.Result.Select(b => BidOutputDTO.FromBid(b, imagesBaseUrl)).ToList();
            PaginatedResult<BidOutputDTO> paginatedBidOutputDTOs = new PaginatedResult<BidOutputDTO>(paginatedBids.Total, bidOutputDTOs);

            return Ok(paginatedBidOutputDTOs);
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPut("{projectId}/bids/{bidId}/approve")]
        public async Task<IActionResult> ApproveBidAsync([FromRoute] long projectId, [FromRoute] long bidId)
        {

            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            string nameOfAuthenticatedClient = authService.GetNameOfUser((ClaimsIdentity)HttpContext.User.Identity);
            User? authenticatedUser = await userManager.FindByIdAsync(authenticatedClientId.ToString());
            if (authenticatedUser == null)
                return Unauthorized();

            Project? storedProject = await projectService.FindProjectWithBidsAsync(projectId);

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "cannot approve a bid for project that is deleted"));

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

            // Notification 
            string notificationMessage = string.Format(Constants.BID_APPROVAL_NOTIFICATION_MESSAGE_FORMAT, nameOfAuthenticatedClient, storedProject.Title);
            string notificationTitle = Constants.BID_APPROVAL_NOTIFICATION_TITLE;
            string imageUrl = $"{Request.Scheme}://{Request.Host}/images/{authenticatedUser.ProfilePicture}";

            var approvalNotification = new BidApprovalNotification(notificationTitle, notificationMessage, storedBid.FreelancerId, imageUrl, projectId, authenticatedClientId, nameOfAuthenticatedClient, bidId);

            await notificationService.CreateAsync(approvalNotification);
            await pushNotificationService.SendBidApprovalNotification(
                BidApprovalNotificationOutputDTO.FromBidApprovalNotification(approvalNotification),
                approvalNotification.ReceiverId);

            return Ok(CreateSuccessResponse("Bid approved."));
        }
        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpPut("{projectId}/bids/{bidId}/reject")]
        public async Task<IActionResult> RejectBidAsync([FromRoute] long projectId, [FromRoute] long bidId)
        {

            long authenticatedClientId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            string nameOfAuthenticatedClient = authService.GetNameOfUser((ClaimsIdentity)HttpContext.User.Identity);
            User? authenticatedUser = await userManager.FindByIdAsync(authenticatedClientId.ToString());
            if (authenticatedUser == null)
                return Unauthorized();

            Project? storedProject = await projectService.FindProjectWithBidsAsync(projectId);

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "cannot reject a bid for project that is deleted"));

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found."));

            if (authenticatedClientId != storedProject.ClientId)
                return Forbid();

            if (storedProject.Status != Constants.PROJECT_STATUS_AVAILABLE)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "Project status is not Available."));

            Bid? storedBid = await projectService.FindBidsAsync(storedProject, bidId);
            if (storedBid == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Bid not found."));

            await projectService.RejectProjectBidAsync(storedBid);

            // Notification
            string notificationMessage = string.Format(Constants.BID_REJECTION_NOTIFICATION_MESSAGE_FORMAT, nameOfAuthenticatedClient, storedProject.Title);
            string notificationTitle = Constants.BID_REJECTION_NOTIFICATION_TITLE;
            string imageUrl = $"{Request.Scheme}://{Request.Host}/images/{authenticatedUser.ProfilePicture}";

            var rejectionNotification = new BidRejectionNotification(notificationTitle, notificationMessage, storedBid.FreelancerId, imageUrl, projectId, authenticatedClientId, nameOfAuthenticatedClient, bidId);

            await notificationService.CreateAsync(rejectionNotification);
            await pushNotificationService.SendBidRejectionNotification(
                BidRejectionNotificationOutputDTO.FromBidRejectionNotification(rejectionNotification),
                rejectionNotification.ReceiverId);
            return Ok(CreateSuccessResponse("Bid rejected."));
        }

        [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectDetailsAsync(long id)
        {
            var storedProject = await mainAppContext.Projects.Include(p => p.Tasks)
                                                        .Where(p => p.Id == id)
                                                        .FirstOrDefaultAsync();

            if (storedProject == null)
                return NotFound(CreateErrorResponse("404", "Project not found."));

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a deleted project."));

            int numberOfCompletedTasks = storedProject.Tasks.Where(t => t.Status == Constants.TASK_STATUS_DONE).ToList().Count;
            decimal totalNumberOFTasks = storedProject.Tasks.Count;
            decimal percentage = 0;
            if (totalNumberOFTasks > 0)
                percentage = (numberOfCompletedTasks / totalNumberOFTasks) * 100;

            return Ok(CreateSuccessResponse(new
            {
                storedProject.Id,
                storedProject.Title,
                storedProject.Status,
                storedProject.Budget,
                storedProject.Duration,
                storedProject.Description,
                Percentage = percentage
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

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a deleted project."));

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
            User? likerUser = await userManager.FindByIdAsync(authenticatedUserId.ToString());
            if (likerUser == null)
                return Unauthorized();

            Project? storedProject = await mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "project not found"));

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a deleted project."));

            ProjectLike? storedLike = await projectLikeService.Find(authenticatedUserId, projectId);
            if (storedLike != null && action == Constants.PROJECT_LIKE_ACTION)
                return Conflict(CreateErrorResponse("409", "you cannot like the same project twice"));

            if (storedLike != null && action == Constants.PROJECT_UNLIKE_ACTION)
                return await UnLikeProjectAsync(storedLike);
            if (storedLike == null && action == Constants.PROJECT_LIKE_ACTION)
                return await LikeProjectAsync(storedProject, authenticatedUserId, authenticatedLikerName, likerUser.ProfilePicture);

            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "no like found to be deleted"));
        }
        [HttpGet("{projectId}/likes/count")]
        public async Task<IActionResult> GetProjectLikesCount([FromRoute] long projectId)
        {
            if (!await projectService.IsProjectExistsAsync(projectId))
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found"));
            return Ok(CreateSuccessResponse(new { likesCount = await projectLikeService.CountLikesForProjectAsync(projectId) }));
        }
        [HttpGet("{projectId}/likes")]
        public async Task<IActionResult> GetLikesForProject([FromRoute] long projectId, [FromQuery] int page = 0, [FromQuery] int pageSize = 15)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            if (!await projectService.IsProjectExistsAsync(projectId))
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found"));

            string imageBaseUrl = $"{Request.Scheme}://{Request.Host}/images";

            var paginatedLikes = await projectLikeService.FindLikesForProjectWithLikerUserAsync(projectId, page, pageSize);
            var paginatedLikesDTO = new PaginatedResult<ProjectLikeOutputDTO>(paginatedLikes.Total, paginatedLikes.Result.Select(pl => ProjectLikeOutputDTO.FromProjectLike(pl, imageBaseUrl)).ToList());

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

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a deleted project."));

            if (authenticatedUserId != storedProject.ClientId && authenticatedUserId != storedProject.FreelancerId)
                return Forbid();

            List<TaskOutputDTO> storedTasksDTOs = await mainAppContext.Tasks.AsNoTracking()
                                                            .Where(t => t.ProjectId == projectId && t.Status.Contains(status))
                                                            .Select(t => TaskOutputDTO.FromTask(t))
                                                            .ToListAsync();
            return Ok(CreateSuccessResponse(storedTasksDTOs));
        }
        private async Task<IActionResult> LikeProjectAsync(Project storedProject, long likerId, string likerName, string? likerProfilePicture)
        {
            await projectLikeService.LikeProjectAsync(likerId, storedProject.Id, likerName);

            string notificationMessage = string.Format(Constants.LIKE_NOTIFICATION_MESSAGE_FORMAT, likerName, storedProject.Title);
            string notificationTitle = Constants.LIKE_NOTIFICATION_TITLE;
            string imageUrl = $"{Request.Scheme}://{Request.Host}/images/{likerProfilePicture}";

            LikeNotification newLikeNotification = new LikeNotification(notificationTitle, notificationMessage, storedProject.ClientId, imageUrl, storedProject.Id, likerId, likerName);

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
        private async Task SubmitBidNotification(Project storedProject, long freelancerId, string freelancerName, string? freelancerProfilePicture)
        {

            string notificationMessage = string.Format(Constants.SUBMIT_BID_NOTIFICATION_MESSAGE_FORMAT, freelancerName, storedProject.Title);
            string notificationTitle = Constants.SUBMIT_BID_NOTIFICATION_TITLE;
            string imageUrl = $"{Request.Scheme}://{Request.Host}/images/{freelancerProfilePicture}";

            SubmitBidNotification newSubmitBidNotification = new SubmitBidNotification(notificationTitle, notificationMessage, storedProject.ClientId, imageUrl, storedProject.Id, freelancerId, freelancerName);

            await notificationService.CreateAsync(newSubmitBidNotification);
            await pushNotificationService.SendSubmitBidNotification(BidSubmissionNotificationOutputDTO.FromSubmitBidNotification(newSubmitBidNotification), newSubmitBidNotification.ReceiverId);

        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT},{Constants.USER_TYPE_FREELANCER}")]
        [HttpPost("{projectId}/comments")]
        public async Task<IActionResult> CreateCommentAsync([FromRoute] long projectId, [FromForm] CommentInputDTO commentInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            User? authenticatedUser = await userManager.GetUserAsync(HttpContext.User);

            if (authenticatedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User not found"));

            Project? storedProject = await mainAppContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (storedProject == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found"));

            if (storedProject.IsDeleted)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Cannot update a deleted project."));

            Comment? comment = new Comment(commentInputDTO, projectId, authenticatedUser.Id);
            if (commentInputDTO.ImageFile != null)
                comment.ImageUrl = await fileStorageService.SaveAsync(commentInputDTO.ImageFile);

            await commentService.SaveCommentAsync(comment);

            //Notification
            string notificationMessage = string.Format(Constants.COMMENT_NOTIFICATION_MESSAGE_FORMAT, authenticatedUser.Name, storedProject.Title);
            string notificationTitle = Constants.COMMENT_NOTIFICATION_TITLE;
            string imageUrl = $"{Request.Scheme}://{Request.Host}/images/{authenticatedUser.ProfilePicture}";

            var newCommentNotification =
                new CommentNotification(notificationTitle, notificationMessage, storedProject.ClientId, imageUrl, authenticatedUser.Name, storedProject.Id, authenticatedUser.Id);
            await notificationService.CreateAsync(newCommentNotification);
            await pushNotificationService.SendCommentNotification(CommentNotificationOutputDTO.FromCommentNotification(newCommentNotification), newCommentNotification.ReceiverId);

            return Ok(CreateSuccessResponse("Commented"));
        }

        [HttpGet("{projectId}/comments")]
        public async Task<IActionResult> GetProjectCommentsAsync([FromRoute] long projectId, [FromQuery] int page = 0, [FromQuery] int pageSize = Constants.COMMENTS_DEFAULT_PAGE_SIZE)
        {
            string imagesBaseUrl = $"{Request.Scheme}://{Request.Host}/images";
            var projectExists = await mainAppContext.Projects.AnyAsync(p => p.Id == projectId);

            if (!projectExists)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Project not found !"));

            PaginatedResult<Comment> paginatedComments = await commentService.GetProjectCommentsAsync(projectId, page, pageSize, imagesBaseUrl);
            PaginatedResult<CommentOutputDTO> paginatedCommentOutDTOs = new PaginatedResult<CommentOutputDTO>(paginatedComments.Total, paginatedComments.Result.Select(c => new CommentOutputDTO(c, c.User.Name, imagesBaseUrl)).ToList());
            return Ok(CreateSuccessResponse(paginatedCommentOutDTOs));
        }

    }
}