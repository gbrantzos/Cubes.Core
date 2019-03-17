using System.Collections.Generic;
using Cubes.Api.RequestContext;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Host.Helpers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public class Item
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public System.DateTime CreatedAt { get; set; }
        }
        private readonly IContextProvider contextProvider;

        public ValuesController(IContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var aa = new Dictionary<string, Item>
            {
                { "Item 1", new Item{ ID = 1, Name = "Item1" ,CreatedAt = System.DateTime.Now.AddYears(-12) } },
                { "Item 2", new Item{ ID = 2, Name = "Item2" ,CreatedAt = System.DateTime.Now.AddYears(-2) } },
                { "Item 3", new Item{ ID = 3, Name = "Item3" ,CreatedAt = System.DateTime.Now.AddYears(2) } }
            };
            var ss = new YamlDotNet.Serialization.Serializer().Serialize(new { Items = aa, Count = aa.Count });
            return new[] { "value1", "value2", contextProvider.Current.ID, contextProvider.Current.Url, contextProvider.Current.IP, contextProvider.Current.QueryString, ss };
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
