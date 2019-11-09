using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace DBAutomatorStandard
{
    internal static class PostgresMapping
    {
        public static bool IsStorable(this PropertyInfo propertyInfo)
        {
            if (typeof(INotMapped).IsAssignableFrom(propertyInfo.GetType()))
            {
                return false;
            }

            if (propertyInfo.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() is NotMappedAttribute notMappedAttribute)
            {
                return false;
            }

            if (propertyInfo.PropertyType == typeof(string) ||
                propertyInfo.PropertyType == typeof(ulong) ||
                propertyInfo.PropertyType == typeof(ulong?) ||
                propertyInfo.PropertyType == typeof(bool) ||
                propertyInfo.PropertyType == typeof(bool?) ||
                propertyInfo.PropertyType == typeof(decimal) ||
                propertyInfo.PropertyType == typeof(decimal?) ||
                propertyInfo.PropertyType == typeof(DateTime) ||
                propertyInfo.PropertyType == typeof(DateTime?) ||
                propertyInfo.PropertyType == typeof(int) ||
                propertyInfo.PropertyType == typeof(int?) ||
                propertyInfo.PropertyType == typeof(long) ||
                propertyInfo.PropertyType == typeof(long?) ||

                propertyInfo.PropertyType.IsEnum
                )
            {
                return true;
            }
            return false;
        }

        //public static void AddParameter(this DynamicParameters p, ConditionModel conditionModel)
        //{
        //    if (conditionModel.Value is ulong)
        //    {
        //        p.Add($"{conditionModel.Name}", Convert.ToInt64(conditionModel.Value));
        //    }
        //    else if (conditionModel.Value is ulong?)
        //    {
        //        if (conditionModel.Value == null)
        //        {
        //            p.Add($"{conditionModel.Name}", null);
        //        }
        //        else
        //        {
        //            p.Add($"{conditionModel.Name}", Convert.ToInt64(conditionModel.Value));
        //        }
        //    }
        //    else
        //    {
        //        p.Add($"{conditionModel.Name}", conditionModel.Value);
        //    }
        //}

        //public static void AddParameter<I>(this DynamicParameters dynamicParameters, PropertyInfo propertyInfo, I item)
        //{
        //    if (!propertyInfo.IsStorable()) { return; }

        //    if (propertyInfo.PropertyType == typeof(ulong))
        //    {
        //        dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", System.Convert.ToInt64(propertyInfo.GetValue(item)));
        //    }
        //    else if (propertyInfo.PropertyType == typeof(ulong?))
        //    {
        //        if (propertyInfo.GetValue(item) == null)
        //        {
        //            dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", null);
        //        }
        //        else
        //        {
        //            dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", System.Convert.ToInt64(propertyInfo.GetValue(item)));
        //        }
        //    }
        //    else
        //    {
        //        dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", propertyInfo.GetValue(item));
        //    }
        //}

















        /// <summary>
        /// Returns true if the provided property is a simple type that can be stored in a database.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        //public static bool IsStorable(this PropertyInfo propertyInfo)
        //{
        //    if (typeof(INotMapped).IsAssignableFrom(propertyInfo.GetType()))
        //    {
        //        return false;
        //    }

        //    if (propertyInfo.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() is NotMappedAttribute notMappedAttribute)
        //    {
        //        return false;
        //    }

        //    if (propertyInfo.PropertyType == typeof(string) ||
        //        propertyInfo.PropertyType == typeof(ulong) ||
        //        propertyInfo.PropertyType == typeof(ulong?) ||
        //        propertyInfo.PropertyType == typeof(bool) ||
        //        propertyInfo.PropertyType == typeof(bool?) ||
        //        propertyInfo.PropertyType == typeof(decimal) ||
        //        propertyInfo.PropertyType == typeof(decimal?) ||
        //        propertyInfo.PropertyType == typeof(DateTime) ||
        //        propertyInfo.PropertyType == typeof(DateTime?) ||
        //        propertyInfo.PropertyType == typeof(int) ||
        //        propertyInfo.PropertyType == typeof(int?) ||
        //        propertyInfo.PropertyType == typeof(long) ||
        //        propertyInfo.PropertyType == typeof(long?) ||

        //        propertyInfo.PropertyType.IsEnum
        //        )
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// Maps the objects type to the equivalent type in the database.
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public static string MapToPostgreSql(this object obj)
        //{
        //    if (obj is string)
        //    {
        //        return "text";
        //    }
        //    if (obj is ulong || obj is ulong? || obj is long || obj is long?)
        //    {
        //        return "bigint";
        //    }
        //    if (obj is bool || obj is bool?)
        //    {
        //        return "boolean";
        //    }
        //    if (obj.GetType().IsEnum)
        //    {
        //        return "int";
        //    }
        //    if (obj is decimal || obj is decimal?)
        //    {
        //        return "decimal";
        //    }
        //    if (obj is DateTime || obj is DateTime?)
        //    {
        //        return "timestamp without time zone";
        //    }
        //    if (obj is int || obj is int?)
        //    {
        //        return "int";
        //    }
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Maps the objects type to the equivalent type in the database.
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public static string MapToPostgreSql(this PropertyInfo property)
        //{
        //    if (property.PropertyType == typeof(string))
        //    {
        //        return "text";
        //    }
        //    if (property.PropertyType == typeof(ulong) || property.PropertyType == typeof(ulong?) || property.PropertyType == typeof(Int64) || property.PropertyType == typeof(Int64?))
        //    {
        //        return "bigint";
        //    }
        //    if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
        //    {
        //        return "boolean";
        //    }
        //    if (property.PropertyType.IsEnum)
        //    {
        //        return "int";
        //    }
        //    if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
        //    {
        //        return "decimal";
        //    }
        //    if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
        //    {
        //        return "timestamp without time zone";
        //    }
        //    if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
        //    {
        //        return "int";
        //    }
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Get the name of the query that matches the expression.
        ///// </summary>
        ///// <typeparam name="I"></typeparam>
        ///// <param name="queryType"></param>
        ///// <param name="tableName"></param>
        ///// <param name="whereConditions"></param>
        ///// <param name="setConditions"></param>
        ///// <returns></returns>
        //public static string GetProcedureName<I>(QueryType queryType, string tableName, List<ConditionModel> whereConditions, List<ConditionModel>? setConditions = null)
        //{
        //    string result = GetLongProcedureName(queryType, tableName, whereConditions, setConditions);

        //    if (result.Length > 63)
        //    {
        //        result = GetShortProcedureName<I>(queryType, tableName, whereConditions, setConditions);
        //    }

        //    return result;
        //}

        //private static string GetLongProcedureName(QueryType queryType, string tableName, List<ConditionModel> whereConditions, List<ConditionModel>? setConditions = null)
        //{

        //    string procName = $"z{tableName.ToLower()}_{queryType.ToString().ToLower()}_";

        //    foreach (ConditionModel condition in whereConditions ?? Enumerable.Empty<ConditionModel>())
        //    {
        //        if (condition.Operator == "=")
        //        {
        //            procName = $"{procName}{condition.Name.ToLower()}_";
        //        }
        //        else if (condition.Operator == "<")
        //        {
        //            procName = $"{procName}l_{condition.Name.ToLower()}_";
        //        }
        //        else if (condition.Operator == ">")
        //        {
        //            procName = $"{procName}g_{condition.Name.ToLower()}_";
        //        }
        //        else if (condition.Operator == "IS NOT")
        //        {
        //            procName = $"{procName}n_{condition.Name.ToLower()}_";
        //        }
        //        else
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    foreach (ConditionModel condition in setConditions ?? Enumerable.Empty<ConditionModel>())
        //    {
        //        if (condition.Operator == "=")
        //        {
        //            procName = $"{procName}s{condition.Name.ToLower()}_";
        //        }
        //        else if (condition.Operator == "<")
        //        {
        //            procName = $"{procName}l_s{condition.Name.ToLower()}_";
        //        }
        //        else if (condition.Operator == ">")
        //        {
        //            procName = $"{procName}g_s{condition.Name.ToLower()}_";
        //        }
        //        else if (condition.Operator == "IS NOT")
        //        {
        //            procName = $"{procName}n_s{condition.Name.ToLower()}_";
        //        }
        //        else
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    procName = procName.Substring(0, procName.Length - 1);

        //    return procName;
        //}

        //private static string GetShortProcedureName<I>(QueryType queryType, string tableName, List<ConditionModel> whereConditions, List<ConditionModel>? setConditions = null)
        //{
        //    var props = typeof(I).GetProperties().ToList();

        //    string procName = $"z{tableName.ToLower()}_{queryType.ToString().ToLower()}_";

        //    foreach (ConditionModel condition in whereConditions ?? Enumerable.Empty<ConditionModel>())
        //    {
        //        if (condition.Operator == "=")
        //        {
        //            procName = $"{procName}{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else if (condition.Operator == "<")
        //        {
        //            procName = $"{procName}l_{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else if (condition.Operator == ">")
        //        {
        //            procName = $"{procName}g_{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else if (condition.Operator == "IS NOT")
        //        {
        //            procName = $"{procName}n_{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    foreach (ConditionModel condition in setConditions ?? Enumerable.Empty<ConditionModel>())
        //    {
        //        if (condition.Operator == "=")
        //        {
        //            procName = $"{procName}s{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else if (condition.Operator == "<")
        //        {
        //            procName = $"{procName}l_s{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else if (condition.Operator == ">")
        //        {
        //            procName = $"{procName}g_s{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else if (condition.Operator == "IS NOT")
        //        {
        //            procName = $"{procName}n_s{props.FindIndex(p => p.Name == condition.Name)}_";
        //        }
        //        else
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    procName = procName.Substring(0, procName.Length - 1);

        //    return procName;
        //}

        ///// <summary>
        ///// Returns the name of the table derived from the name of the class or the TableNameAttribute
        ///// </summary>
        ///// <typeparam name="I"></typeparam>
        ///// <returns></returns>
        //public static string GetTableName<I>()
        //{
        //    string result = typeof(I).Name;

        //    if (typeof(I).GetCustomAttributes<TableNameAttribute>(true).FirstOrDefault() is TableNameAttribute tableNameAttribute)
        //    {
        //        result = tableNameAttribute.TableName;
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Returns the name of the column derived from the name of the property or the ColumnNameAttribute
        ///// </summary>
        ///// <typeparam name="I"></typeparam>
        ///// <returns></returns>
        //public static string GetColumnName<I>(string propertyName)
        //{
        //    string result = propertyName;

        //    PropertyInfo propertyInfo = typeof(I).GetProperty(propertyName);

        //    if (propertyInfo.GetCustomAttributes<ColumnNameAttribute>(true).FirstOrDefault() is ColumnNameAttribute columnNameAttribute)
        //    {
        //        result = columnNameAttribute.ColumnName;
        //    }

        //    return result;
        //}

        /// <summary>
        /// Add the properties and values to the dynamic parameters based on the conditions provided in the expression.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="conditionModel"></param>
        /// <param name="prefix"></param>
        //public static void AddParameter(DynamicParameters p, ConditionModel conditionModel, string prefix = "_")
        //{
        //    //if (conditionModel.Value is ulong)
        //    //{
        //    //    p.Add($"{prefix}{conditionModel.Name.ToLower()}", Convert.ToInt64(conditionModel.Value));
        //    //}
        //    //else if (conditionModel.Value is ulong?)
        //    //{
        //    //    if (conditionModel.Value == null)
        //    //    {
        //    //        p.Add($"{prefix}{conditionModel.Name.ToLower()}", null);
        //    //    }
        //    //    else
        //    //    {
        //    //        p.Add($"{prefix}{conditionModel.Name.ToLower()}", Convert.ToInt64(conditionModel.Value));
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    p.Add($"{prefix}{conditionModel.Name.ToLower()}", conditionModel.Value);
        //    //}
        //    if (conditionModel.Value is ulong)
        //    {
        //        p.Add($"{conditionModel.Name}", Convert.ToInt64(conditionModel.Value));
        //    }
        //    else if (conditionModel.Value is ulong?)
        //    {
        //        if (conditionModel.Value == null)
        //        {
        //            p.Add($"{conditionModel.Name}", null);
        //        }
        //        else
        //        {
        //            p.Add($"{conditionModel.Name}", Convert.ToInt64(conditionModel.Value));
        //        }
        //    }
        //    else
        //    {
        //        p.Add($"{conditionModel.Name}", conditionModel.Value);
        //    }
        //}

        /// <summary>
        /// Add the properties to the dynamic parameters and set the value based on the provided I item object.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="conditionModel"></param>
        /// <param name="prefix"></param>
        //public static void AddParameter<I>(DynamicParameters dynamicParameters, PropertyInfo propertyInfo, I item)
        //{
        //    if (!propertyInfo.IsStorable()) { return; }

        //    if (propertyInfo.PropertyType == typeof(ulong))
        //    {
        //        dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", System.Convert.ToInt64(propertyInfo.GetValue(item)));
        //    }
        //    else if (propertyInfo.PropertyType == typeof(ulong?))
        //    {
        //        if (propertyInfo.GetValue(item) == null)
        //        {
        //            dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", null);
        //        }
        //        else
        //        {
        //            dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", System.Convert.ToInt64(propertyInfo.GetValue(item)));
        //        }
        //    }
        //    else
        //    {
        //        dynamicParameters.Add($"_{propertyInfo.Name.ToLower()}", propertyInfo.GetValue(item));
        //    }
        //}
    }
}
