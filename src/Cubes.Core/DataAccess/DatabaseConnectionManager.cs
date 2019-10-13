using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Cubes.Core.DataAccess
{
    public class DatabaseConnectionManager : IDatabaseConnectionManager
    {
        private static Dictionary<string, string> knownProviders = new Dictionary<string, string>()
        {
            { "oracle", "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess" },
            { "mssql",  "System.Data.SqlClient.SqlClientFactory, System.Data" },
            { "mysql",  "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data" }
        };
        private readonly DataAccessSettings settings;

        public DatabaseConnectionManager(IOptionsSnapshot<DataAccessSettings> options)
            => this.settings = options.Value;

        public DbConnection GetConnection(string connectionName)
        {
            var connections = settings.Connections;
            var connectionInfo = connections.FirstOrDefault(i => i.Name.Equals(connectionName, StringComparison.CurrentCultureIgnoreCase));
            if (connectionInfo != null)
            {
                string providerName = String.Empty;
                if (!knownProviders.TryGetValue(connectionInfo.DbProvider, out providerName))
                    providerName = connectionInfo.DbProvider;

                var providerType = GetProviderType(providerName);
                var providerInst = Activator.CreateInstance(providerType, true);
                var cnxFactory = providerInst.GetType().GetMethod("CreateConnection");

                var connection = cnxFactory.Invoke(providerInst, null) as DbConnection;
                connection.ConnectionString = connectionInfo.ConnectionString;

                return connection;
            }
            else
                throw new ArgumentException($"Could not find connection: {connectionName}");
        }

        // Get provider type
        private Type GetProviderType(string providerName)
        {
            if (providerName.IndexOf(",", StringComparison.Ordinal) > 0)
                providerName = providerName.Substring(0, providerName.IndexOf(",", StringComparison.Ordinal));
            var type = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => t.FullName.Equals(providerName))
                .FirstOrDefault();
            if (type == null)
                throw new ArgumentException($"Could not create DbProvider type for '{providerName}'");

            return type;
        }

        /*
        Just in case we need it ...
        https://github.com/godsharp/GodSharp.Data.Common.DbProvider/blob/master/src/GodSharp.Data.Common.DbProvider/DbProviderFactories.cs

        public static DbProviderFactory GetFactory(string providerInvariantName)
        {
            Type type = Type.GetType(providerInvariantName);
            if (null != type)
            {
                FieldInfo field = type.GetField("Instance", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
                if (null != field && field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
                {
                    object value = field.GetValue(null);
                    if (value != null)
                    {
                        return (DbProviderFactory)value;
                    }
                }
            }
        }
        */
    }
}