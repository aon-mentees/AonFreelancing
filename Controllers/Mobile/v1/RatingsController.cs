using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/ratings")]
    [ApiController]
    public class RatingsController : BaseController
    {
        private readonly RatingService _ratingService;
        private readonly AuthService _authService;
        private readonly ProjectService _projectService;

        public RatingsController(RatingService ratingService, AuthService authService, ProjectService projectService)
        {
            _ratingService = ratingService;
            _authService = authService;
            _projectService = projectService;
        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT}, {Constants.USER_TYPE_FREELANCER}")]
        [HttpPost]
        public async Task<IActionResult> CreateRatingAsync([FromBody] RatingInputDTO request)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedRaterUserId = _authService.GetUserId(identity);


            if (authenticatedRaterUserId == request.RatedUserId || !await _projectService.IsUser1WorkedWithUser2Async(authenticatedRaterUserId, request.RatedUserId))
                return Forbid();

            if (await _ratingService.IsRatingAlreadyGivenAsync(authenticatedRaterUserId, request.RatedUserId))
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "You cannot rate the same user twice."));

            await _ratingService.Save(new Rating(request, authenticatedRaterUserId));

            return Ok(CreateSuccessResponse("Rating created successfully."));
        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT}, {Constants.USER_TYPE_FREELANCER}")]
        [HttpGet]
        public async Task<IActionResult> GetRatingsForUser([FromQuery] long userId)
        {
            IEnumerable<RatingOutputDTO> storedRatings = (await _ratingService.GetRatingsForUserAsync(userId)).Select(r => new RatingOutputDTO(r));
            return Ok(CreateSuccessResponse(storedRatings));
        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT}, {Constants.USER_TYPE_FREELANCER}")]
        [HttpGet("average-rate")]
        public async Task<IActionResult> GetAverageRating([FromQuery] long userId)
        {
            var average = await _ratingService.GetAverageRatingForUserAsync(userId);
            return Ok(CreateSuccessResponse(new { AverageRating = average }));
        }
    }
}