using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.AspNetCore.Identity;

namespace AonFreelancing.Services;

public class RoleService : MainDbService
{
    private readonly RoleManager<ApplicationRole>_roleManager;
    public RoleService(RoleManager<ApplicationRole> roleManager, MainAppContext mainAppContext) : base(mainAppContext)
    {
        _roleManager = roleManager;
    }

    public async Task<ApplicationRole?> GetByNameAsync(string name) => await _roleManager.FindByNameAsync(name);        
}