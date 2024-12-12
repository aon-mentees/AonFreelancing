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
        
        public async Task<IEnumerable<Otp>> FindAllAsync() => await _mainAppContext.Otps.ToListAsync();
        public async Task<Otp?> FindByIdAsync(int id) => await _mainAppContext.Otps.FindAsync(id);
        public async Task<Otp?> FindByPhoneNumber(string phoneNumber) => await _mainAppContext.Otps.FirstOrDefaultAsync(o => o.PhoneNumber == phoneNumber);
        public void Update(Otp storedOtp) => _mainAppContext.Otps.Update(storedOtp);
    }
}