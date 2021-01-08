using IdentityHost.Helpers;
using IdentityHost.Model;
using MyStaging.Function;
using System;
using System.Collections.Generic;

namespace IdentityHost.Services
{
    public class UserService : IManagerService
    {
        private readonly IdentityHostDbContext dbContext;
        public UserService(IdentityHostDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string ServiceName => nameof(UserService);

        public M_User Add(M_User user, List<int> roles)
        {
            user.LoginName = user.LoginName.ToLower();
            user.State = 0;
            user.CreateTime = DateTime.Now;
            user.Password = SecurityHelper.GetSHA256SignString(user.Password);

            user = dbContext.M_User.Insert.Add(user);
            UpdateRole(user.Id, roles);

            return user;
        }

        private void UpdateRole(int userId, List<int> roles)
        {
            if (roles.Count > 0)
            {
                List<M_Mapping> maps = new List<M_Mapping>();
                foreach (var item in roles)
                {
                    var map = new M_Mapping
                    {
                        UserId = userId,
                        CreateTime = DateTime.Now,
                        RoleId = item
                    };
                    maps.Add(map);
                }

                dbContext.M_Mapping.Insert.AddRange(maps).SaveChange();
            }
        }

        public M_User Edit(int id, string name, string imgFace, string phone, List<int> roles)
        {
            var user = dbContext.M_User.Update.SetValue(f => f.Name, name)
                                                        .SetValue(f => f.ImgFace, imgFace)
                                                        .SetValue(f => f.Phone, phone)
                                                        .Where(f => f.Id == id)
                                                        .SaveChange();
            UpdateRole(id, roles);

            return user;
        }

        public M_User Detail(string loginName)
        {
            var user = dbContext.M_User.Select.Where(f => f.LoginName == loginName.ToLower()).ToOne();

            return user;
        }

        public M_User Detail(int id)
        {
            var user = dbContext.M_User.Select.Where(f => f.Id == id).ToOne();

            return user;
        }

        public bool UpdatePassword(int id, string password)
        {
            var newPassword = SecurityHelper.GetSHA256SignString(password);
            var user = dbContext.M_User.Update.SetValue(f => f.Password, newPassword).Where(f => f.Id == id).SaveChange();

            return user != null;
        }

        public bool Delete(int id)
        {
            var user = dbContext.M_User.Update.SetValue(f => f.State, 3).Where(f => f.Id == id).SaveChange();
            dbContext.M_Mapping.Delete.Where(f => f.UserId == user.Id).SaveChange();

            return user != null;
        }

        public List<M_User> List(string name, int state, int pageIndex, int pageSize)
        {
            var builder = dbContext.M_User.Select;
            if (state >= 0)
            {
                builder.Where(f => f.State == state);
            }
            if (!string.IsNullOrEmpty(name))
            {
                builder.Where(f => f.Name.Like(name));
            }
            var list = builder.Page(pageIndex, pageSize).OrderByDescing(f => f.CreateTime).ToList();

            return list;
        }
    }
}
