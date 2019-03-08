using System;
using System.Linq;
using Cubes.Core.Commands;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Cubes.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly ICommandBus commandBus;
        private readonly ITypeResolver typeResolver;

        public CommandController(ICommandBus commandBus, ITypeResolver typeResolver)
        {
            this.commandBus = commandBus;
            this.typeResolver = typeResolver;
        }

        /// <summary>
        /// Get all available commands
        /// </summary>
        /// <remarks>
        /// List of all available commands with sample empty command instance as JSON.
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Get()
        {
            var commandType = typeof(ICommand<>);
            var commandTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsCommand())
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
        public ActionResult Execute(string commandType, [FromBody]JToken commandInst)
        {
            if (commandInst == null)
                return BadRequest("Body of request cannot be null!");

            var type = typeResolver.GetByName(commandType);
            if (type == null)
                return BadRequest($"Could not resolve type: {commandType}");

            var instance = commandInst.ToObject(type);
            if (instance == null)
                return BadRequest($"Given instance is not a valid JSON object of type '{commandType}'");

            var result = commandBus.Submit(instance);
            return Ok(result);
        }
    }
}