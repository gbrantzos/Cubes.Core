using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cubes.Core.Commands;
using Cubes.Core.DataAccess;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ICommandBus bus;
        private readonly ICubesEnvironment cubes;
        private readonly ISettingsProvider settingsProvider;
        private readonly IQueryExecutor queryExecutor;

        public ValuesController(ICommandBus bus, ICubesEnvironment cubes, ISettingsProvider settingsProvider, IQueryExecutor queryExecutor)
        {
            this.bus = bus;
            this.cubes = cubes;
            this.settingsProvider = settingsProvider;
            this.queryExecutor = queryExecutor;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var settings = settingsProvider.Load<ValuesSettings>();Console.WriteLine($"SettingsProvider hashcode: {settingsProvider.GetHashCode()}");
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var ttypes = asm.GetTypes().ToList();
                // Console.WriteLine($"{asm.GetName()} - Types: {ttypes.Count()}");
                }
                catch (System.Exception x)
                {
                    Console.WriteLine($"Error getting types from: '{asm.GetName()}");
                    // Console.WriteLine( x.Message);
                }

            }
            //return new string[] { "value1", "value2", cubes.GetFolder(FolderKind.Root) };
            /*
            var types = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Select(x => x.FullName)
                .Where(n => n.StartsWith("MySql.Data.MySqlClient.MySqlClientFactory")).ToList();
                //return types;
            var type = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => t.FullName.Equals("MySql.Data.MySqlClient.MySqlClientFactory"))
                .FirstOrDefault();


            //     .Where(t => t.FullName.Equals("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data"))
            //     .FirstOrDefault();
            //Type type = Type.GetType("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data");
            var providerInst = Activator.CreateInstance(type, true);
            var cnxFactory = providerInst.GetType().GetMethod("CreateConnection");

            var connection = cnxFactory.Invoke(providerInst, null) as DbConnection;
            connection.ConnectionString = "Server=localhost;Database=ibe;Uid=ibe;Pwd=ibe;";
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "select count(*) from ibe.acriss_category";
            var rd = cmd.ExecuteReader();
            while(rd.Read())
            {
                Console.WriteLine($"Result: {rd.GetInt32(0)}");
            }
            rd.Close();
            connection.Close();

            */
            var res = bus.Submit(new ACommnad { });
            return Ok(new { V1 = "value1", V2 = "value2", CubesRoot = cubes.GetFolder(CubesFolderKind.Root), Settings = settings });
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

    [SettingsPrefix("Values")]
    public class ValuesSettings
    {
        public int ID { get; set; }
        public string Description { get; set; }
    }
}
