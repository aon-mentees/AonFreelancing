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
        private readonly UserService _userService;

        public RatingsController(RatingService ratingService, AuthService authService, ProjectService projectService, UserService userService)
        {
            _ratingService = ratingService;
            _authService = authService;
            _projectService = projectService;
            _userService = userService;
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
        public async Task<IActionResult> GetRatingsForUser([FromQuery] long userId, [FromQuery] int page = 0, [FromQuery] int pageSize = Constants.RATING_DEFAULT_PAGE_SIZE)
        {
          
            IEnumerable<RatingOutputDTO> storedRatings = (await _ratingService.GetRatingsForUserAsync(userId, page, pageSize)).Select(r => new RatingOutputDTO(r));
            return Ok(CreateSuccessResponse(storedRatings));
        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT}, {Constants.USER_TYPE_FREELANCER}")]
        [HttpGet("average-rate")]
        public async Task<IActionResult> GetAverageRating([FromQuery] long userId)
        {
            var storedUser = await _userService.FindByIdAsync(userId);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User Not Found !"));

            var average = await _ratingService.GetAverageRatingForUserAsync(userId);
            return Ok(CreateSuccessResponse(new { AverageRating = average }));
        }

        [Authorize(Roles = $"{Constants.USER_TYPE_CLIENT}, {Constants.USER_TYPE_FREELANCER}")]
        [HttpGet("rating-summary")]
        public async Task<IActionResult> GetUserRating([FromQuery] long userId)
        {
            var storedUser = await _userService.FindByIdAsync(userId);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "User Not Found !"));

            RatingSummaryDTO userRating = await _ratingService.GetRatingCalculationForUserAsync(userId);
            return Ok(CreateSuccessResponse(userRating));
        }
    }
}