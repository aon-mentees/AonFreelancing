using System.ComponentModel.DataAnnotations;
using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using Microsoft.AspNetCore.Identity;
namespace AonFreelancing.Services;


public class UserService : MainDbService
{
    private readonly MainAppContext _mainAppContext;
    private readonly UserManager<User> _userManager;
    public UserService(MainAppContext mainAppContext, UserManager<User> userManager): base(mainAppContext)
    {
        _mainAppContext = mainAppContext;
        _userManager = userManager;
    }

    public async Task<User?> GetByIdAsync(int id) => await _mainAppContext.Users.FindAsync(id);
    public async Task<IEnumerable<User>> GetAllAsync() => await _mainAppContext.Users.ToListAsync();
    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber) => await _mainAppContext.Users.Where(tu => tu.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
    public User Create(UserRegistrationRequest request) => new User(request);
    public async Task<User?> GetByNormalizedEmailAsync(string normalizedEmail) => await _mainAppContext.Users.Where(u => u.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync();
    public async Task<bool> IsExistsByNormalizedEmailAsync(string normalizedEmail) => await _mainAppContext.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail);
    public async Task<bool> IsExistsByPhoneNumberAsync(string phoneNumber) => await _mainAppContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
    
    public async Task<User?> GetByEmailAsync(string email) => await _mainAppContext.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
    public async Task<bool> IsUserNameTakenAsync(string newUserName) => await _mainAppContext.Users.AnyAsync(u => u.UserName == newUserName);


    // public async Task<ApplicationRole?> GetRoleByNameAsync(string name)  await _roleService.GetByNameAsync(name);

    public async Task<string> GetUserRoleAsync(User user)=> (await _userManager.GetRolesAsync(user)).First();
    public async Task<IdentityResult> CreateAsync(User user, string password) => await _userManager.CreateAsync(user, password);
    public async Task AddToRoleAsync(User user, string storedRole)=> await _userManager.AddToRoleAsync(user, storedRole);
    public async Task<bool> CheckPasswordAsync(User storedUser, string password) => await _userManager.CheckPasswordAsync(storedUser, password);
}