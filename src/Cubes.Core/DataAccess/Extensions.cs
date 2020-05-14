using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Cubes.Core.Utilities;

namespace Cubes.Core.DataAccess
{
    public static class Extensions
    {
        /// <summary>
        /// Execute a non query command.static Parameters are passed as an anonymous object.
        /// </summary>
        /// <param name="dbConnection">Connection to use</param>
        /// <param name="commandText">Sql statement</param>
        /// <param name="commandParameters">Parameters</param>
        /// <returns></returns>
        public static int ExecuteCommand(this DbConnection dbConnection, string commandText, object commandParameters = null)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            DataTable schema = dbConnection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
            string text = schema.Rows[0][DbMetaDataColumnNames.ParameterMarkerFormat].ToString();
            if (text == "{0}")
            {
                text = "@{0}";
            }
            using DbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = commandText;
            if (commandParameters != null)
            {
                foreach (KeyValuePair<string, object> item in commandParameters.AsPropertiesDictionary())
                {
                    string text2 = item.Key;
                    if (!text2.StartsWith(text[0].ToString()))
                    {
                        text2 = string.Format(text, text2);
                    }
                    DbParameter dbParameter = dbCommand.CreateParameter();
                    dbParameter.ParameterName = text2;
                    dbParameter.Value = item.Value;
                    dbCommand.Parameters.Add(dbParameter);
                }
            }
            return dbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Map DataTable rows to object list of type TResult.
        /// </summary>
        /// <param name="dt">Source data table</param>
        /// <param name="mappings">Property name to row label mappings</param>
        /// <param name="onCreated">Action to execute after object is populated</param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TResult> ToObjectList<TResult>(this DataTable dt,
            Dictionary<string, string> mappings = null,
            Action<TResult> onCreated = null) where TResult : new()
        {
            DataColumnCollection rowCols = dt.Columns;
            Type resType = typeof(TResult);
            if (mappings == null)
            {
                mappings = new Dictionary<string, string>();
            }
            foreach (DataRow row in dt.Rows)
            {
                TResult resObj = new TResult();
                foreach (PropertyInfo p in resType.GetProperties())
                {
                    if (!mappings.TryGetValue(p.Name, out string columnName))
                    {
                        columnName = p.Name;
                    }
                    if (rowCols.Contains(columnName) && row[columnName] != DBNull.Value)
                    {
                        object propValue = null;
                        if (p.PropertyType.IsEnum && rowCols[columnName].DataType.Equals(typeof(int)))
                        {
                            propValue = Enum.ToObject(p.PropertyType, Convert.ToInt32(row[columnName]));
                        }
                        if (p.PropertyType.IsEnum && rowCols[columnName].DataType.Equals(typeof(string)))
                        {
                            propValue = Enum.Parse(p.PropertyType, row[columnName].ToString(), ignoreCase: true);
                        }
                        if (propValue == null)
                        {
                            propValue = Convert.ChangeType(row[columnName], p.PropertyType);
                        }
                        p.SetValue(resObj, propValue, null);
                    }
                }
                onCreated?.Invoke(resObj);
                yield return resObj;
            }
        }
    }
}