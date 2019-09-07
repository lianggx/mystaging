using MyStaging.Mapping;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace MyStaging.Interface
{
    public interface IQueryContext
    {
        List<NpgsqlParameter> ParamList { get; set; }
        string CommandText { get; set; }
        object ReadObj(DynamicBuilder builder, DbDataReader dr, Type objType);
    }
}