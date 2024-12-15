using AonFreelancing.Utilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AonFreelancing.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJWT(Models.User user, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:
                [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(Constants.JWT_ROLE, role),
                new Claim(Constants.JWT_GIVEN_NAME, user.Name),
                new Claim(Constants.JWT_EMAIL, user.Email),
                new Claim(Constants.JWT_PHONE_NUMBER, user.PhoneNumber)
                ],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
