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


namespace AonFreelancing.Services
{
    public class AuthService
    {
        private readonly MainAppContext _mainAppContext;
        private readonly OtpManager _otpManager;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JwtService _jwtService;
        private readonly TempUserService _tempUserService;
        private readonly OTPService _otpService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;

        public AuthService(MainAppContext mainAppContext, OtpManager otpManager, 
                            UserManager<User> userManager, IConfiguration configuration,
                            JwtService jwtService, RoleManager<ApplicationRole> roleManager, 
                            TempUserService tempUserService, OTPService otpService, 
                            UserService userServic, RoleService roleService)
        {
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _userManager = userManager;
            _configuration = configuration;
            _jwtService = jwtService;
            _roleManager = roleManager;
            _tempUserService = tempUserService;
            _otpService = otpService;
            _userService = userServic;
            _roleService = roleService;
        }

        // TempUser Methods
        public async Task<TempUser?> FindTempUserByPhoneNumberAsync(string phoneNumber) => await _tempUserService.GetByPhoneNumberAsync(phoneNumber);        
        public async Task<string> CreateTempUserAndOtp(TempUserDTO tempUserDTO)
        {
            TempUser newTempUser = _tempUserService.Create(tempUserDTO);
            string newOtpCode = _otpManager.GenerateOtp();
            Otp newOtp = _otpService.Create(new OtpInputDTO(tempUserDTO.PhoneNumber,
                                                                    newOtpCode, 
                                                                    Convert.ToInt32(_configuration["Otp:ExpireInMinutes"])));
            await _tempUserService.AddAsync(newTempUser);
            await _otpService.AddAsync(newOtp);

            // Save all changes
            await _tempUserService.SaveChangesAsync();
            return newOtp.Code;
        }
        public async Task<bool> IsTempUserExistsAsync(string phoneNumber)
        {
            TempUser? tempUser = await _tempUserService.GetByPhoneNumberAsync(phoneNumber);
            return tempUser != null;
        }
        public async Task<bool> IsPhoneNumberConfirmableAsync(string phoneNumber)
        {
            TempUser? tempUser = await _tempUserService.GetByPhoneNumberAsync(phoneNumber);
            if (tempUser == null)
                return false;
            return !tempUser.PhoneNumberConfirmed;
        }

        // OTP && Verfication methods
        async Task<bool> ProcessOtpCodeAsync(string phoneNumber, string otpCode)
        {
            Otp? storedOtp = await _otpService.GetByPhoneNumber(phoneNumber);
            bool isValidOtp = IsValidOtp(otpCode, storedOtp);
            if (isValidOtp)
            {
                storedOtp.IsUsed = true;
                TempUser? storedTempUser = await _tempUserService.GetByPhoneNumberAsync(phoneNumber);
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
            Otp? otp = await _otpService.GetByPhoneNumber(phoneNumber);
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
            var storedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
            return storedUser != null && await _userManager.CheckPasswordAsync(storedUser, password);
        }
    
        // USER Methods
        public long GetUserId(ClaimsIdentity identity) => long.Parse(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
        public async Task<IdentityResult> CreateUserAsync(User user, string password) => await _userService.CreateAsync(user, password);        
        public async Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail) => await _userService.GetByNormalizedEmailAsync(normalizedEmail);
        public async Task<bool> IsUserExistsByNormalizedEmailAsync(string normalizedEmail) => await _userService.IsExistsByNormalizedEmailAsync(normalizedEmail);
        public async Task<bool> IsUserExistsByPhoneNumberAsync(string phoneNumber) => await _userService.IsExistsByPhoneNumberAsync(phoneNumber);
        
        public async Task<User?> GetUserByEmailAsync(string email)=> await _userService.GetByEmailAsync(email);
        public async Task<bool> IsUserExistsAsync(string phoneNumber) => await _userService.GetByPhoneNumberAsync(phoneNumber) != null;
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

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber) => await _userService.GetByPhoneNumberAsync(phoneNumber);
        // User Roles Methods
        public async Task<string> GetUserRoleAsync(User user) => await _userService.GetUserRoleAsync(user);
        public async Task AssignRoleToUserAsync(User user, string roleName)
        {
            ApplicationRole? storedRole = await _roleService.GetByNameAsync(roleName);;
            await _userService.AddToRoleAsync(user, storedRole.Name);
        }
        public async Task RemoveEntity(object entity)
        {
            _mainAppContext.Remove(entity);
            await _mainAppContext.SaveChangesAsync();
        }

    }
}
