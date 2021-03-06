using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Cubes.Core.DataAccess;
using Cubes.Core.Utilities;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly IQueryManager queryManager;
        private readonly IConnectionManager connectionManager;
        private readonly DataAccessOptions settings;
        private readonly ILocalStorage localStorage;
        private readonly IDefaultQueries defaultQueries;

        public DataController(IConnectionManager connectionManager,
            IQueryManager queryManager,
            IOptionsSnapshot<DataAccessOptions> options,
            ILocalStorage localStorage,
            IDefaultQueries defaultQueries)
        {
            this.queryManager        = queryManager;
            this.connectionManager   = connectionManager;
            this.settings            = options.Value;
            this.localStorage        = localStorage;
            this.defaultQueries      = defaultQueries;
        }

        /// <summary>
        /// Get Connections
        /// </summary>
        /// <remarks>
        /// Get all <see cref="Connection"/> objects defined in the DataAccessSettings file.
        /// </remarks>
        /// <returns><see cref="IEnumerable{Connection}"/></returns>
        [HttpGet("connections")]
        public IEnumerable<Connection> GetConnections() => this.settings.Connections;

        /// <summary>
        /// Test connection
        /// </summary>
        /// <remarks>
        /// Test if <see cref="Connection"/> defined in request body can be used by calling OpenAsync method.
        /// </remarks>
        /// <param name="connection">Connection name</param>
        /// <returns></returns>
        [HttpPost("connections/test")]
        public async Task<IActionResult> TestConnection([FromBody] Connection connection)
        {
            try
            {
                using var cnx = this.connectionManager.GetConnection(connection);
                await cnx.OpenAsync();
                await cnx.CloseAsync();

                return Ok("Connection was successful!");
            }
            catch (DbException dx)
            {
                return BadRequest(dx.Message);
            }
            catch (ArgumentException ax)
            {
                return NotFound(ax.Message);
            }
        }

        /// <summary>
        /// Get Queries
        /// </summary>
        /// <remarks>
        /// Get all <see cref="Query"/> objects defined in the DataAccessSettings file.
        /// </remarks>
        /// <returns><see cref="IEnumerable{Query}"/></returns>
        [HttpGet("queries")]
        public IEnumerable<Query> GetQueries() => this.settings.Queries;

        /// <summary>
        /// Get names of all default queries.
        /// </summary>
        /// <remarks>
        /// Returns the names of all default queries available as a string list.
        /// </remarks>
        /// <returns></returns>
        [HttpGet("defaultQueries")]
        public IEnumerable<string> GetDefaultQueries() => defaultQueries.GetQueryNames();

        /// <summary>
        /// Get default query
        /// </summary>
        /// <remarks>
        /// Retrieve a default query named 'queryName'. You register a default query by adding the appropriate
        /// IQueryProvider in the DI services.
        /// </remarks>
        /// <param name="queryName"></param>
        /// <returns></returns>
        [HttpGet("defaultQueries/{queryName}")]
        public IActionResult GetDefaultQuery(string queryName)
        {
            try
            {
                return Ok(defaultQueries.GetQuery(queryName));
            }
            catch (ArgumentException x)
            {
                return NotFound(x.Message);
            }
        }

        /// <summary>
        /// Execute known query
        /// </summary>
        /// <remarks>
        /// Execute query stored on DataAccessSettings file using a known database connection.
        /// Query string contains values for query parameters.
        /// </remarks>
        /// <param name="connectionName">Connection name</param>
        /// <param name="queryName">Query name</param>
        /// <returns></returns>
        [HttpGet("queries/{connectionName}/{queryName}")]
        public async Task<IActionResult> ExecuteQuery(string connectionName, string queryName)
            => await ExecuteQueryInternal(connectionName, queryName, null);

        /// <summary>
        /// Execute query defined in request
        /// </summary>
        /// <remarks>
        /// Execute query defined in requests body using a known database connection.
        /// Query string contains values for query parameters.
        /// </remarks>
        /// <param name="connectionName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("queries/{connectionName}")]
        public async Task<IActionResult> ExecuteQuery(string connectionName, [FromBody] Query query)
            => await ExecuteQueryInternal(connectionName, null, query);

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("exportSettings")]
        public IActionResult GetExportsettings()
        {
            var settings = localStorage.Get<ExportSettings>(ExportSettings.StorageKey) ?? ExportSettings.Default;
            return Ok(settings);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("exportSettings")]
        public IActionResult SaveExportSettings(ExportSettings settings)
        {
            localStorage.Save(ExportSettings.StorageKey, settings);
            return Ok("Export Settings saved!");
        }

        private async Task<IActionResult> ExecuteQueryInternal(string connectionName, string queryName, Query query)
        {
            try
            {
                using var cnx = this.connectionManager.GetConnection(connectionName);
                await cnx.OpenAsync();

                if (query == null)
                    query = this.queryManager.GetSqlQuery(queryName);
                var dynamicParameters = new DynamicParameters();
                if (Request.Query.Count > 0)
                {
                    var nullValues = Request.Query.ContainsKey("nulls") ?
                        Request.Query["nulls"].ToArray() :
                        Enumerable.Empty<string>();
                    foreach (var item in Request.Query)
                    {
                        if (item.Key == "nulls") continue;
                        var prm = query.Parameters.Find(p => p.Name == item.Key);

                        if (prm == null)
                            throw new ArgumentException($"Parameter '{item.Key}' is not defined in the query!");
                        if (!Enum.TryParse(typeof(DbType), prm.DbType, out var dbType))
                            throw new ArgumentOutOfRangeException($"{prm.DbType} is not a valid DbType");

                        if (nullValues.Contains(item.Key))
                            dynamicParameters.Add(item.Key, DBNull.Value, (DbType)dbType);
                        else
                            dynamicParameters.Add(item.Key, item.Value.FirstOrDefault(), (DbType)dbType);
                    }
                }
                var result = cnx.Query<dynamic>(query.QueryCommand, dynamicParameters);

                return Ok(new { Results = result });
            }
            catch (DbException dx)
            {
                return BadRequest(dx.Message);
            }
            catch (ArgumentException ax)
            {
                return NotFound(ax.Message);
            }
        }

        public class ExportSettings
        {
            public static string StorageKey => "DataAccess.ExportSettings";

            public string Separator { get; set; }
            public bool IncludeHeaders { get; set; }

            public static ExportSettings Default = new ExportSettings
            {
                Separator      = ";",
                IncludeHeaders = true
            };
        }
    }
}