using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Cubes.Core.DataAccess
{
    public class ConnectionManager : IConnectionManager
    {
        public static Dictionary<string, string> RegisteredProviders { get; private set; }

        private readonly DataAccessOptions options;

        public ConnectionManager(IOptionsSnapshot<DataAccessOptions> options)
            => this.options = options.Value;

        public static void RegisterProviders()
        {
            RegisteredProviders = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DbProviderFactory)))
                .Select(type => new
                {
                    TypeName     = type.FullName,
                    AssemblyName = type.Assembly.GetName().Name
                })
                .ToDictionary(t => t.AssemblyName, t => t.TypeName);
        }

        public DbConnection GetConnection(Connection connection)
        {
            var providerType = GetProviderType(connection.DbProvider);
            var providerInst = Activator.CreateInstance(providerType, true);
            var cnxFactory   = providerInst.GetType().GetMethod(nameof(DbProviderFactory.CreateConnection));

            var dbConnection = cnxFactory.Invoke(providerInst, null) as DbConnection;
            dbConnection.ConnectionString = connection.ConnectionString;

            return dbConnection;
        }

        public DbConnection GetConnection(string connectionName)
        {
            var connectionInfo = options
                .Connections
                .Find(i => i.Name.Equals(connectionName, StringComparison.CurrentCultureIgnoreCase));
            if (connectionInfo != null)
                return GetConnection(connectionInfo);
            else
                throw new ArgumentException($"Could not find connection: {connectionName}");
        }

        // Get provider type
        private Type GetProviderType(string providerName)
        {
            var type = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(t => t.FullName.Equals(providerName));
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