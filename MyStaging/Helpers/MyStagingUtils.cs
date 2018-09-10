using MyStaging.Mapping;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库实体查找对象
    /// </summary>
    public class MyStagingUtils
    {
        /// <summary>
        ///  根据传入的实体对象获得数据库架构级表的映射名称
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
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
