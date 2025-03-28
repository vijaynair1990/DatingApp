using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ErrorHandlerController(DataContext context) : BaseApiController
{
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetAuthError()
    {
        return "Secrets";
    }

    [HttpGet("no-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var data = context.Users.Find(-1);
        if (data == null) return NotFound();

        return data;
    }


    [HttpGet("server-error")]
    public ActionResult<AppUser> GetServerError()
    {

        var data = context.Users.Find(-1) ?? throw new Exception("Null reference error");

        return data;

    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequestError()
    {
        return BadRequest("Bad request");
    }
}
