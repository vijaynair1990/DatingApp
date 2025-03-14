using System;

namespace API.DTOs;

public class UserDTO
{
    public required string username { get; set; }
    public required string Token { get; set; }
}
