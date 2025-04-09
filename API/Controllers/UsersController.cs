using System;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
{

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();
        return Ok(users);
    }


    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUserById(string username)
    {
        var user = await userRepository.GetMemberAsync(username);

        if (user == null) return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
        var username = User.GetUsername();

        var user = await userRepository.GetUserByUsernameAsync(username);
        if (user == null) return BadRequest("No User found");

        mapper.Map(memberUpdateDTO, user);

        if (await userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        var username = User.GetUsername();
        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null) return BadRequest("Cannot update user");

        var result = await photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        user.Photos.Add(photo);

        if (await userRepository.SaveAllAsync())
            //return mapper.Map<PhotoDTO>(photo);

            return CreatedAtAction(nameof(GetUserById), new { username = user.Username }, mapper.Map<PhotoDTO>(photo));

        return BadRequest("Problem adding photos");



    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var username = User.GetUsername();
        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null) return BadRequest("Cannot find user");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;

        photo.IsMain = true;
        if (await userRepository.SaveAllAsync())
            return NoContent();

        return BadRequest("Problem setting main photos");

    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var username = User.GetUsername();
        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null) return BadRequest("Cannot find user");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.IsMain) return BadRequest("Main photo can't be deleted!!!");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
        }

        user.Photos.Remove(photo);
        if (await userRepository.SaveAllAsync())
            return Ok();

        return BadRequest("Problem deleting photos");

    }



}
