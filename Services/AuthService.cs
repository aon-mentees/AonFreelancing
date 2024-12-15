using AonFreelancing.Commons;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Data;
using System.Security.Claims;
using static System.Net.WebRequestMethods;


namespace AonFreelancing.Services
{
    public class AuthService
    {
        private readonly OtpManager _otpManager;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly TempUserService _tempUserService;
        private readonly OTPService _otpService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        
        public AuthService(OtpManager otpManager,
                            JwtService jwtService, TempUserService tempUserService,
                            OTPService otpService, UserService userServic, 
                            RoleService roleService, IConfiguration configuration)
        {
            _otpManager = otpManager;
            _configuration = configuration;
            _jwtService = jwtService;
            _tempUserService = tempUserService;
            _otpService = otpService;
            _userService = userServic;
            _roleService = roleService;
        }

        // TempUser Methods
        public async Task<TempUser?> FindTempUserByPhoneNumberAsync(string phoneNumber) => await _tempUserService.FindByPhoneNumberAsync(phoneNumber);        
        public string GetNameOfUser(ClaimsIdentity identity) => identity.FindFirst(ClaimTypes.GivenName).Value;
        public string GetUserRole(ClaimsIdentity identity) => identity.FindFirst(ClaimTypes.Role).Value;
        public async Task<string> CreateTempUserAndOtp(PhoneNumberReq phoneNumberReq)
        {
            TempUser newTempUser = _tempUserService.Create(phoneNumberReq.PhoneNumber);
            string newOtpCode = _otpManager.GenerateOtp();
            Otp newOtp = new Otp(phoneNumberReq.PhoneNumber,
                                newOtpCode, 
                                Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]));
            await _tempUserService.AddAsync(newTempUser);
            await _otpService.AddAsync(newOtp);

