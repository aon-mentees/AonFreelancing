using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AonFreelancing.Controllers.Mobile.v1
{
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRatingAsync([FromBody] RatingInputDTO request)
        {
            if(!ModelState.IsValid)
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

            return Ok(new { Message = "Rating added successfully." });
        }


        [HttpGet("{userId}/ratings")]
        public async Task<IActionResult> GetRatingsForUser(int userId)
        {
            var ratings = await _ratingService.GetRatingsForUserAsync(userId);
            return Ok(ratings);
        }

        [Authorize]
        [HttpGet("{userId}/average-rate")]
        public async Task<IActionResult> GetAverageRating(int userId)
        {
            var average = await _ratingService.GetAverageRatingForUserAsync(userId);
            return Ok(new { UserId = userId, AverageRating = average });
        }
    }
}
