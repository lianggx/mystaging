using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MyStaging.Schemas
{
    public interface ISchemaModel
    {
        List<SchemaModel> SchemaSet { get; }
        List<PropertyInfo> Properties { get; }
    }
}
