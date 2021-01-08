using Mysql.Model;
using MyStaging.Function;
using System.Collections.Generic;
using System.Linq;

namespace Mysql.Services
{
    public class ResourceService
    {
        private readonly MysqlDbContext dbContext;
        public ResourceService(MysqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<M_Resource> List(int type)
        {
            var resources = dbContext.M_Resource.Select.Where(f => f.State == 0 && f.Type == type).ToList("DISTINCT a.*");

            return GroupResource(resources);
        }

        public List<M_Resource> ResourceByRole(int[] roles)
        {
            var resources = dbContext.M_Resource.Select.InnerJoin<M_Roleresource>("b", (a, b) => a.Id == b.ResourceId)
                                                             .Where(f => f.Type == 1)
                                                             .Where<M_Roleresource>(f => f.RoleId.In(roles))
                                                             .OrderByDescing(f => f.Sort)
                                                             .ToList("DISTINCT a.*");

            return GroupResource(resources);
        }

        private List<M_Resource> GroupResource(List<M_Resource> resources)
        {
            var result = new List<M_Resource>();
            var group = resources.OrderByDescending(f => f.Sort).GroupBy(g => g.ParentId);
            foreach (var g in group)
            {
                if (g.Key.HasValue)
                {
                    result.AddRange(g.OrderByDescending(f => f.Sort).ToList());
                }
                else
                {
                    var item = resources.Where(f => f.Id == g.Key).FirstOrDefault();
                    if (item != null)
                    {
                        item.Children = g.OrderByDescending(f => f.Sort).ToList();
                    }
                }
            }

            return result;
        }
    }
}
