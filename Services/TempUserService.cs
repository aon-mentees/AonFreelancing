using AonFreelancing.Commons;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Requests;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using AonFreelancing.Interfaces;
using Microsoft.AspNetCore.Routing.Tree;
using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Services
{
    public class TempUserService : MainDbService
    {
        public TempUserService(MainAppContext mainAppContext) : base(mainAppContext){}
        
        public async Task<TempUser?> GetByIdAsync(int id) => await _mainAppContext.TempUsers.FindAsync(id);
        public async Task<IEnumerable<TempUser>> GetAllAsync() => await _mainAppContext.TempUsers.ToListAsync();
        public async Task<TempUser?> FindByPhoneNumberAsync(string phoneNumber) => await _mainAppContext.TempUsers.Where(tu => tu.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
        public TempUser Create(TempUserDTO tempUserDTO) => new TempUser(tempUserDTO.PhoneNumber);
    }
}
