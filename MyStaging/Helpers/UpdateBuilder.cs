using MyStaging.Helpers;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace MyStaging.Helpers
{
    public class UpdateBuilder<T> : QueryContext<T> where T : class, new()
    {
        private List<string> setList = new List<string>();
        protected UpdateBuilder<T> SetField(string field, NpgsqlDbType dbType, object value, Type specificType = null)
        {
            base.AddParameter(field, dbType, value, specificType);
            setList.Add($"{field}=@{field}");
            return this;
        }

        public int SaveChange()
        {
            string tableName = MyStagingUtils.GetMapping(typeof(T));
            string cmdText = $"UPDATE {tableName} SET {string.Join(",", this.setList)} WHERE id = @id";
            return base.ExecuteNonQuery(cmdText);
        }
    }
}
