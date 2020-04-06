using System;
using System.Linq;
using System.Threading.Tasks;
using Cubes.Core.Commands;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Cubes.Core.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ITypeResolver typeResolver;

        public RequestsController(IMediator mediator, ITypeResolver typeResolver)
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
        /// Execute request
        /// </summary>
        /// <remarks>
        /// Execute given request using body as instance and capture results.
        /// </remarks>
        /// <param name="requestType"></param>
        /// <param name="requestInst"></param>
        /// <returns></returns>
        [HttpPost, Route("{requestType}")]
        public async Task<ActionResult> Execute(string requestType, [FromBody]JToken requestInst)
        {
            if (requestInst == null)
                return BadRequest("Body of request cannot be null!");

            var type = typeResolver.GetByName(requestType);
            if (type == null)
                return BadRequest($"Could not resolve type: {requestType}");

            var instance = requestInst.ToObject(type);
            if (instance == null)
                return BadRequest($"Given instance is not a valid JSON object of type '{requestType}'");

            // TODO Check this
            IResult result = await mediator.Send(instance) as IResult;
            if (result.ExceptionThrown != null)
                throw new Exception($"Request execution failed!{Environment.NewLine}{requestInst}", result.ExceptionThrown);
            if (result.HasErrors)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}