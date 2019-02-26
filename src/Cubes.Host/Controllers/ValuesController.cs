using System.Collections.Generic;
using Cubes.Core.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IQueryExecutor queryExecutor;

        public ValuesController(IQueryExecutor queryExecutor)
        {
            this.queryExecutor = queryExecutor;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<Fiy>> Get(int id)
        {
            var sqlQuery = new SqlQuery
            {
                Name = "",
                Query = "select * from FIY"
            };
            var fiyList = queryExecutor.Query<Fiy>("Local.SEn", sqlQuery, null, new Dictionary<string, string> { { "ID", "FIYID" } });

            return Ok(new { List = fiyList });
        }
        public class Fiy
        {
            public int ID { get; set; }
            public string FiyTitle { get; set; }
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
