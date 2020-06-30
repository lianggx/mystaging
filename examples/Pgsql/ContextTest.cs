using MyStaging.Common;
using Pgsql.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pgsql
{
    public class ContextTest
    {
        const string connectionString = "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;Pooling=true;Maximum Pool Size=1000;";
        StagingOptions options;
        PgsqlDbContext userContext;
        public void Start()
        {
            options = new StagingOptions("user", connectionString);
            userContext = new PgsqlDbContext(options);

            Insert();
            Delete();
            Update();
            Select();
        }

        private void Insert()
        {
            List<UserModel> users = new List<UserModel>();
            for (int i = 0; i < 100; i++)
            {
                var user = new UserModel
                {
                    id = ObjectId.NewId().ToString(),
                    age = 18,
                    createtime = DateTime.Now,
                    password = "123456",
                    nickname = "lgx",
                    IP = "127.0.0.1",
                    loginname = "lgx",
                    money = 0,
                    role = et_role.普通成员,
                    sex = true,
                    wealth = 0
                };
                users.Add(user);
            }

            var affrows = userContext.User.Insert.AddRange(users).SaveChange();
            var firstUser = users[0];
            firstUser.id = ObjectId.NewId().ToString();
            userContext.User.Insert.Add(firstUser);

            Console.WriteLine("AddRange=={0}", affrows);
            Console.WriteLine("Add=={0}", firstUser.id);
        }
        private void Delete()
        {
            var user = userContext.User.Select.OrderByDescing(f => f.createtime).ToOne();
            userContext.User.Delete.Where(f => f.id == user.id).SaveChange();

            var delete = userContext.User.Select.Where(f => f.id == user.id).ToOne();
            Console.WriteLine("Delete==User:{0},{1}", user.id, delete == null);
        }
        private void Update()
        {
            var user = userContext.User.Select.OrderByDescing(f => f.createtime).ToOne();
            var newUser = userContext.User.Update.SetValue(f => f.age, 28)
                                                         .Where(f => f.id == user.id)
                                                         .SaveChange();

            Console.WriteLine("Update==user:{0},Age:old[{1}],new[{2}]", user.id, user.age, newUser.age);
        }
        private void Select()
        {
            for (int i = 1; i < 10; i++)
            {
                var userId = "5ddc9d8eb5ee485e50000001";
                var sum = userContext.User.Select.Sum<long>(f => f.age);
                Console.WriteLine(sum);
                var user = userContext.User.Select.Where(f => f.id == userId).ToOne();
             //   var users = userContext.User.Select.ToList();
                //Console.WriteLine("userid=={0}", user.id);
             //   Console.WriteLine("users=={0}--", users.Count);
                //users.Clear();
            }
        }
    }
}
