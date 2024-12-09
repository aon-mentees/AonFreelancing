using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AonFreelancing.Controllers.Web.v1
{
    [Authorize]
    [Route("api/web/v1/rate")]
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

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT},{Constants.USER_TYPE_FREELANCER}")]
        [HttpPost]
        public async Task<IActionResult> CreateRatingAsync([FromBody] RatingInputDTO request)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            long authenticatedRaterUserId = _authService.GetUserId(identity);

            if (authenticatedRaterUserId == request.RatedUserId)
                return Forbid("You cannot rate yourself.");

            if (!await _projectService.IsUser1WorkedWithUser2Async(authenticatedRaterUserId, request.RatedUserId))
                return Forbid();

            if (await _ratingService.IsRatingAlreadyGivenAsync(authenticatedRaterUserId, request.RatedUserId))
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "You cannot rate the same user twice."));

            await _ratingService.Save(authenticatedRaterUserId, request.RatedUserId, request.RatingValue, request.Comment);

            return Ok(CreateSuccessResponse("Rating created successfully."));
        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT},{Constants.USER_TYPE_FREELANCER}")]
        [HttpGet("{userid}/rate")]
        public async Task<IActionResult> GetRatingsForUser(long userid)
        {
            var ratings = await _ratingService.GetRatingsForUserAsync(userid);

            //if (ratings == null || !ratings.Any())
            //    return Ok(CreateSuccessResponse());

            return Ok(CreateSuccessResponse(ratings));
        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT},{Constants.USER_TYPE_FREELANCER}")]
        [HttpGet("{userId}/average-rate")]
        public async Task<IActionResult> GetAverageRating(long userId)
        {
            var average = await _ratingService.GetAverageRatingForUserAsync(userId);

            //if (average == null)
            //    return NotFound(CreateErrorResponse(
            //        StatusCodes.Status404NotFound.ToString(),
            //        "No ratings available to calculate average."
            //    ));

            return Ok(CreateSuccessResponse(new {AverageRating = average }));
        }
    }
}