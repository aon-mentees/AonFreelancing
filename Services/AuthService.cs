using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Requests;
using AonFreelancing.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace AonFreelancing.Services
{
    public class AuthService
    {
        private readonly MainAppContext _mainAppContext;
        private readonly OtpManager _otpManager;
        public AuthService(MainAppContext mainAppContext, OtpManager otpManager) {
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
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
                if(storedTempUser != null)
                    storedTempUser.PhoneNumberConfirmed = true;
                await _mainAppContext.SaveChangesAsync();
            }

            return isValidOtp;
        }

        bool IsValidOtp(string providedOtp, Otp? storedOtp) => storedOtp != null &&
                                                                DateTime.Now < storedOtp.ExpiresAt &&
                                                                providedOtp.Equals(storedOtp.Code) &&
                                                                !storedOtp.IsUsed;

        public async Task SaveTempUserAndOtpAsync(TempUser newTempUser, Otp newOtp)
        {
            await _mainAppContext.TempUsers.AddAsync(newTempUser);
            await _mainAppContext.Otps.AddAsync(newOtp);
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task<bool> IsUserExistsInTempAsync(PhoneNumberReq phoneNumberReq)
        {
            return await _mainAppContext.TempUsers
              .AnyAsync(u => u.PhoneNumber == phoneNumberReq.PhoneNumber);
        }

        public async Task AddAsync(TempUser tempUser)
        {
            await _mainAppContext.TempUsers.AddAsync(tempUser);
            await _mainAppContext.SaveChangesAsync();

        }
        public async Task AddOtpAsync(Otp oTP)
        {
            await _mainAppContext.Otps.AddAsync(oTP);
            await _mainAppContext.SaveChangesAsync();
        }

        public async Task Remove(object item)
        {
            _mainAppContext.Remove(item);
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task<TempUser> GetTempUserAsync(string PhoneNumber)
        {
            return await _mainAppContext.TempUsers.FirstOrDefaultAsync(u=>u.PhoneNumber == PhoneNumber);
        }
         async Task<bool> IsPhoneNumberConfirmableAsync(string PhonerNumber) => await _mainAppContext.TempUsers.AnyAsync(tu => tu.PhoneNumber == PhonerNumber && !tu.PhoneNumberConfirmed);


        public async Task<Otp> GetOTPAsync(string Phone)
        {
            return await _mainAppContext.Otps.Where(o => o.PhoneNumber == Phone)
                   .FirstOrDefaultAsync();

        }

        public async Task UpdateTempUser(string PhoneNumber)
        {
            var TempUser = await _mainAppContext.TempUsers
              .FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber);
            if (TempUser != null) { 
                TempUser.PhoneNumberConfirmed = true;
                await _mainAppContext.SaveChangesAsync();
            }
        }

        public async Task UpdateOtpAsync(Otp otp)
        {

            otp.IsUsed = true;// Update Otp
            await _mainAppContext.SaveChangesAsync();

        }

    }
}
