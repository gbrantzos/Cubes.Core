using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Cubes.Web.Filters
{
    public class ValidateModelFilterAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelFilterAttribute> logger;

        public ValidateModelFilterAttribute(ILogger<ValidateModelFilterAttribute> logger)
        {
            this.logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                var message = $"Model validation failed: Controller '{actionContext.Controller.GetType().Name}', action '{actionContext.ActionDescriptor.DisplayName}'";
                var modelErrors = actionContext
                    .ModelState
                    .Values
                    .SelectMany(i => i.Errors.Select(e => String.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage))
                    .Where(i => !String.IsNullOrEmpty(i))
                    .ToList();

                logger.LogWarning($"Request {actionContext.HttpContext.Request.Path}, method {actionContext.HttpContext.Request.Method}");
                logger.LogWarning(message);

                if (modelErrors.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var err in modelErrors)
                        sb.AppendLine(err);
                    logger.LogWarning(sb.ToString());
                }

                actionContext.Result = new BadRequestObjectResult(actionContext.ModelState);
            }
        }
    }
}
