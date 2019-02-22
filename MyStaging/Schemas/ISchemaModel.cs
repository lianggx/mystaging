using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MyStaging.Schemas
{
    public interface ISchemaModel
    {
        Dictionary<string, SchemaModel> SchemaSet { get; }
        List<PropertyInfo> Properties { get; }
    }
}
