using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Route("api/mobile/v1/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        readonly AuthService _authService;
        readonly UserService _userService;
        public AuthController(AuthService authService,UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }
        [HttpPut("resend-verification-code")]
        public async Task<IActionResult> ResendOtpAsync([FromBody] PhoneNumberReq phoneNumberReq)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var storedOTP = await _authService.FindOtpByPhoneNumber(phoneNumberReq.PhoneNumber);
            if (storedOTP == null)
                return NotFound(CreateErrorResponse(
                StatusCodes.Status404NotFound.ToString(), "No OTP entry found for the specified phone number."));

            if (storedOTP.IsUsed)
                return Conflict(CreateErrorResponse(
                StatusCodes.Status409Conflict.ToString(), "OTP for this phone number is already used."));

            string regeneratedOtpCode = await _authService.RecreateOtpCodeAsync(storedOTP);
            await _authService.SendOtpAsync(regeneratedOtpCode, phoneNumberReq.PhoneNumber);

            return Ok(CreateSuccessResponse("OTP code resent to your phone number, during testing you may not receive it, please use 123456"));
        }
        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCodeAsync([FromBody] PhoneNumberReq phoneNumberReq)
        {
            //checks input validation
            if (!ModelState.IsValid)
                return CustomBadRequest();
            var validationResult = await _authService.CanSendOtpAsync(phoneNumberReq.PhoneNumber);
              if (!validationResult.IsSuccess)           
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), validationResult.ErrorMessage));
            
            string generatedOtpCode = await _authService.CreateTempUserAndOtp(phoneNumberReq);
            await _authService.SendOtpAsync(generatedOtpCode, phoneNumberReq.PhoneNumber);
            
            return Ok(CreateSuccessResponse("OTP code sent to your phone number, during testing you may not receive it, please use 123456"));
        }

        [HttpPost("verify-phone-number")]
        public async Task<IActionResult> VerifyPhoneNumberAsync([FromBody] PhoneVerificationRequest phoneVerificationRequest)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            if (await _authService.ProcessPhoneVerificationRequestAsync(phoneVerificationRequest))
                return Ok(CreateSuccessResponse("Activated"));
            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Unauthorized"));
        }

        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistrationAsync([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            string normalizedReceivedEmail = userRegistrationRequest.Email.ToUpper();
            bool isUserExistsByNormalizedEmail = await _authService.IsUserExistsByNormalizedEmailAsync(normalizedReceivedEmail);
            bool isUserExistsByPhoneNumber = await _authService.IsUserExistsByPhoneNumberAsync(userRegistrationRequest.PhoneNumber);
            TempUser? storedTempUser = await _authService.FindTempUserByPhoneNumberAsync(userRegistrationRequest.PhoneNumber);

            if (isUserExistsByNormalizedEmail)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "This email address is already used"));
            if (isUserExistsByPhoneNumber)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "This phone number is already used"));
            if (storedTempUser == null)
                return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Submit and verify your phone number before registering your details"));
            User? newUser = null;
            if (userRegistrationRequest.UserType == Constants.USER_TYPE_FREELANCER)
                newUser = new Freelancer(userRegistrationRequest);
            else if (userRegistrationRequest.UserType == Constants.USER_TYPE_CLIENT)
            {
                if (userRegistrationRequest.CompanyName == null)
                    return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "company name is required for clients"));
                newUser = new Client(userRegistrationRequest);
            }
            if (newUser == null)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "No such user type exists."));

            newUser.UserName = await _authService.GenerateUserNameFromName(newUser.Name);
            newUser.PhoneNumberConfirmed = true;
            var userCreationResult = await _authService.CreateUserAsync(newUser, userRegistrationRequest.Password);
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
            await _authService.AssignRoleToUserAsync(newUser, userRegistrationRequest.UserType);
            await _authService.RemoveTempUser(storedTempUser);

            return CreatedAtAction(nameof(ProfileController.GetProfileByIdAsync), "Profile", new { id = newUser.Id }, null);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();
            if (await _authService.ValidateCredentialsAsync(req.Email, req.Password))
            {
                User? storedUser = await _authService.FindUserByEmailAsync(req.Email);
                if (!storedUser.PhoneNumberConfirmed)
                    return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Verify Your Account First"));
                if (storedUser.IsDeleted)
                {
                    var canReactivate = await _authService.IsAccountPermanentlyDeletedAsync(storedUser);
                    if (canReactivate)
                        return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Your account has been permanently deleted."));
                    storedUser.IsDeleted = false;
                    storedUser.DeletedAt = null;
                    await _userService.SaveChangesAsync();
                }
                string role = await _authService.FindUserRoleAsync(storedUser);
                string token = await _authService.GenerateAuthToken(storedUser, role);
                return Ok(CreateSuccessResponse(new LoginResponse(token, new UserDetailsDTO(storedUser, role ?? string.Empty))));
            }
            return Unauthorized(CreateErrorResponse(StatusCodes.Status401Unauthorized.ToString(), "Unauthorized"));
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