using AonFreelancing.Contexts;
using AonFreelancing.Interfaces;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace AonFreelancing.Services
{
    public class OTPService : MainDbService
    {
        public OTPService(MainAppContext mainAppContext) : base(mainAppContext){}
        
        public async Task<IEnumerable<Otp>> GetAllAsync() => await _mainAppContext.Otps.ToListAsync();
        public async Task<Otp?> GetByIdAsync(int id) => await _mainAppContext.Otps.FindAsync(id);
        public async Task<Otp?> GetByPhoneNumber(string phoneNumber) => await _mainAppContext.Otps.FirstOrDefaultAsync(o => o.PhoneNumber == phoneNumber);
        public Otp Create(OtpInputDTO otpDTO) => new Otp(otpDTO);
        public void Update(Otp storedOtp) => _mainAppContext.Otps.Update(storedOtp);
    }
}