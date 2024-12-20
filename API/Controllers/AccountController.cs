using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController(DataContext dataContext,ITokenService tokenService, IMapper mapper): BaseApiController
    {
        [HttpPost("register")] //account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username))
                return BadRequest("UserName is already exists");

            using var hmac=new HMACSHA512();

            var user =mapper.Map<AppUser>(registerDto);

            user.UserName= registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
            
            dataContext.Users.Add(user);
            await dataContext.SaveChangesAsync();

            return Ok(new UserDto
            {
               UserName=user.UserName,
               Token=tokenService.CreateToken(user),
               KnownAs=user.KnownAs,
               Gender = user.Gender
            });
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await dataContext.Users
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(x =>
                x.UserName.ToLower() == loginDto.Username.ToLower());
            
            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i < computedHash.Length; i++) 
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Invalid Password");
            }
            return Ok(new UserDto 
            {
                UserName=user.UserName, 
                KnownAs=user.KnownAs,
                Token=tokenService.CreateToken(user),
                Gender=user.Gender,
                PhotoUrl=user.Photos.FirstOrDefault(x=>x.IsMain)?.Url
            });
        }
        
        private async Task<bool> UserExists(string username)
        {
            return await dataContext.Users.AnyAsync(x=>x.UserName.ToLower() == username.ToLower());
        }
    }
}
