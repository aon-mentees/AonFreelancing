using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly MainAppContext _mainAppContext;
        private readonly AuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly OtpManager _otpManager;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            RoleManager<ApplicationRole> roleManager,
            OtpManager otpManager,
            JwtService jwtService,
            AuthService authService,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _jwtService = jwtService;
            _authService = authService;
            _configuration = configuration;

        }

        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] PhoneNumberReq phoneNumberReq)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            //Checks if this phone number is already used by another user
            if (await _mainAppContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumberReq.PhoneNumber))
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "phone number is already used by an account"));
            //Checks if an otp exists for the received phone number
            if (await _mainAppContext.Otps.AnyAsync(o => o.PhoneNumber == phoneNumberReq.PhoneNumber))
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "otp is already sent"));

            TempUser newTempUser = new TempUser(phoneNumberReq.PhoneNumber);
            string otpCode = _otpManager.GenerateOtp();
            Otp newOtp = new Otp(phoneNumberReq.PhoneNumber, otpCode, Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]));

            await _authService.SaveTempUserAndOtpAsync(newTempUser, newOtp);
            await _otpManager.SendOTPAsync(newOtp.Code, newOtp.PhoneNumber);

            return Ok(CreateSuccessResponse("OTP code sent to your phone number, during testing you may not receive it, please use 123456"));
        }

        [HttpPost("verify-phone-number")]
        public async Task<IActionResult> VerifyPhoneNumberAsync([FromBody] PhoneVerificationRequest phoneVerificationRequest)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            if (await _authService.ProcessPhoneVerificationRequestAsync(phoneVerificationRequest))
                return Ok(CreateSuccessResponse("Activated"));
            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistrationAsync([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            User? storedUser = await _mainAppContext.Users.Where(u => u.PhoneNumber == userRegistrationRequest.PhoneNumber).FirstOrDefaultAsync();
            if (storedUser != null) //check if the provided phone number is already used.
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "phone number is already used"));

            TempUser? storedTempUser = await _mainAppContext.TempUsers.Where(tu => tu.PhoneNumber == userRegistrationRequest.PhoneNumber).FirstOrDefaultAsync();
            if (storedTempUser == null)//check if the request is associated with a temp user record.
                return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "submit and verify your phone number before registering your details"));

            if (userRegistrationRequest.UserType == Constants.USER_TYPE_FREELANCER)
                storedUser = new Freelancer(userRegistrationRequest);
            else if (userRegistrationRequest.UserType == Constants.USER_TYPE_CLIENT)
                storedUser = new Client(userRegistrationRequest);
            if (storedUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "No such user type exists."));
            string normalizedName = storedUser.Name.ToLower().Replace(" ", "");
            string generatedUserName;
            do
            {
                generatedUserName = normalizedName + new Random().Next(999_999);
            } while (await _mainAppContext.Users.AnyAsync(u => u.UserName == generatedUserName));

            storedUser.UserName = generatedUserName;
            storedUser.PhoneNumberConfirmed = true;
            var userCreationResult = await _userManager.CreateAsync(storedUser, userRegistrationRequest.Password);
            if (!userCreationResult.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>()
                {
                    Errors = userCreationResult.Errors
                    .Select(e => new Error()
                    {
                        Code = e.Code,
                        Message = e.Description
                    })
                    .ToList()
                });

            var storedRole = await _roleManager.FindByNameAsync(userRegistrationRequest.UserType);
            await _userManager.AddToRoleAsync(storedUser, storedRole.Name);
            await _authService.Remove(storedTempUser);

            return CreatedAtAction(nameof(UsersController.GetProfileByIdAsync), "users", new { id = storedUser.Id }, null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var storedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
            if (storedUser != null && await _userManager.CheckPasswordAsync(storedUser, req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(storedUser))
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(),
                        "Verify Your Account First"));

                var role = (await _userManager.GetRolesAsync(storedUser)).FirstOrDefault();
                var token = _jwtService.GenerateJWT(storedUser, role ?? string.Empty);
                return Ok(CreateSuccessResponse(new LoginResponse(token, new UserDetailsDTO(storedUser, role ?? string.Empty))));

            }

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        // [HttpPost("forgot-password")]
        // public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordReq req)
        // {
        //     if (string.IsNullOrEmpty(req.PhoneNumber))
        //     {
        //         return BadRequest(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new() { Code = StatusCodes.Status400BadRequest.ToString(), Message = "Invalid request." }
        //             }
        //         });
        //     }
        //
        //     var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
        //     if (user == null)
        //     {
        //         return Ok(new ApiResponse<string>
        //         {
        //             IsSuccess = true,
        //             Results = "If the phone number is registered, you will receive an OTP.",
        //             Errors = []
        //         });
        //     }
        //
        //     var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //     await _otpManager.SendOTPAsync(user.PhoneNumber ?? string.Empty, token);
        //     
        //     return Ok(new ApiResponse<string>
        //     {
        //         IsSuccess = true,
        //         Results = "If the phone number is registered, you will receive an OTP.",
        //         Errors = []
        //     });
        // }
        //
        // [Authorize(Roles = "Freelancer, Client")]
        // [HttpPost("reset-password")]
        // public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordReq req)
        // {
        //     if (string.IsNullOrEmpty(req.PhoneNumber) || string.IsNullOrEmpty(req.Password))
        //     {
        //         return BadRequest(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new() { 
        //                     Code = StatusCodes.Status400BadRequest.ToString(),
        //                     Message = "Invalid request." 
        //                 }
        //             }
        //         });
        //     }
        //
        //     var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
        //     if (user == null)
        //     {
        //         return NotFound(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new() { 
        //                     Code = StatusCodes.Status404NotFound.ToString(),
        //                     Message = "User not found." 
        //                 }
        //             }
        //         });
        //     }
        //
        //     if (req.Password != req.ConfirmPassword)
        //     {
        //         return BadRequest(new ApiResponse<string>
        //         {
        //             IsSuccess = false,
        //             Results = null,
        //             Errors = new List<Error> {
        //                 new()
        //                 {
        //                     Code = StatusCodes.Status400BadRequest.ToString(), 
        //                     Message = "Passwords do not match."
        //                 }
        //             }
        //         });
        //     }
        //
        //     var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //     var result = await _userManager.ResetPasswordAsync(user, token ,req.Password);
        //
        //     if (result.Succeeded)
        //     {
        //         return Ok(new ApiResponse<string>
        //         {
        //             IsSuccess = true,
        //             Results = "Password reset successfully.",
        //             Errors = []
        //         });
        //     }
        //
        //     return BadRequest(new ApiResponse<string>
        //     {
        //         IsSuccess = false,
        //         Results = null,
        //         Errors = result.Errors.Select(e => new Error
        //         {
        //             Code = e.Code,
        //             Message = e.Description
        //         }).ToList()
        //     });
        // }
    }
}