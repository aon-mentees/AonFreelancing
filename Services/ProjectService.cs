using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Requests;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AonFreelancing.Services
{
    public class ProjectService
    {
        private readonly MainAppContext _mainAppContext;
        private readonly OtpManager _otpManager;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JwtService _jwtService;
        public ProjectService(MainAppContext mainAppContext, OtpManager otpManager, UserManager<User> userManager, IConfiguration configuration, JwtService jwtService, RoleManager<ApplicationRole> roleManager)
        {
            _mainAppContext = mainAppContext;
            _otpManager = otpManager;
            _userManager = userManager;
            _configuration = configuration;
            _jwtService = jwtService;
            _roleManager = roleManager;
        }
      

    }
}
