using AonFreelancing.Commons;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Requests;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using static System.Net.WebRequestMethods;


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
        public AuthService(MainAppContext mainAppContext, OtpManager otpManager, UserManager<User> userManager, IConfiguration configuration, JwtService jwtService, RoleManager<ApplicationRole> roleManager)
        {
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _userManager = userManager;
            _configuration = configuration;
            _jwtService = jwtService;
            _roleManager = roleManager;
        }

        public long GetUserId(ClaimsIdentity identity) => long.Parse(identity.FindFirst(ClaimTypes.NameIdentifier).Value);

        public async Task<bool> ProcessPhoneVerificationRequestAsync(PhoneVerificationRequest phoneVerificationRequest)
        {
            bool isPhoneNumberConfirmable = await IsPhoneNumberConfirmableAsync(phoneVerificationRequest.Phone);
            bool isOtpValid = await ProcessOtpCodeAsync(phoneVerificationRequest.Phone, phoneVerificationRequest.OtpCode);
            return isPhoneNumberConfirmable && isOtpValid;
        }

        async Task<bool> ProcessOtpCodeAsync(string phoneNumber, string otpCode)
        {
            Otp? storedOtp = await _mainAppContext.Otps.FirstOrDefaultAsync(o => o.PhoneNumber == phoneNumber);
            bool isValidOtp = IsValidOtp(otpCode, storedOtp);
            if (isValidOtp)
            {
                storedOtp.IsUsed = true;
                TempUser? storedTempUser = await _mainAppContext.TempUsers.FirstOrDefaultAsync(tu => tu.PhoneNumber == phoneNumber);
                if (storedTempUser != null)
                    storedTempUser.PhoneNumberConfirmed = true;
                await _mainAppContext.SaveChangesAsync();
            }

            return isValidOtp;
        }

        bool IsValidOtp(string providedOtp, Otp? storedOtp)
        {
            return storedOtp != null &&
                   DateTime.Now < storedOtp.ExpiresAt &&
                   providedOtp.Equals(storedOtp.Code) &&
                   !storedOtp.IsUsed;
        }
        public async Task<User?> FindUserByNormalizedEmailAsync(string normalizedEmail)
        {
            return await _mainAppContext.Users.Where(u => u.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync();
        }
        public async Task<TempUser?> FindTempUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _mainAppContext.TempUsers.Where(tu => tu.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
        }
        public async Task SaveTempUserAndOtpAsync(TempUser newTempUser, Otp newOtp)
        {
            await _mainAppContext.TempUsers.AddAsync(newTempUser);
            await _mainAppContext.Otps.AddAsync(newOtp);
            await _mainAppContext.SaveChangesAsync();
        }
       
        public async Task<bool> IsUserExistsAsync(string phoneNumber)
        {
            return await _mainAppContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }
        public async Task<bool> IsTempUserExistsAsync(string phoneNumber)
        {
            return await _mainAppContext.TempUsers.AnyAsync(tu => tu.PhoneNumber == phoneNumber);
        }
        public async Task<bool> IsOtpExistsAsync(string phoneNumber)
        {
            return await _mainAppContext.Otps.AnyAsync(o => o.PhoneNumber == phoneNumber);
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
        public async Task<string> CreateTempUserAndOtp(string phoneNumber)
        {
            string newOtpCode = _otpManager.GenerateOtp();
            TempUser newTempUser = new TempUser(phoneNumber);
            Otp newOtp = new Otp(phoneNumber, newOtpCode, Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]));

            await SaveTempUserAndOtpAsync(newTempUser, newOtp);
            return newOtp.Code;
        }
        public async Task SendOtpAsync(string otpCode, string phoneNumber)
        {
            await _otpManager.SendOTPAsync(otpCode, phoneNumber);
        }
        public async Task<ApplicationRole?> FindRoleByNameAsync(string name)
        {
            return await _roleManager.FindByNameAsync(name);
        }
        public async Task<Otp> FindOtpAsync(string phoneNumber)
        {
            return await _mainAppContext.Otps.FirstOrDefaultAsync(o => o.PhoneNumber == phoneNumber);
        }
        public async Task<string> RecreateOtpCodeAsync(Otp storedOTP)
        {
            string newOtpCode = _otpManager.GenerateOtp();
            await UpdateStoredOtpAsync(storedOTP, newOtpCode);
            return storedOTP.Code;
        }
        public async Task UpdateStoredOtpAsync(Otp newOtp,string otpCode)
        {
            newOtp.Code = otpCode;
            newOtp.CreatedDate = DateTime.Now;
            newOtp.ExpiresAt = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Otp:ExpireInMinutes"]));
            await SaveRecreatedOtpAsync();
        }
        public async Task SaveRecreatedOtpAsync()
        {
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task<string> FindUserRoleAsync(User user)
        {
            return (await _userManager.GetRolesAsync(user)).First();
        }
        public async Task<string> GenerateAuthToken(User user, string role)
        {
            return _jwtService.GenerateJWT(user, role ?? string.Empty);
        }
        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var storedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
            return storedUser != null && await _userManager.CheckPasswordAsync(storedUser, password);
        }
        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _mainAppContext.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
        }
        public async Task AssignRoleToUserAsync(User user, string roleName)
        {
            ApplicationRole? storedRole = await FindRoleByNameAsync(roleName);
            await _userManager.AddToRoleAsync(user, storedRole.Name);
        }
        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }
        public async Task<string> GenerateUserNameFromName(string Name)
        {
            string normalizedName = Name.ToLower().Replace(" ", "");
            string generatedUserName;
            do
            {
                generatedUserName = normalizedName + new Random().Next(999_999);
            } while (await _mainAppContext.Users.AnyAsync(u => u.UserName == generatedUserName));
            return generatedUserName;
        }

        public async Task RemoveEntity(object entity)
        {
            _mainAppContext.Remove(entity);
            await _mainAppContext.SaveChangesAsync();
        }

        async Task<bool> IsPhoneNumberConfirmableAsync(string PhonerNumber)
        {
            return await _mainAppContext.TempUsers.AnyAsync(tu => tu.PhoneNumber == PhonerNumber && !tu.PhoneNumberConfirmed);
        }


    }
}
