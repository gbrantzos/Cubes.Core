using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Cubes.Core.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Core.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator mediator;

        public UsersController(IMediator mediator)
            => this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>
        /// Get users
        /// </summary>
        /// <remarks>
        /// Get all users.
        /// </remarks>
        /// <returns><see cref="IEnumerable{User}"/></returns>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<User>))]
        public async Task<IActionResult> Get()
        {
            var result = await this.mediator.Send(new GetUsers());
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        /// <summary>
        /// Save a user
        /// </summary>
        /// <remarks>
        /// Save a single user or create a new one.
        /// If no password is defined in the request only user details are updated.
        /// </remarks>
        /// <param name="saveUser"></param>
        /// <returns></returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Save([FromBody] SaveUser saveUser)
        {
            var result = await this.mediator.Send(saveUser);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        /// <summary>
        /// Authenticate user
        /// </summary>
        /// <remarks>
        /// Authenticate user with given credentials.
        /// On success user details (including user roles) and  a JWT token are returned.
        /// </remarks>
        /// <param name="authenticateUser"></param>
        /// <returns></returns>
        [HttpPost("authenticate")]
        [Consumes("application/json")]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(AuthenticateUserResponse))]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateUser authenticateUser)
        {
            var result = await this.mediator.Send(authenticateUser);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        /// <remarks>
        /// Reset password for given user.
        /// </remarks>
        /// <param name="resetUserPassword"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        [Consumes("application/json")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetUserPassword resetUserPassword)
        {
            var result = await this.mediator.Send(resetUserPassword);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }
    }
}
