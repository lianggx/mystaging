using IdentityHost.Model;
using System.Collections.Generic;

namespace IdentityHost.Services
{
    public class AccessLogService : IManagerService
    {
        private readonly IdentityHostDbContext dbContext;
        public AccessLogService(IdentityHostDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string ServiceName => nameof(AccessLogService);

        public M_Accesslog Add(M_Accesslog accesslog)
        {
            return dbContext.M_Accesslog.Insert.Add(accesslog);
        }

        public M_Accesslog Detail(int id)
        {
            var log = dbContext.M_Accesslog.Select.Where(f => f.Id == id).ToOne();

            return log;
        }

        public List<M_Accesslog> List(int pageIndex, int pageSize)
        {
            var builder = dbContext.M_Accesslog.Select.Page(pageIndex, pageSize).OrderByDescing(f => f.CreateTime);

            return builder.ToList();
        }
    }
}
