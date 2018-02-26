using MyStaging.Mapping;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MyStaging.Helpers
{
    public class MyStagingUtils
    {
        public static string GetMapping(Type t)
        {
            string tableName = string.Empty;
            TypeInfo typeInfo = t.GetTypeInfo();
            EntityMappingAttribute mapping = typeInfo.GetCustomAttribute(typeof(EntityMappingAttribute)) as EntityMappingAttribute;
            if (mapping != null)
            {
                tableName = mapping.Name;
                if (!string.IsNullOrEmpty(mapping.Schema))
                {
                    tableName = mapping.Schema + "." + tableName;
                }
            }
            else
                throw new NotSupportedException("在表连接实体上找不到特性 EntityMappingAttribute ，请确认数据库实体模型");

            return tableName;
        }
    }
}
