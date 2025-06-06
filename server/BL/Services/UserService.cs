﻿using BL.InterfaceServices;
using DL;
using DL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services
{
    public class UserService : IUserService
    {

        private readonly IConfiguration _configuration;
        private readonly IDataContext _dataContext;

        public UserService(IDataContext dataContex, IConfiguration configuration)
        {
            _dataContext = dataContex;
            _configuration = configuration;
        }

        public async Task<string> GenerateJwtTokenAsync(string username, string[] roles)
        {

            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            // הוספת תפקידים כ-Claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dataContext.Users.ToListAsync();
        }
        public async Task<User> GetUserByIdAsync(int id)
        {
            //BabyValidation.ValidateBabyId(id);
            return await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task AddUserAsync(User user)
        {
            //BabyValidation.ValidateBabyId(baby.Id);
            //BabyValidation.ValidateBabyName(baby.Name);
            {
                _dataContext.Users.Add(user);
                await _dataContext.SaveChangesAsync();
            }

        }
        public async Task UpdateUserAsync(int id, User user)
        {
            //BabyValidation.ValidateBabyId(baby.Id);
            //BabyValidation.ValidateBabyId(id);
            //BabyValidation.ValidateBabyName(baby.Name);
            var newUser = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (newUser != null)
            {
                newUser.Name = user.Name;
                newUser.Email = user.Email;
                //newUser.Password = user.Password;
                newUser.Role = user.Role;
                newUser.UpdatedAt = DateTime.Now;
                newUser.UpdatedBy = user.UpdatedBy;
                await _dataContext.SaveChangesAsync();
            }

        }
        public async Task RemoveUserAsync(int id)
        {
            var userToDelete = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userToDelete != null)
            {
                _dataContext.Users.Remove(userToDelete);
                await _dataContext.SaveChangesAsync();
            }

        }

        public async Task<User> LoginAsync(string email, string password)
        {
            User user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            return user;
        }
    }
}
