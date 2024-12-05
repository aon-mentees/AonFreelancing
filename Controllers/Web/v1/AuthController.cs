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
using Client = AonFreelancing.Models.Client;

namespace AonFreelancing.Controllers.Web.v1
{
    [Route("api/web/v1/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        //private readonly MainAppContext _mainAppContext;
        private readonly AuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly OtpManager _otpManager;
        private readonly JwtService _jwtService;

        public AuthController(
            UserManager<User> userManager,
            MainAppContext mainAppContext,
            RoleManager<ApplicationRole> roleManager,
            OtpManager otpManager,
            JwtService jwtService,
            AuthService authService
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            //_mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _jwtService = jwtService;
            _authService = authService;
        }

        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] PhoneNumberReq phoneNumberReq)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var IsExist = await _authService.IsUserExistsInTempAsync(phoneNumberReq);
            if (IsExist)
            {
                return Conflict(CreateErrorResponse(
                    StatusCodes.Status409Conflict.ToString(), "User already exists."));
            }
            var tempUser = new TempUser()
            {
                PhoneNumber = phoneNumberReq.PhoneNumber,
                PhoneNumberConfirmed = false
            };

            await _authService.AddAsync(tempUser);
            var otp = new Otp()
            {
                PhoneNumber = phoneNumberReq.PhoneNumber,
                Code = _otpManager.GenerateOtp(),
                CreatedDate = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                IsUsed = false,
            };

            await _authService.AddOtpAsync(otp);
            await _otpManager.SendOTPAsync(otp.Code, otp.PhoneNumber);

            return Ok(CreateSuccessResponse("OTP code sent to your phone number, during testing you may not receive it, please use 123456"));
        }

        [HttpPost("verify-phone-number")]
        public async Task<IActionResult> VerifyPhoneNumberAsync([FromBody] PhoneVerificationRequest verifyReq)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            if (await _authService.ProcessPhoneVerificationRequestAsync(verifyReq))
                return Ok(CreateSuccessResponse("Activated"));

            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "UnAuthorized"));
        }

        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistrationAsync([FromBody] UserRegistrationRequest registerReq)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var tempUser = await _authService.GetTempUserAsync(registerReq.PhoneNumber);
            if (tempUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "Phone number is invalid."));
            
            User? user = registerReq.UserType switch
            {
                Constants.USER_TYPE_FREELANCER => new Freelancer
                {
                    Name = registerReq.Name,
                    //UserName = registerReq.Username,
                    PhoneNumber = tempUser.PhoneNumber,
                    PhoneNumberConfirmed = tempUser.PhoneNumberConfirmed,
                    //Skills = registerReq.Skills ,
                },
                Constants.USER_TYPE_CLIENT => new Client()
                {
                    Name = registerReq.Name,
                    //UserName = registerReq.Username,
                    PhoneNumber = tempUser.PhoneNumber,
                    PhoneNumberConfirmed = tempUser.PhoneNumberConfirmed,
                    CompanyName = registerReq.CompanyName ?? string.Empty,
                },
                _ => null
            };

            if (user == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(),
                    "No such user type exists."));

            var userCreationResult = await _userManager.CreateAsync(user, registerReq.Password);
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
            
            var role = new ApplicationRole { Name = registerReq.UserType };
            await _roleManager.CreateAsync(role);
            await _userManager.AddToRoleAsync(user, role.Name);
            await _authService.Remove(tempUser);

            return CreatedAtAction(nameof(UsersController.GetProfileByIdAsync), "users", 
                new { id = user.Id }, null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
            if (user != null && await _userManager.CheckPasswordAsync(user, req.Password))
            {
                if (!await _userManager.IsPhoneNumberConfirmedAsync(user))
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(),
                        "Verify Your Account First"));

                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                var token = _jwtService.GenerateJWT(user, role ?? string.Empty);
                return Ok(CreateSuccessResponse(new LoginResponse()
                {
                    AccessToken = token,
                    UserDetailsDTO = new UserDetailsDTO(user, role ?? string.Empty)
                }));
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