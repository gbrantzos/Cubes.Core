using System;
using System.Threading.Tasks;
using Cubes.Core.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Core.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator mediator;

        public RolesController(IMediator mediator)
            => this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await this.mediator.Send(new GetRoles());
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveRoles saveRoles)
        {
            var result = await this.mediator.Send(saveRoles);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }
    }
}
