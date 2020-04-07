using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using MyStaging.Helpers;

namespace MyStaging.Mapping
{
    public class DynamicBuilder<T>
    {
        private delegate T Load(IDataRecord dataRecord);
        private Load handler;

        private DynamicBuilder() { }

        public T Build(IDataRecord dataRecord)
        {
            return handler(dataRecord);
        }

        public static DynamicBuilder<T> CreateBuilder(IDataRecord dr)
        {
            DynamicBuilder<T> dynamicBuilder = new DynamicBuilder<T>();
            dynamicBuilder.handler = (Load)MyStagingUtils.CreateDynamicDelegate(typeof(T), dr, typeof(Load));
            return dynamicBuilder;
        }
    }

    public class DynamicBuilder
    {
        private delegate object Load(IDataRecord dataRecord);
        private Load handler;
        public Type TargetType { get; set; }

        internal DynamicBuilder(Type targetType)
        {
            this.TargetType = targetType;
        }

        public object Build(IDataRecord dataRecord)
        {
            return handler(dataRecord);
        }

        public DynamicBuilder CreateBuilder(IDataRecord dr)
        {
            this.handler = (Load)MyStagingUtils.CreateDynamicDelegate(TargetType, dr, typeof(Load));
            return this;
        }
    }


}
