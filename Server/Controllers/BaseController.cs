using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApi.Controllers
{

    [Authorize()]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected Guid GetUserObjectId() => new Guid(User.FindFirst("oid").Value);
        protected string GetUserName() => User.FindFirst("name").Value;
        protected string GetUserEmail() => User.FindFirst("preferred_username").Value?.ToLowerInvariant();
    }
}
