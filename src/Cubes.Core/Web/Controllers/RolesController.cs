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
    public class RolesController : ControllerBase
    {
        private readonly IMediator mediator;

        public RolesController(IMediator mediator)
            => this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>
        /// Get roles
        /// </summary>
        /// <remarks>
        /// Get all available roles, including system roles.
        /// </remarks>
        /// <returns><see cref="IEnumerable{Role}"/></returns>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<Role>))]
        public async Task<IActionResult> Get()
        {
            var result = await this.mediator.Send(new GetRoles());
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }

        /// <summary>
        /// Save roles
        /// </summary>
        /// <remarks>Saves an array of roles.</remarks>
        /// <param name="saveRoles">The roles to save</param>
        /// <returns></returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Save([FromBody] SaveRoles saveRoles)
        {
            var result = await this.mediator.Send(saveRoles);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result.Response);
        }
    }
}
