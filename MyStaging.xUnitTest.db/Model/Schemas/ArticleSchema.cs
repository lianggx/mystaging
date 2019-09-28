using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.Schemas;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Reflection;

namespace MyStaging.xUnitTest.Model.Schemas
{
    public partial class ArticleSchema : ISchemaModel
    {
        public static ArticleSchema Instance => new ArticleSchema();
        public Dictionary<string, SchemaModel> SchemaSet => new Dictionary<string, SchemaModel>
            {
                {"id", new SchemaModel{ FieldName = "id", DbType =  NpgsqlDbType.Varchar, Size = -1 ,Primarykey = true} },
                {"userid", new SchemaModel{ FieldName = "userid", DbType =  NpgsqlDbType.Varchar, Size = -1 ,Primarykey = true} },
                {"title", new SchemaModel{ FieldName = "title", DbType =  NpgsqlDbType.Varchar, Size = 255} },
                {"content", new SchemaModel{ FieldName = "content", DbType =  NpgsqlDbType.Jsonb, Size = -1} },
                {"createtime", new SchemaModel{ FieldName = "createtime", DbType =  NpgsqlDbType.Timestamp, Size = 8} }
            };
        public List<PropertyInfo> Properties => ContractUtils.GetProperties(typeof(ArticleModel));

    }
}
