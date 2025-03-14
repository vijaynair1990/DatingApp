using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context):BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<AppUser>> Register(RegisterDTO registerdto)
    {
        if(await UserExists(registerdto.username))
        return BadRequest("UserName already Exists");

        using var hmac=new HMACSHA512();

        var user=new AppUser
        {
                Username=registerdto.username.ToLower(),
                PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerdto.password)),
                PasswordSalt=hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AppUser>> Login(LoginDTO logindto)
    {
        var user=await context.Users.FirstOrDefaultAsync(x=>x.Username==logindto.username.ToLower());

        if(user==null) return Unauthorized("Invalid Username");

        using var hmac=new HMACSHA512(user.PasswordSalt);

        var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(logindto.password));

        for (int i = 0; i < computedHash.Length; i++)
        {
           if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("Invalid Password"); 
        }

        return user;
    }


    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(x =>x.Username.ToLower()==username.ToLower());
    }
}