            // Save all changes
            await _tempUserService.SaveChangesAsync();
            return newOtp.Code;
        }
        public async Task<bool> IsTempUserExistsAsync(string phoneNumber)
        {
            TempUser? tempUser = await _tempUserService.FindByPhoneNumberAsync(phoneNumber);
            return tempUser != null;
        }
        public async Task<bool> IsPhoneNumberConfirmableAsync(string phoneNumber)
        {
            TempUser? tempUser = await _tempUserService.FindByPhoneNumberAsync(phoneNumber);
            if (tempUser == null)
                return false;
            return !tempUser.PhoneNumberConfirmed;
        }

        // OTP && Verfication methods
        async Task<bool> ProcessOtpCodeAsync(string phoneNumber, string otpCode)
        {
            Otp? storedOtp = await _otpService.FindByPhoneNumber(phoneNumber);
            bool isValidOtp = IsValidOtp(otpCode, storedOtp);
            if (isValidOtp)
            {
                storedOtp.IsUsed = true;
                TempUser? storedTempUser = await _tempUserService.FindByPhoneNumberAsync(phoneNumber);
                if (storedTempUser != null)
                    storedTempUser.PhoneNumberConfirmed = true;
                await _tempUserService.SaveChangesAsync();
            }

            return isValidOtp;
        }
        public async Task<bool> ProcessPhoneVerificationRequestAsync(PhoneVerificationRequest phoneVerificationRequest)
        {
            bool isPhoneNumberConfirmable = await IsPhoneNumberConfirmableAsync(phoneVerificationRequest.Phone);
            bool isOtpValid = await ProcessOtpCodeAsync(phoneVerificationRequest.Phone, phoneVerificationRequest.OtpCode);
            Console.WriteLine("Is phone number confirmed: " + isPhoneNumberConfirmable + isOtpValid);
            return isPhoneNumberConfirmable && isOtpValid;
        }   
        bool IsValidOtp(string providedOtp, Otp? storedOtp)
        {
            return storedOtp != null &&
                   DateTime.Now < storedOtp.ExpiresAt &&
                   providedOtp.Equals(storedOtp.Code) &&
                   !storedOtp.IsUsed;
        }
        public async Task<bool> IsOtpExistsAsync(string phoneNumber)
        {
            Otp? otp = await _otpService.FindByPhoneNumber(phoneNumber);
            return otp != null;
        }
        
        public async Task<ValidationResult> CanSendOtpAsync(string phoneNumber)
        {
            // Check if the phone number is already associated with an account
            if (await IsUserExistsAsync(phoneNumber))
                return new ValidationResult("Phone number is already used by an account");
            // Check if an OTP exists for the phone number
            if (await IsOtpExistsAsync(phoneNumber))
                return new ValidationResult("OTP is already sent");
            return new ValidationResult();
        }
        public async Task SendOtpAsync(string otpCode, string phoneNumber) => await _otpManager.SendOTPAsync(otpCode, phoneNumber);
        public async Task<string> GenerateAuthToken(User user, string role)
        {
            return _jwtService.GenerateJWT(user, role ?? string.Empty);
        }
        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var storedUser = await _userService.FindByNormalizedEmailAsync(email);
            return storedUser != null && await _userService.CheckPasswordAsync(storedUser, password);
        }
        
        public async Task<Otp?> FindOtpByPhoneNumber(string phoneNumber) => await _otpService.FindByPhoneNumber(phoneNumber);
         public async Task<string> RecreateOtpCodeAsync(Otp storedOTP)
        {
            string newOtpCode = _otpManager.GenerateOtp();
            await UpdateStoredOtpAsync(storedOTP, newOtpCode);
            return storedOTP.Code;
        }

        public async Task UpdateStoredOtpAsync(Otp storedOTP,string otpCode)
        {
            storedOTP.Code = otpCode;
            storedOTP.CreatedDate = DateTime.Now;
            storedOTP.ExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]));
            await _otpService.SaveChangesAsync();
        }

        // USER Methods
        public long GetUserId(ClaimsIdentity identity) => long.Parse(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
        public async Task<IdentityResult> CreateUserAsync(User user, string password) => await _userService.CreateAsync(user, password);        
        public async Task<User?> FindUserByNormalizedEmailAsync(string normalizedEmail) => await _userService.FindByNormalizedEmailAsync(normalizedEmail);
        public async Task<bool> IsUserExistsByNormalizedEmailAsync(string normalizedEmail) => await _userService.IsExistsByNormalizedEmailAsync(normalizedEmail);
        public async Task<bool> IsUserExistsByPhoneNumberAsync(string phoneNumber) => await _userService.IsExistsByPhoneNumberAsync(phoneNumber);
        
        public async Task<User?> FindUserByEmailAsync(string email)=> await _userService.FindByEmailAsync(email);
        public async Task<bool> IsUserExistsAsync(string phoneNumber) => await _userService.FindByPhoneNumberAsync(phoneNumber) != null;
        public async Task<string> GenerateUserNameFromName(string Name)
        {
            string normalizedName = Name.ToLower().Replace(" ", "");
            string generatedUserName;
            do
            {
                generatedUserName = normalizedName + new Random().Next(999_999);
            } while (await _userService.IsUserNameTakenAsync(generatedUserName));
            return generatedUserName;
        }

        public async Task<User?> FindByPhoneNumberAsync(string phoneNumber) => await _userService.FindByPhoneNumberAsync(phoneNumber);
        // User Roles Methods
        public async Task<string> FindUserRoleAsync(User user) => await _userService.FindUserRoleAsync(user);
        public async Task AssignRoleToUserAsync(User user, string roleName)
        {
            ApplicationRole? storedRole = await _roleService.GetByNameAsync(roleName);;
            await _userService.AddToRoleAsync(user, storedRole.Name);
        }
        public async Task RemoveTempUser(TempUser entity)
        {
            _tempUserService.Remove(entity);
            await _tempUserService.SaveChangesAsync();
        }
    }
}
