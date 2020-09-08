using System;
using Cubes.Core.Base;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Web.ResponseWrapping
{
    public interface IApiResponseBuilder
    {
        ApiResponse Create();
    }

    public class ApiResponseBuilder : IApiResponseBuilder
    {
        private readonly IContextProvider contextProvider;
        private readonly CubesConfiguration cubesConfig;

        public ApiResponseBuilder(IOptions<CubesConfiguration> options, IContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
            this.cubesConfig = options.Value;
        }

        public ApiResponse Create() => new ApiResponse(DateTime.UtcNow, cubesConfig.Version, contextProvider.Current.ID);
    }
}
