using MyStaging.Helpers;
using MyStaging.Mapping;
using MyStaging.Schemas;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库更新对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InsertBuilder<T> : QueryContext<T> where T : class, new()
    {
        private readonly List<T> setList = new List<T>();
        private List<T> models = new List<T>();
        private ISchemaModel schema;
        private static object zeroTime = new DateTime();
        private static Dictionary<string, string> defaultValueField = new Dictionary<string, string>
        {
            { "createtime",null },
            { "create_time", null }
        };

        public InsertBuilder() { }

        public InsertBuilder(ISchemaModel schema)
        {
            this.schema = schema;
        }

        /// <summary>
        ///  从数据库数组字段中移除指定的值
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="dbType">数据库数据类型</param>
        /// <param name="value">字段指定的值</param>
        /// <param name="size">字段大小</param>
        /// <param name="specificType">通常在字段类型为枚举类型时需要指定</para
        /// <returns></returns>
        public T Insert(T model)
        {
            this.models.Add(model);
            this.ToString();
            this.CommandText += " RETURNING *;";
            return base.InsertOnReader(this.CommandText);
        }


        /// <summary>
        ///  从数据库数组字段中移除指定的值
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="dbType">数据库数据类型</param>
        /// <param name="value">字段指定的值</param>
        /// <param name="size">字段大小</param>
        /// <param name="specificType">通常在字段类型为枚举类型时需要指定</para
        /// <returns></returns>
        public InsertBuilder<T> InsertRange(List<T> items)
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
            this.ToString();
            var affrows = base.ExecuteNonQuery(this.CommandText);
            return affrows;
        }

        /// <summary>
        ///  重写方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.models.Count == 0)
                throw new ArgumentOutOfRangeException("No items.");

            base.ParamList.Clear();

            string tableName = MyStagingUtils.GetMapping(typeof(T));
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"INSERT INTO {tableName}");

            string fieldsName = string.Empty;
            var fieldCollection = this.schema.SchemaSet;
            foreach (var field in fieldCollection)
            {
                fieldsName += "\"" + field.FieldName + "\",";
            }

            fieldsName = fieldsName.Remove(fieldsName.Length - 1, 1);
            sqlBuilder.Append($"({fieldsName}) VALUES ");

            for (int i = 0; i < models.Count; i++)
            {
                var mObj = this.models[i];
                string piNames = string.Empty;

                foreach (var field in fieldCollection)
                {
                    PropertyInfo pi = this.schema.Properties.Where(f => f.Name.ToLower() == field.FieldName.ToLower()).FirstOrDefault();
                    if (pi == null)
                        throw new KeyNotFoundException($"数据库字段名称：{field.FieldName} 在实体对象 {mObj.GetType().Name} 上未找到对应属性！");

                    var piName = $"@{field.FieldName}_{i}";
                    piNames += piName + ",";
                    var value = pi.GetValue(mObj);
                    if (field.Primarykey || defaultValueField.ContainsKey(field.FieldName))
                    {
                        if (value == null
                            || value.Equals(Guid.Empty)
                            || zeroTime.Equals(value))
                            value = CreateDefaultValue(field);
                    }

                    base.AddParameter(piName, field.DbType, value, field.Size);
                }

                piNames = piNames.Remove(piNames.Length - 1, 1);
                sqlBuilder.Append($"({piNames}),");
            }

            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            base.CommandText = sqlBuilder.ToString();

            return CommandText;
        }

        private object CreateDefaultValue(SchemaModel model)
        {
            object defaultValue = null;
            if (model.DbType == NpgsqlDbType.Uuid)
            {
                defaultValue = Guid.NewGuid();
            }
            else if (defaultValueField.ContainsKey(model.FieldName.ToLower()))
            {
                if (model.DbType == NpgsqlDbType.Time || model.DbType == NpgsqlDbType.TimeTZ)
                    defaultValue = DateTime.Now.TimeOfDay;
                else
                    defaultValue = DateTime.Now;
            }

            return defaultValue;
        }
    }
}
