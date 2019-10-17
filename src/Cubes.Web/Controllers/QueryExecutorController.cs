//using System;
//using System.Linq;
//using Cubes.Core.DataAccess;
//using Microsoft.AspNetCore.Mvc;

//namespace Cubes.Web.Controllers
//{
//    [ApiController, Route("api/[controller]")]
//    public class QueryExecutorController : ControllerBase
//    {
//        private readonly IQueryExecutor queryExecutor;

//        public QueryExecutorController(IQueryExecutor queryExecutor)
//            => this.queryExecutor = queryExecutor;

//        /// <summary>
//        /// Execute known query
//        /// </summary>
//        /// <remarks>
//        /// Execute query stored on DataAccessSettings file using a known database connection.
//        /// Query string contains values for query parameters.
//        /// </remarks>
//        /// <param name="connection">Connection name</param>
//        /// <param name="query">Query name</param>
//        /// <returns></returns>
//        [HttpGet("{connection}/{query}")]
//        public ActionResult ExecuteQuery(string connection, string query)
//        {
//            var parameters = Request
//                .Query
//                .ToDictionary(q => q.Key, q => (Object)q.Value.ToString());

//            var results = queryExecutor.Query(connection, query, parameters);
//            return Ok(results);
//        }

//        /// <summary>
//        /// Execute query provided
//        /// </summary>
//        /// <remarks>
//        /// Execute query provided on request body using a known database connection.
//        /// Query string contains values for query parameters.
//        /// /// </remarks>
//        /// <param name="connection"></param>
//        /// <param name="query"></param>
//        /// <returns></returns>
//        [HttpPost("{connection}")]
//        public ActionResult ExecuteQuery(string connection, [FromBody]Query query)
//        {
//            var parameters = Request
//                .Query
//                .ToDictionary(q => q.Key, q => (Object)q.Value.ToString());

//            var results = queryExecutor.Query(connection, query, parameters);
//            return Ok(results);
//        }
//    }
//}


//    /*
//        var sqlQuery = new SqlQuery
//        {
//            Name = "",
//            Query = "select * from FIY where FIYINITDATE >= :p_date",
//            Parameters = new List<SqlQueryParameter>
//            {
//                new SqlQueryParameter
//                {
//                    Name = "p_date",
//                    DbType = "DateTime"
//                }
//            }

//        };
//        var fiyList = queryExecutor.Query<Fiy>(
//            "Local.SEn",
//            sqlQuery,
//            new Dictionary<string, object> { { "p_date", new DateTime(2014, 1, 1) } },
//            new Dictionary<string, string> { { "FIYID", "ID" }, { "FIYINITDATE", "StartDate" }, { "FIYISCURRENT", "Current" } });
//        var fiy2List = queryExecutor.Query(
//            "Local.SEn",
//            sqlQuery,
//            new Dictionary<string, object> { { "p_date", new DateTime(2014, 1, 1) } },
//            new Dictionary<string, string> { { "FIYID", "ID" }, { "FIYINITDATE", "StartDate" } });

//     */
