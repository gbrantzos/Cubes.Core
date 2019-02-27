using System;
using System.Collections.Generic;
using System.Data;
using Cubes.Core.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cubes.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IQueryExecutor queryExecutor;
        private readonly ILogger<ValuesController> logger;

        public ValuesController(IQueryExecutor queryExecutor, ILogger<ValuesController> logger)
        {
            this.logger = logger;
            this.queryExecutor = queryExecutor;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            logger.LogDebug("From ValuesController");
            logger.LogWarning("From ValuesController");
            return new[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<Fiy>> Get(int id)
        {
            var sqlQuery = new SqlQuery
            {
                Name = "",
                Query = "select * from FIY where FIYINITDATE >= :p_date",
                Parameters = new List<SqlQueryParameter>
                {
                    new SqlQueryParameter
                    {
                        Name = "p_date",
                        DbType = "DateTime"
                    }
                }

            };
            var fiyList = queryExecutor.Query<Fiy>(
                "Local.SEn",
                sqlQuery,
                new Dictionary<string, object> { { "p_date", new DateTime(2014, 1, 1) } },
                new Dictionary<string, string> { { "FIYID", "ID" }, { "FIYINITDATE", "StartDate" }, { "FIYISCURRENT", "Current" } });
            var fiy2List = queryExecutor.Query(
                "Local.SEn",
                sqlQuery,
                new Dictionary<string, object> { { "p_date", new DateTime(2014, 1, 1) } },
                new Dictionary<string, string> { { "FIYID", "ID" }, { "FIYINITDATE", "StartDate" } });

            return Ok(new { List = fiyList, List2 = fiy2List });
        }
        public class Fiy
        {
            public int ID { get; set; }
            public string FiyTitle { get; set; }
            public DateTime StartDate { get; set; }
            public bool Current { get; set; }
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
