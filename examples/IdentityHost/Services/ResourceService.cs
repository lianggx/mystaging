using IdentityHost.Model;
using MyStaging.Function;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityHost.Services
{
    public class ResourceService : IManagerService
    {
        private readonly IdentityHostDbContext dbContext;
        public ResourceService(IdentityHostDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string ServiceName => nameof(ResourceService);

        public M_Resource Add(M_Resource resource)
        {
            resource.CreateTime = DateTime.Now;
            resource.State = 0;
            resource = dbContext.M_Resource.Insert.Add(resource);

            return resource;
        }

        public M_Resource Edit(int id, string content, string name, bool authorize, int type, int? parentId, int sort)
        {
            var resource = dbContext.M_Resource.Update.SetValue(f => f.Content, content)
                                                               .SetValue(f => f.Name, name)
                                                               .SetValue(f => f.Authorize, authorize)
                                                               .SetValue(f => f.Type, type)
                                                               .SetValue(f => f.ParentId, parentId)
                                                               .SetValue(f => f.Sort, sort)
                                                               .Where(f => f.Id == id)
                                                               .SaveChange();

            return resource;
        }

        public bool Delete(int id)
        {
            dbContext.M_Resource.Update.SetValue(f => f.ParentId, null).Where(f => f.ParentId == id).SaveChange();
            var affrows = dbContext.M_Resource.Delete.Where(f => f.Id == id).SaveChange();

            return affrows > 0;
        }

        public List<M_Resource> Root(int? type)
        {
            var builder = dbContext.M_Resource.Select.Where(f => f.State == 0 && f.ParentId == null);
            if (type.HasValue)
            {
                builder.Where(f => f.Type == type.Value);
            }

            var list = builder.OrderByDescing(f => f.Sort).ToList();

            return list;
        }

        public M_Resource Detail(int id)
        {
            var resource = dbContext.M_Resource.Select.Where(f => f.Id == id).ToOne();
            return resource;
        }

        public M_Resource Detail(string content)
        {
            var resource = dbContext.M_Resource.Select.Where(f => f.Content == content).ToOne();
            return resource;
        }

        public List<M_Resource> List(int type)
        {
            var resources = dbContext.M_Resource.Select.Where(f => f.State == 0 && f.Type == type).ToList("DISTINCT a.*");

            return GroupResource(resources);
        }

        public List<M_Resource> ResourceByRole(int[] roleId)
        {
            var resources = dbContext.M_Resource.Select.InnerJoin<M_Roleresource>("b", (a, b) => a.Id == b.ResourceId)
                                                             .Where(f => f.Type == 1)
                                                             .Where<M_Roleresource>(f => f.RoleId.In(roleId))
                                                             .OrderByDescing(f => f.Sort)
                                                             .ToList("DISTINCT a.*");

            return GroupResource(resources);
        }

        public List<M_Resource> ResourceByRole(int roleId)
        {
            var resources = dbContext.M_Resource.Select.InnerJoin<M_Roleresource>("b", (a, b) => a.Id == b.ResourceId)
                                                                   .Where("b.roleid={0}", roleId)
                                                                   .OrderBy(f => f.Name)
                                                                   .ToList();
            return resources;
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
