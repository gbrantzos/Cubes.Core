using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace Cubes.Core.DataAccess
{
    public class QueryExecutor : IQueryExecutor
    {
        private readonly ISqlQueryManager queryManager;
        private readonly IDatabaseConnectionManager connectionManager;

        public QueryExecutor(ISqlQueryManager queryManager, IDatabaseConnectionManager connectionManager)
        {
            this.queryManager = queryManager;
            this.connectionManager = connectionManager;
        }

        public IEnumerable<TResult> Query<TResult>(
            string namedConnection,
            SqlQuery sqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null,
            Action<TResult> afterPopulating = null) where TResult : class, new()
        {
            using (var cnx = connectionManager.GetConnection(namedConnection))
            {
                var rd = ExecuteSqlQuery(cnx, sqlQuery, parameterValues);
                return MapToObject<TResult>(rd, columnToProperty, afterPopulating);
            }
        }

        public IEnumerable<dynamic> Query(
            string namedConnection,
            SqlQuery sqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null)
        {
            using (var cnx = connectionManager.GetConnection(namedConnection))
            {
                var rd = ExecuteSqlQuery(cnx, sqlQuery, parameterValues);
                return MapToDynamic(rd, columnToProperty);
            }
        }

        public IEnumerable<TResult> Query<TResult>(
            string namedConnection,
            string namedSqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null,
            Action<TResult> afterPopulating = null) where TResult : class, new()
        {
            var sqlQuery = queryManager.GetSqlQuery(namedSqlQuery);
            return Query<TResult>(namedConnection, sqlQuery, parameterValues, columnToProperty, afterPopulating);
        }

        public IEnumerable<dynamic> Query(
            string namedConnection,
            string namedSqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null)
        {
            var sqlQuery = queryManager.GetSqlQuery(namedSqlQuery);
            return Query(namedConnection, sqlQuery, parameterValues, columnToProperty);
        }


        // Helper methods
        private IDataReader ExecuteSqlQuery(IDbConnection connection, SqlQuery query, Dictionary<string, object> parameterValues)
        {
            // Make sure connection is open
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            // Create connection
            var command = connection.CreateCommand();
            command.CommandText = query.Query;

            // Oracle hack...
            if (command.GetType().Name.Equals("OracleCommand"))
                command.GetType().GetProperty("BindByName").SetValue(command, true, null);

            // Add parameters
            if (query.Parameters != null && query.Parameters.Count > 0)
            {
                foreach (var queryParam in query.Parameters)
                {
                    var commandParam = command.CreateParameter();
                    command.Parameters.Add(commandParam);

                    commandParam.ParameterName = queryParam.Name;
                    commandParam.DbType = (DbType)Enum.Parse(typeof(DbType), queryParam.DbType);

                    if (parameterValues.ContainsKey(queryParam.Name))
                        commandParam.Value = ConvertParamValue(parameterValues[queryParam.Name], queryParam.DbType);
                    else
                        commandParam.Value = queryParam.Default ?? DBNull.Value;
                }
            }

            // Get reader
            return command.ExecuteReader();
        }

        private static object ConvertParamValue(object value, string dbTypeName)
        {
            object retValue = null;
            DbType dbType = (DbType)Enum.Parse(typeof(DbType), dbTypeName);
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.String:
                    retValue = value.ToString();
                    break;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    retValue = DateTime.Parse(value.ToString());
                    break;
                case DbType.Decimal:
                    retValue = Decimal.Parse(value.ToString());
                    break;
                case DbType.Single:
                    retValue = Single.Parse(value.ToString());
                    break;
                case DbType.VarNumeric:
                case DbType.Double:
                    retValue = Double.Parse(value.ToString());
                    break;
                case DbType.Guid:
                    retValue = Guid.Parse(value.ToString());
                    break;
                case DbType.Int16:
                    retValue = Int16.Parse(value.ToString());
                    break;
                case DbType.Int32:
                    retValue = Int32.Parse(value.ToString());
                    break;
                case DbType.Int64:
                    retValue = Int64.Parse(value.ToString());
                    break;
                case DbType.Time:
                    retValue = DateTime.Parse(value.ToString()).ToUniversalTime();
                    break;
                case DbType.UInt16:
                    retValue = UInt16.Parse(value.ToString());
                    break;
                case DbType.UInt32:
                    retValue = UInt32.Parse(value.ToString());
                    break;
                case DbType.UInt64:
                    retValue = UInt64.Parse(value.ToString());
                    break;
                case DbType.Binary:
                case DbType.Byte:
                case DbType.Boolean:
                case DbType.Currency:
                case DbType.Object:
                case DbType.SByte:
                case DbType.Xml:
                case DbType.DateTimeOffset:
                default:
                    break;
            }
            if (retValue == null)
                retValue = value;
            return retValue;
        }

        private IEnumerable<TResult> MapToObject<TResult>(IDataReader dataReader, Dictionary<string, string> columnToProperty, Action<TResult> onCreated = null) where TResult : class, new()
        {
            var toReturn = new List<TResult>();
            var resType = typeof(TResult);
            var properties = resType.GetProperties();
            if (columnToProperty == null)
                columnToProperty = new Dictionary<string, string>();

            // Create tmp Dictionary to lookup by property name
            var prop2column = columnToProperty
                .ToList()
                .ToDictionary(p => p.Value, p => p.Key);

            // Get DataReader columns
            var rdColumns = dataReader.
                GetSchemaTable().
                Rows.
                Cast<DataRow>().
                Select(i => i["ColumnName"].ToString().ToUpper()).
                ToArray();

            while (dataReader.Read())
            {
                var readerValues = new object[] { };
                dataReader.GetValues(readerValues);

                var resObj = new TResult();

                foreach (var p in properties)
                {
                    if (!prop2column.TryGetValue(p.Name, out var columnName))
                        columnName = p.Name;

                    // Safety
                    if (!rdColumns.Contains(columnName.ToUpper()))
                        continue;
                    if (dataReader[columnName] == DBNull.Value)
                        continue;

                    // Property values
                    object propValue = null;

                    // Enums handling
                    if (p.PropertyType.IsEnum && dataReader[columnName].GetType().Equals(typeof(Int32)))
                        propValue = Enum.ToObject(p.PropertyType, Convert.ToInt32(dataReader[columnName]));
                    if (p.PropertyType.IsEnum && dataReader[columnName].GetType().Equals(typeof(String)))
                        propValue = Enum.Parse(p.PropertyType, dataReader[columnName].ToString(), true);

                    // Default conversion
                    if (propValue == null)
                        propValue = Convert.ChangeType(dataReader[columnName], p.PropertyType);

                    // Set property value
                    p.SetValue(resObj, propValue, null);
                }

                // On created callback
                onCreated?.Invoke(resObj);

                toReturn.Add(resObj);
            }
            return toReturn;
        }

        private IEnumerable<dynamic> MapToDynamic(IDataReader dataReader, Dictionary<string, string> columnToProperty)
        {
            var toReturn = new List<dynamic>();

            if (columnToProperty == null)
                columnToProperty = new Dictionary<string, string>();

            // Get DataReader columns
            var rdColumns = dataReader.
                GetSchemaTable().
                Rows.
                Cast<DataRow>().
                Select(i => i["ColumnName"].ToString().ToUpper()).
                ToArray();

            while (dataReader.Read())
            {
                var readerValues = new object[] { };
                dataReader.GetValues(readerValues);

                var dynamicObj = new ExpandoObject();

                foreach (var column in rdColumns)
                {
                    // Safety
                    if (dataReader[column] == DBNull.Value)
                        continue;

                    if (!columnToProperty.TryGetValue(column, out var propertyName))
                        propertyName = column;

                    AddProperty(dynamicObj, propertyName, dataReader[column]);
                }
                toReturn.Add(dynamicObj);
                
            }

            return toReturn;
        }

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
