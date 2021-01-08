using IdentityHost.Model;
using MyStaging.Function;
using System;
using System.Collections.Generic;

namespace IdentityHost.Services
{
    public class RoleService : IManagerService
    {
        private readonly IdentityHostDbContext dbContext;
        public RoleService(IdentityHostDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string ServiceName => nameof(RoleService);

        public M_Role Add(M_Role role)
        {
            role.CreateTime = DateTime.Now;
            role.State = 0;
            return dbContext.M_Role.Insert.Add(role);
        }

        public M_Role EditName(int id, string name)
        {
            var role = dbContext.M_Role.Update.SetValue(f => f.Name, name).Where(f => f.Id == id).SaveChange();
            return role;
        }

        public bool Delete(int id)
        {
            var affrows = dbContext.M_Role.Delete.Where(f => f.Id == id).SaveChange();
            affrows += dbContext.M_Mapping.Delete.Where(f => f.RoleId == id).SaveChange();
            return affrows > 0;
        }

        public M_Role Detail(int id)
        {
            return dbContext.M_Role.Select.Where(f => f.Id == id).ToOne();
        }

        public List<M_Role> List(int pageIndex, int pageSize)
        {
            var results = dbContext.M_Role.Select.Page(pageIndex, pageSize).OrderByDescing(f => f.CreateTime).ToList();

            return results;
        }

        public List<M_Role> GetRoles(int userId)
        {
            var roles = dbContext.M_Mapping.Select.InnerJoin<M_Role>("b", (a, b) => a.RoleId == b.Id && b.State == 0)
                                                                                   .Where(f => f.UserId == userId)
                                                                                   .OrderBy(f => f.CreateTime)
                                                                                   .ToList<M_Role>("b.Id,b.Name");
            return roles;
        }

        public bool AddR2U(int userId, int[] roleId)
        {
            var affrows = 0;
            if (roleId?.Length > 0)
            {
                var roles = dbContext.M_Role.Select.Where(f => f.Id.In(roleId)).ToList();
                if (roles.Count == 0)
                    throw new ArgumentException();

                var insertItem = new List<M_Mapping>();
                foreach (var item in roles)
                {
                    var roleItem = new M_Mapping
                    {
                        UserId = userId,
                        RoleId = item.Id
                    };

                    insertItem.Add(roleItem);
                }

                affrows = dbContext.M_Mapping.Delete.Where(f => f.UserId == userId).SaveChange();
                affrows += dbContext.M_Mapping.Insert.AddRange(insertItem).SaveChange();
            }
            else
            {
                affrows = dbContext.M_Mapping.Delete.Where(f => f.UserId == userId).SaveChange();
            }

            return affrows > 0;
        }

        public bool AddR2R(int roleId, int[] resourceId)
        {
            var affrows = 0;

            if (resourceId?.Length > 0)
            {
                var res = dbContext.M_Resource.Select.Where(f => f.Id.In(resourceId)).ToList();
                if (res.Count == 0)
                    throw new ArgumentException();

                var resources = new List<M_Roleresource>();
                foreach (var item in res)
                {
                    var roleresource = new M_Roleresource
                    {
                        ResourceId = item.Id,
                        RoleId = roleId
                    };
                    resources.Add(roleresource);
                }
                affrows = dbContext.M_Roleresource.Delete.Where(f => f.RoleId == roleId).SaveChange();
                affrows += dbContext.M_Roleresource.Insert.AddRange(resources).SaveChange();
            }
            else
            {
                affrows = dbContext.M_Roleresource.Delete.Where(f => f.RoleId == roleId).SaveChange();
            }

            return affrows > 0;
        }

        public bool ValidatorRole(int resourceId, int[] roleId)
        {
            return dbContext.M_Roleresource.Select.Where(f => f.ResourceId == resourceId && f.RoleId.In(roleId)).ToOne() != null;
        }
    }
}
