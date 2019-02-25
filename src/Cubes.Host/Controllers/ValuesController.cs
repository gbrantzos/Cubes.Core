using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cubes.Core.Commands;
using Cubes.Core.Environment;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ICommandBus bus;
        private readonly ICubesEnvironment cubes;

        public ValuesController(ICommandBus bus, ICubesEnvironment cubes)
        {
            this.bus = bus;
            this.cubes = cubes;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
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


            var res = bus.Submit(new ACommnad { });
            return new string[] { "value1", "value2", cubes.GetFolder(FolderKind.Root) };
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
