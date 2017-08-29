# mystaging
* 这是一个 .netcore+pgsql 的脚手架，它可以让你使用 .netcore2.0的新特性，基于 pgsql 数据库，可以在项目中自由的使用 lambda 表达式编写业务，同时支持自定义的 sql 语句。
* mystaging，非常的小巧，下面将介绍 mystaging 的项目框架。

* 该项目目前处于起步阶段，可能不适用于大型项目，请结合业务需要酌情使用
  
---

* 开发交流QQ群：614072957

---
**构建工具 MyStaging.App**
  1. 将 MyStaging.csproj 项目打包成 MyStaging.zip ，并复制到 MyStaging.App/bin/debug 目录下
  2. 编辑构建工具下的 @build.bat 文件,配置相关参数，参数配置见*参数说明*
  3. 运行该批处理文件，可以直接生成 proj.db 项目文件
     
**参数说明**
   * **-h** host，数据库所在服务器地址
   * **-p** port,端口号
   * **-u** username，登录数据库账号名称
   * **-a** password，登录密码
   * **-d** database，数据库名称
   * **-pool** pool,数据库连接池最大值，默认 32
   * **-proj** csproj,生成的 db 层项目名称
   * **-o** output path，生成项目的输出路径

---

**实体对象说明**
   构建工具会自动生成DAL层，包含实体模型和对象关系，由于执行数据库查询的过程中，高度依赖实体模型映射，所以在实体模型时，对实体做了一些ORM的映射设置，这些设置对实体店影响非常小。
    
---
 * **EntityMappingAttribute**
        该特性类接受一个属性：TableName，指明该实体模型映射到数据库中的>模式.表名，如

        ```
        [EntityMapping(TableName = "public.user")]
        public partial class Public_userModel
        {
        }
        ```
        
---

* **ForeignKeyMappingAttribute**
        应用该该特性类到属性上，表示这个是一个外键引用的属性，如

        ```
        private Public_userModel _public_User=null;
        [ForeignKeyMapping,JsonIgnore]public Public_userModel Public_User { get{ if(_public_User==null)_public_User= Public_user.Context.Where(f=>f.Id==this.User_id).ToOne();  return _public_User;} }
        ```
        *以上代码还应用了特性：JsonIgnore ，表示，该外键在对象进行 json 序列化的时候选择忽略该属性*

---

*  **NonDbColumnMappingAttribute**
        应用该该特性类到属性上，表示这个是一个自定义的属性，在进行数据库查询的时候将忽略该属性，如

        ```
        [NonDbColumnMappingAttribute,JsonIgnore] public  Public_user.UpdateBuilder UpdateBuilder{get{return new Public_user.UpdateBuilder(this.Id);}}
        ```
        
  *以上代码还应用了特性：JsonIgnore ，表示，该外键在对象进行 json 序列化的时候选择忽略该属性*

---

####MyStaging 命名空间
*  **MyStaging.Common**  主要定义公共类型和资源
* **MyStaging.Helpers**     数据库操作帮助类、连接池管理工具类、DAL操作辅助函数
* **MyStaging.Mapping**  动态对象生成管理、实体对象映射定义

**MyStaging.Helpers.QueryContext**
    DAL继承的基类，该类实现了所有对数据库操作的封装，可直接继承使用，如果使用脚手架附带的构建工具，直接进行业务编写即可

---

**数据库操作**

---

* **插入记录**
``` C#
    Public_userModel user = new Public_userModel();
    user.Id = Guid.NewGuid();
    user.Login_name = "test@gmail.com";
    Public_user.Insert(user);
```

---

* **修改记录**
``` C#
    // 自动根据主键修改
    Public_userModel user = new Public_userModel();
    user.UpdateBuilder.SetLogin_time(DateTime.Now).SaveChange(); 

    // 自定义条件修改
    user.UpdateBuilder.SetLogin_time(DateTime.Now).Where(f => f.Sex == true).SaveChange();

    // 直接修改
    Public_user.Update(Guid.Empty).SetLogin_time(DateTime.Now).Where(f => f.Sex == true).SaveChange();

    // 自定义条件的直接修改
    Public_user.UpdateBuilder.SetLogin_time(DateTime.Now).Where(f => f.Id == Guid.Empty).Where(f => f.Sex == true).SaveChange();
```

---

* **删除记录**
``` C#
    // 根据主键删除
    Public_user.Delete(Guid.Empty);
    // 根据条件删除
    Public_user.DeleteBuilder.Where(f => f.Id == Guid.Empty).Where(f => f.Sex == true).SaveChange();
```

---

* **查询单条记录**
``` C#
    Public_userModel user = Public_user.Context.Where(f => f.Login_name == "test@gmail.com").ToOne();
```

---


* **指定查询列**
``` C#
    Public_userModel user = Public_user.Context.Where(f => f.Login_name == "test@gmail.com").ToOne("id","login_name");
```

---

* **指定查询返回类型**
``` C#
    public class UserModel{
        public string Login_name{get;set;}
        public Guid Id{get;set;}
    }
    Public_userModel user = Public_user.Context.Where(f => f.Login_name == "test@gmail.com").ToOne<UserModel>("id","login_name");
```

---

* **查询列表**
``` C#
    List<Public_userModel> list = Public_user.Context.ToList();
    List<Public_userModel> list = Public_user.Context.Where(f => f.Login_name == "test@gmail.com").ToList();    

    public class UserModel{
        public string Login_name{get;set;}
        public Guid Id{get;set;}
    }

    List<UserModel> list = Public_user.Context.ToList<UserModel>("id","login_name");

```

---

* **表连接查询**
``` C#
    public class UserModel{
        public string Login_name{get;set;}
        public Guid Id{get;set;}
    }

    List<UserModel> list = Public_user.Context.Union<TopicModel>("b",UnionType.INNER_JOIN,(a,b)=>a.Id==b.User_Id).Where(a=>a.Id=Guid.Empty).Where<TopicModel>(b=>b.Publish==true).ToList<UserModel>("id","login_name");

```
---

* **分页**
``` C#
    Public_user.Context.Where(f => f.Login_name == "test@gmail.com").OrderBy(f=>f.State).Page(1,10);
    Public_user.Context.Where(f => f.Login_name == "test@gmail.com").OrderDescing(f=>f.State);
```
---

* **排序**
``` C#
    Public_user.Context.Where(f => f.Login_name == "test@gmail.com").OrderBy(f=>f.State);
    Public_user.Context.Where(f => f.Login_name == "test@gmail.com").OrderDescing(f=>f.State);
```
---

* **聚合查询**
``` C#
    Public_user.Context.Where(f => f.Login_name == "test@gmail.com").Avg(f=>f.Age);
    Public_user.Context.Where(f => f.Login_name == "test@gmail.com").Sum(f=>f.Blance);
    // Max,Min,GroupBy,Having
```
---

* **事务**
``` C#
     PgSqlHelper.Transaction(() =>
        {
            Public_userModel user= Public_user.Context.Where(f => f.Login_name == "test@gmail.com").ToOne();
            user.UpdateBuilder.SetLogin_time(DateTime.Now).SaveChange();
        });
```
---


