using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService(IConfiguration config) : ITokenService
    {
        public string CreateToken(AppUser user)
        {
            var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot access token key from appsettings");
            if (tokenKey.Length < 64) throw new Exception("Your token key needs to be longer");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            //claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName)
            };
            var cerds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokensDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials=cerds
            };

            var tokenHandler=new JwtSecurityTokenHandler();
            var token=tokenHandler.CreateToken(tokensDescriptor);

            return tokenHandler.WriteToken(token);
             
        }
    }
}
