using MyStaging.Common;
using MyStaging.Core;
using System;
using System.Data;

namespace MyStaging.Mapping
{
    public class DynamicBuilder<T> : IDisposable
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
            DynamicBuilder<T> dynamicBuilder = new DynamicBuilder<T>
            {
                handler = (Load)MyStagingUtils.CreateDynamicDelegate(typeof(T), dr, typeof(Load))
            };
            return dynamicBuilder;
        }

        public void Dispose()
        {
            this.handler = null;
        }
    }

    public class DynamicBuilder : IDisposable
    {
        private delegate object Load(IDataRecord dataRecord);
        private Load handler;
        public Type TargetType { get; set; }

        public DynamicBuilder(Type targetType)
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

        public void Dispose()
        {
            this.handler = null;
        }
    }


}
