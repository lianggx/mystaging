using MySql.Data.MySqlClient;
using MyStaging.Common;
using MyStaging.Core;
using MyStaging.DataAnnotations;
using MyStaging.Interface.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Linq;

namespace MyStaging.MySql.Core
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

            var properties = MyStagingUtils.GetDbFields(typeof(T));

            // 检查自增
            PropertyInfo autoIncrement = null;
            foreach (var pi in properties)
            {
                var pk = pi.GetCustomAttribute<PrimaryKeyAttribute>();
                if (pk != null && pk.AutoIncrement)
                {
                    autoIncrement = pi;
                    break;
                }
            }

            if (autoIncrement != null)
            {
                this.CommandText += "\n SELECT LAST_INSERT_ID();";
            }
            try
            {
                using var reader = dbContext.ByMaster().Execute.ExecuteDataReader(CommandType.Text, CommandText, this.Parameters.ToArray());
                if (autoIncrement != null)
                {
                    reader.Read();
                    var value = reader[0];
                    value = Convert.ChangeType(value, autoIncrement.PropertyType);
                    autoIncrement.SetValue(model, value);
                }

                return model;
            }
            finally
            {
                this.Clear();
            }
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

            string tableName = MyStagingUtils.GetMapping(typeof(T), ProviderType.MySql);
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"INSERT INTO {tableName}");

            string fieldsName = string.Empty;
            var properties = MyStagingUtils.GetDbFields(typeof(T));
            foreach (var p in properties)
            {
                fieldsName += $"`{p.Name}`,";
            }

            fieldsName = fieldsName.Remove(fieldsName.Length - 1, 1);
            sqlBuilder.Append($"({fieldsName}) VALUES ");

            for (int i = 0; i < models.Count; i++)
            {
                string valueString = string.Empty;
                foreach (var pi in properties)
                {
                    var paramName = $"@{pi.Name}_{i}";
                    var value = pi.GetValue(models[i]);
                    var pk = pi.GetCustomAttribute<PrimaryKeyAttribute>();
                    var hasPK = pk != null;
                    if (hasPK && pk.AutoIncrement)
                    {
                        valueString += "default,";
                    }
                    else
                    {
                        valueString += paramName + ",";
                        if (hasPK || defaultValueField.ContainsKey(pi.Name.ToLower()))
                        {
                            if (value == null || value.Equals(Guid.Empty) || zeroTime.Equals(value))
                                value = GetDefaultValue(pi);
                        }
                        Parameters.Add(new MySqlParameter(paramName, value));
                    }
                }
                valueString = valueString.Remove(valueString.Length - 1, 1);
                sqlBuilder.Append($"({valueString}),");
            }

            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            sqlBuilder.Append(";");
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

        public virtual void Clear()
        {
            this.models.Clear();
            this.Parameters.Clear();
            this.CommandText = null;
        }

        public List<DbParameter> Parameters { get; set; } = new List<DbParameter>();
        public string CommandText { get; set; }
    }
}
