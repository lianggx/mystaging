using System;

namespace MyStaging.Interface
{
    public interface IQueryPipe : IQueryContext
    {
        IQueryPipe ToPipe(int page = 1, int size = 10, params string[] fields);
        Type PipeResultType { get; set; }
    }
}