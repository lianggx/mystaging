using MyStaging.Mapping;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MyStaging.Common
{
    public class ContractUtils
    {
        public static List<PropertyInfo> GetProperties(Type type)
        {
            var properties = new List<PropertyInfo>();
            var pis = type.GetProperties();
            for (int j = 0; j < pis.Length; j++)
            {
                PropertyInfo pi = pis[j];
                var attr = pi.GetCustomAttribute(typeof(NonDbColumnMappingAttribute));
                if (attr != null) continue;
                properties.Add(pi);
            }

            return properties;
        }
    }
}
