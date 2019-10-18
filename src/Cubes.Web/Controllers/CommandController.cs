using System;
using System.Linq;
using System.Threading.Tasks;
using Cubes.Core.Commands;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Cubes.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ITypeResolver typeResolver;

        public CommandController(IMediator mediator, ITypeResolver typeResolver)
        {
            this.mediator     = mediator;
            this.typeResolver = typeResolver;
        }

        /// <summary>
        /// Get all available requests
        /// </summary>
        /// <remarks>
        /// List of all available requests with sample empty request instance as JSON.
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Get()
        {
            var commandType = typeof(IRequest<>);
            var commandTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsMediatorRequest())
                .Select(t =>
                {
                    var attribute = t.GetAttribute<DisplayAttribute>();
                    var display = attribute == null || String.IsNullOrEmpty(attribute.Name) ? t.Name : attribute.Name;

                    return new
                    {
                        FullName     = t.FullName,
                        Display      = display,
                        EmptyCommand = Activator.CreateInstance(t)
                    };
                })
                .ToList();
            return Ok(commandTypes);
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <remarks>
        /// Execute given command using and capture results.
        /// </remarks>
        /// <param name="commandType"></param>
        /// <param name="commandInst"></param>
        /// <returns></returns>
        [HttpPost, Route("{commandType}")]
        public async Task<ActionResult> Execute(string commandType, [FromBody]JToken commandInst)
        {
            if (commandInst == null)
                return BadRequest("Body of request cannot be null!");

            var type = typeResolver.GetByName(commandType);
            if (type == null)
                return BadRequest($"Could not resolve type: {commandType}");

            var instance = commandInst.ToObject(type);
            if (instance == null)
                return BadRequest($"Given instance is not a valid JSON object of type '{commandType}'");

            object result = await mediator.Send(instance);
            return Ok(result);
        }
    }
}