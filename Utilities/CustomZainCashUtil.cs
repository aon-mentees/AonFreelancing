using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AonFreelancing.Commons;
using Microsoft.IdentityModel.Tokens;

namespace AonFreelancing.Utilities;

public class CustomZainCashUtil
{
    public static CustomTokenResult DecodeToken(string token, string secret)
    {
        JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
        securityTokenHandler.ReadToken(token);
        SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        TokenValidationParameters validationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = (SecurityKey) symmetricSecurityKey,
            ClockSkew = TimeSpan.Zero
        };
        ClaimsPrincipal claimsPrincipal = securityTokenHandler.ValidateToken(token, validationParameters, out SecurityToken _);
        string str1 = claimsPrincipal.Claims.FirstOrDefault<Claim>((Func<Claim, bool>) (c => c.Type.Equals("id", StringComparison.CurrentCultureIgnoreCase)))?.Value;
        string str2 = claimsPrincipal.Claims.FirstOrDefault<Claim>((Func<Claim, bool>) (c => c.Type.Equals("msg", StringComparison.CurrentCultureIgnoreCase)))?.Value;
        string str3 = claimsPrincipal.Claims.FirstOrDefault<Claim>((Func<Claim, bool>) (c => c.Type.Equals("status", StringComparison.CurrentCultureIgnoreCase)))?.Value;
        string str4 = claimsPrincipal.Claims.FirstOrDefault<Claim>((Func<Claim, bool>) (c => c.Type.Equals("orderId", StringComparison.CurrentCultureIgnoreCase)))?.Value;
        return new CustomTokenResult()
        {
            Id = str1 ?? "",
            Msg = str2 ?? "",
            OrderId = str4 ?? "",
            Status =   str3 ?? ""
        };
    }
}