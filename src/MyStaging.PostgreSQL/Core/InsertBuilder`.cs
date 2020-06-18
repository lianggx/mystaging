using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Interface.Core;
using MyStaging.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MyStaging.PostgreSQL.Core
{
    public class InsertBuilder<T> : IInsertBuilder<T> where T : class
    {
        private readonly DbContext dbContext;
        private readonly List<T> models = new List<T>();
        private static readonly object zeroTime = new DateTime();
        private static readonly Dictionary<string, string> defaultValueField = new Dictionary<string, string>
        {
            { "createtime",null },
            { "create_time", null }
        };

        public InsertBuilder(DbContext context)
        {
            this.dbContext = context;
        }

        public T Add(T model)
        {
            this.models.Add(model);
            this.ToSQL();
            CommandText += " RETURNING *;";
            dbContext.Execute.ExecuteDataReader(dr =>
            {
                model = DynamicBuilder<T>.CreateBuilder(dr).Build(dr);
            }, CommandType.Text, CommandText, Parameters.ToArray());

            this.models.Clear();
            return model;
        }

        /// <summary>
        ///  调用该方法必须在最后调用 SaveChange()，否则不会将数据保存到数据库
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public IInsertBuilder<T> AddRange(List<T> items)
        {
            this.models.AddRange(items);
            return this;
        }

        /// <summary>
        ///  将当前更改保存到数据库
        /// </summary>
        /// <returns></returns>
        public int SaveChange()
        {
            this.ToSQL();
            var affrows = dbContext.Execute.ExecuteNonQuery(CommandType.Text, CommandText, Parameters.ToArray());
            this.models.Clear();
            return affrows;
        }

        public string ToSQL()
        {
            if (this.models.Count == 0)
                throw new ArgumentOutOfRangeException("No items.");

            Parameters.Clear();

            string tableName = MyStagingUtils.GetMapping(typeof(T));
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"INSERT INTO {tableName}");

            string fieldsName = string.Empty;
            var pis = MyStagingUtils.GetProperties(typeof(T));
            foreach (var p in pis)
            {
                fieldsName += "\"" + p.Name + "\",";
            }

            fieldsName = fieldsName.Remove(fieldsName.Length - 1, 1);
            sqlBuilder.Append($"({fieldsName}) VALUES ");

            for (int i = 0; i < models.Count; i++)
            {
                string paramNameString = string.Empty;
                foreach (var pi in pis)
                {
                    var paramName = $"@{pi.Name}_{i}";
                    paramNameString += paramName + ",";
                    var value = pi.GetValue(models[i]);
                    var primaryKey = pi.GetCustomAttribute<PrimaryKeyAttribute>() != null;
                    if (primaryKey || defaultValueField.ContainsKey(pi.Name.ToLower()))
                    {
                        if (value == null
                            || value.Equals(Guid.Empty)
                            || zeroTime.Equals(value))
                            value = GetDefaultValue(pi);
                    }
                    Parameters.Add(new Npgsql.NpgsqlParameter(paramName, value));
                }

                paramNameString = paramNameString.Remove(paramNameString.Length - 1, 1);
                sqlBuilder.Append($"({paramNameString}),");
            }

            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            CommandText = sqlBuilder.ToString();

            return CommandText;
        }

        private object GetDefaultValue(PropertyInfo pi)
        {
            object defaultValue = null;
            if (pi.PropertyType == typeof(Guid))
            {
                defaultValue = Guid.NewGuid();
            }
            else if (defaultValueField.ContainsKey(pi.Name.ToLower()))
            {
                if (pi.PropertyType == typeof(TimeSpan))
                    defaultValue = DateTime.Now.TimeOfDay;
                else
                    defaultValue = DateTime.Now;
            }

            return defaultValue;
        }

        public List<DbParameter> Parameters { get; set; } = new List<DbParameter>();
        public string CommandText { get; set; }
    }
}
