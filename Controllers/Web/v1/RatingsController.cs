using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AonFreelancing.Controllers.Web.v1
{
    [Route("api/web/v1/ratings")]
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRatingAsync([FromBody] RatingInputDTO request)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedRaterUserId = _authService.GetUserId(identity);

            if (authenticatedRaterUserId == request.RatedUserId)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "You cannot rate yourself."));

            if (!await _projectService.IsUser1WorkedWithUser2Async(authenticatedRaterUserId, request.RatedUserId))
                return Forbid();

            if (await _ratingService.HasUserRatedAsync(authenticatedRaterUserId, request.RatedUserId))
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "You cannot rate the same user twice."));

            await _ratingService.Save(authenticatedRaterUserId, request.RatedUserId, request.RatingValue, request.Comment);

            return Ok(CreateSuccessResponse("Rating created successfully."));
        }

        [Authorize]
        [HttpGet("{userId}/ratings")]
        public async Task<IActionResult> GetRatingsForUser(long userId)
        {
            var ratings = await _ratingService.GetRatingsForUserAsync(userId);

            if (ratings == null || !ratings.Any())
                return NotFound(CreateErrorResponse(
                    StatusCodes.Status404NotFound.ToString(),
                    "No ratings found for the specified user."
                ));

            return Ok(CreateSuccessResponse(ratings));
        }

        [Authorize]
        [HttpGet("{userId}/average-rate")]
        public async Task<IActionResult> GetAverageRating(long userId)
        {
            var average = await _ratingService.GetAverageRatingForUserAsync(userId);

            if (average == null)
                return NotFound(CreateErrorResponse(
                    StatusCodes.Status404NotFound.ToString(),
                    "No ratings available to calculate average."
                ));

            return Ok(CreateSuccessResponse(new { UserId = userId, AverageRating = average }));
        }
    }
}
