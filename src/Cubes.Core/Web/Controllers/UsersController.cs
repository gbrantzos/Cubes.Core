using System;
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await this.mediator.Send(new GetUsers());
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveUser saveUser)
        {
            var result = await this.mediator.Send(saveUser);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateUser authenticateUser)
        {
            var result = await this.mediator.Send(authenticateUser);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetUserPassword resetUserPassword)
        {
            var result = await this.mediator.Send(resetUserPassword);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }
    }
}
