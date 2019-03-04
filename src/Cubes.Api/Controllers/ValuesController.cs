using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Cubes.Core.Commands;
using Cubes.Core.Commands.Basic;
using Cubes.Core.DataAccess;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;

namespace Cubes.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IQueryExecutor queryExecutor;
        private readonly ILogger<ValuesController> logger;
        private readonly ISettingsProvider settings;
        private readonly ICommandBus bus;

        public ValuesController(IQueryExecutor queryExecutor, ILogger<ValuesController> logger, ISettingsProvider settings, ICommandBus bus)
        {
            this.bus = bus;
            this.logger = logger;
            this.settings = settings;
            this.queryExecutor = queryExecutor;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            logger.LogDebug("From ValuesController");
            logger.LogWarning("From ValuesController");
            logger.LogInformation(settings.Load<SampleSettings>().Description);

            //var cmd = new RunOsProcessCommand
            //{
            //    Command = "git",
            //    Arguments = "log -10",
            //    StartIn = "/Users/georgebrantzos/Projects/ibe"
            //};
            //var res = bus.Submit(cmd);

            return new[] { "value1", "value2"};
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

    public class SampleSettings
    {
        public int ID { get; set; }
        public String Description { get; set; }
    }
}
