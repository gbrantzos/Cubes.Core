using System;
using System.Linq;
using Cubes.Core.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class QueryExecutorController : ControllerBase
    {
        private readonly IQueryExecutor queryExecutor;

        public QueryExecutorController(IQueryExecutor queryExecutor)
            => this.queryExecutor = queryExecutor;

        [HttpGet("{connection}/{query}")]
        public ActionResult ExecuteQuery(string connection, string query)
        {
            var parameters = Request
                .Query
                .ToDictionary(q => q.Key, q => (Object)q.Value.ToString());

            var results = queryExecutor.Query(connection, query, parameters);
            return Ok(results);
        }

        [HttpPost("{connection}")]
        public ActionResult ExecuteQuery(string connection, [FromBody]SqlQuery query)
        {
            var parameters = Request
                .Query
                .ToDictionary(q => q.Key, q => (Object)q.Value.ToString());

            var results = queryExecutor.Query(connection, query, parameters);
            return Ok(results);
        }
    }
}


    /*
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

     */
