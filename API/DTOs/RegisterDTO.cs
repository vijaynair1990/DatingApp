using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDTO
{
    [Required]
    public string username { get; set; }=string.Empty;

    [Required]
    public string? knownas { get; set; }
    [Required]
    public string? gender { get; set; }
    [Required]
    public string? dateofbirth { get; set; }
    [Required]
    public string? city { get; set; }
    [Required]
    public string? country { get; set; }
    [Required]
    [StringLength(8,MinimumLength =4)]
    public string password { get; set; }=string.Empty;
}
