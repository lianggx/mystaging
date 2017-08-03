using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.App
{
    public class Test
    {

        class u_model
        {
            public Guid id { get; set; }
            public string title { get; set; }
        }

        static void TestExpression()
        {
            //Guid mid = Guid.Parse("70981417-88f2-4c07-b326-674993fa4b14");
            //UserDal user = new UserDal();
            //var result = user.Where(f => f.Id == mid).ToOne();
            //int affrows = UserDal.Update(mid)
            //                     .SetNickName("name")
            //                     .SaveChange();



            //u_model userModel = Public_guser.Context
            //    .Union<Public_appModel>("b", UnionType.INNER_JOIN, (a, b) => a.Id == b.Guser_id)
            //    .Union<Public_appModel, Public_appmoduleModel>("c", UnionType.LEFT_JOIN, (a, b) => a.Id == b.App_id)
            //    .Where<Public_appModel>(f => f.Guser_id == Guid.Parse("d3a35954-e5bb-406b-8300-dd83ae859b86"))
            //    .ToOne<u_model>("a.id,b.title");

            //Public_guserModel model = Public_guser.Context.Where(f => f.Phone == "1180000000").ToOne();
            //int count = Public_guser.Delete(model.Id);
            //int affrows = Public_guser.Update(model.Id).SetState(Et_proxy_state.冻结).SaveChange();
            //Public_guserModel user = new Public_guserModel();
            //user.Id = Guid.NewGuid();
            //user.Phone = "1180000000";
            //user.Create_time = DateTime.Now;
            //user.Reg_time = DateTime.Now;
            //user.Login_time = DateTime.Now;
            //user.State = Et_proxy_state.正常;
            //var um = Public_guser.Insert(user);
            // List<Mall_productModel> model = Mall_product.Context.Page(1, 10).OrderBy(f => f.Create_time).ToList();
        }
    }
}
