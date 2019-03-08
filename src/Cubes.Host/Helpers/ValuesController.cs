using System.Collections.Generic;
using Cubes.Api.RequestContext;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Host.Helpers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IContextProvider contextProvider;

        public ValuesController(IContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new[] { "value1", "value2", contextProvider.Current.ID, contextProvider.Current.Url, contextProvider.Current.IP, contextProvider.Current.QueryString };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }
        
        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
