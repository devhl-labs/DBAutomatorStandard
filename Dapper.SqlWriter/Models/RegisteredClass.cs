using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;
using System.Threading.Tasks;
using System.Data;

namespace Dapper.SqlWriter
{
    public class RegisteredClass<C> where C : class
    {
        internal string DatabaseTableName { get; set; } = string.Empty;

        public List<RegisteredProperty<C>> RegisteredProperties { get; set; } = new List<RegisteredProperty<C>>();

        public SqlWriter SqlWriter { get; internal set; }

        public RegisteredClass(SqlWriter sqlWriter)
        {
            SqlWriter = sqlWriter;

            DatabaseTableName = GetTableName();

            Dictionary<string, string> columnMaps = new Dictionary<string, string>();

            foreach (PropertyInfo property in typeof(C).GetProperties())
            {
                RegisteredProperty<C> registeredProperty = new RegisteredProperty<C>(this, property, SqlWriter)
                {
                    ColumnName = GetColumnName(property),

                    IsKey = IsKey(property),

                    IsAutoIncrement = IsAutoIncrement(property),

                    NotMapped = !property.IsStorable()
                };

                RegisteredProperties.Add(registeredProperty);

                if (registeredProperty.Property.Name != registeredProperty.ColumnName)                
                    columnMaps.Add(registeredProperty.ColumnName, registeredProperty.Property.Name);                
            }

            var mapper = new Func<Type, string, PropertyInfo>((type, columnName) =>
            {
                if (columnMaps.ContainsKey(columnName))                
                    return type.GetProperty(columnMaps[columnName]);                
                else                
                    return type.GetProperty(columnName);                
            });

            var map = new CustomPropertyTypeMap(typeof(C), (type, columnName) => mapper(type, columnName));

            SqlMapper.SetTypeMap(typeof(C), map);
        }

        public RegisteredClass<C> NotMapped(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Utils.GetMemberExpression(key);

            RegisteredProperties.First(p => p.Property.Name == member.Member.Name).NotMapped = true;

            return this;
        }

        public RegisteredClass<C> Key(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Utils.GetMemberExpression(key);

            RegisteredProperties.First(p => p.Property.Name == member.Member.Name).IsKey = true;

            return this;
        }

        public RegisteredClass<C> AutoIncrement(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Utils.GetMemberExpression(key);

            RegisteredProperties.First(p => p.Property.Name == member.Member.Name).IsAutoIncrement = true;

            return this;
        }

        public RegisteredClass<C> TableName(string tableName)
        {
            DatabaseTableName = tableName;

            return this;
        }

        private ColumnMap? _columnMap = null;

        public RegisteredClass<C> ColumnName(Expression<Func<C, object>> key, string columnName)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Utils.GetMemberExpression(key);

            RegisteredProperties.First(p => p.Property.Name == member.Member.Name).ColumnName = columnName;

            ColumnMap(member.Member.Name, columnName);

            return this;
        }

        private void ColumnMap(string memberName, string columnName)
        {
            _columnMap ??= new ColumnMap();

            _columnMap.Add(memberName, columnName);

            SqlMapper.SetTypeMap(typeof(C), new CustomPropertyTypeMap(typeof(C), (type, columnName) => type.GetProperty(_columnMap[columnName])));

        }

        private bool IsAutoIncrement(PropertyInfo property)
        {
            if (Attribute.IsDefined(property, typeof(AutoIncrementAttribute)))            
                return true;            

            return false;
        }

        private bool IsKey(PropertyInfo property)
        {
            if (Attribute.IsDefined(property, typeof(KeyAttribute)))            
                return true;            

            return false;
        }

        private string GetTableName()
        {
            string result = SqlWriter.ToTableName(typeof(C).Name);

            if (typeof(C).GetCustomAttributes<TableAttribute>(true).FirstOrDefault() is TableAttribute tableNameAttribute)            
                result = tableNameAttribute.Name;            

            if (SqlWriter.Capitalization == Capitalization.Lower)
                return result.ToLower();

            if (SqlWriter.Capitalization == Capitalization.Upper)
                return result.ToUpper();

            return result;
        }

        private string GetColumnName(PropertyInfo property)
        {
            string memberName = property.Name;

            string columnName = property.Name;

            if (SqlWriter.Capitalization == Capitalization.Lower)
                columnName = memberName.ToLower();

            if (SqlWriter.Capitalization == Capitalization.Upper)
                columnName = memberName.ToUpper();

            if (property.GetCustomAttributes<ColumnAttribute>(true).FirstOrDefault() is ColumnAttribute columnNameAttribute)
                columnName = columnNameAttribute.Name;

            if (memberName != columnName)
                ColumnMap(memberName, columnName);

            return columnName;
        }

        public RegisteredProperty<C> RegisteredProperty(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression memberExpression = Utils.GetMemberExpression(key);

            return RegisteredProperties.First(p => p.Property.Name == memberExpression.Member.Name);
        }

        public Func<string, string, DynamicParameters, IDbConnection, int?, Task<IEnumerable<C>>> QueryAsync = DefaultQueryAsync;

        private static async Task<IEnumerable<C>> DefaultQueryAsync(string sql, string splitOn, DynamicParameters p, IDbConnection connection, int? commandTimeOut)
        {
            return await connection.QueryAsync<C>(sql, p, null, commandTimeOut).ConfigureAwait(false);
        }
    }
}
