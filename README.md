# mystaging介绍
* 这是一个 .netcore+pgsql 的脚手架，可以一键生成实体对象和业务层接口，让开发人员无需关注底层变动，专注编写业务代码，它可以让你使用 .netcore2.0的新特性，基于 pgsql 数据库，可以在项目中自由的使用 lambda 表达式编写业务，同时支持自定义的 sql 语句。

**特性**
* mystaging，非常的小巧，下面将介绍 mystaging 的项目框架。
* 一键生成，无需编写实体模型代码
* 自动生成外键关系对象，延迟加载，使用对象的时候加载数据
* 内置连接池管理，无需重复创建连接
* 简化的删除/修改操作，支持多条件表达式
* 支持数据库枚举类型自动映射生成
* 支持视图自动生成实体模型和业务操作接口
* 除了可以使用 lambda 进行编写业务外，还支持复杂的自定义查询语句和条件
* 支持 GIS 函数调用
* 生成数据库实体和自定义实体隔离，支持分部扩展类
* 支持读写分离，支持多个从库配置，在配置了从库的情况下，读取数据默认使用从库连接，可以独立指定某条 sql 语句执行时使用主库
* 增加异常通知委托外部订阅接口
* 自动主从数据库切换，当单个从库连接不可用时，自动切换到其它从库，当所有从库连接不可用时，自动切换到主库
* 可配置的多库连接负载均衡，根据不同服务器性能调整最大连接数，建立自动监视从库异常连接机制，待异常连接可用时恢复到连接队列
* 对多表连接的扩展支持
* 在执行 sql 语句失败时输出完整 sql（含参数化） 语句的方法
* 所有方法的均有代码注释和参数说明，方便阅读和调用
* 该项目目前已成功应用到多个项目中
* 项目 gitbhub 地址：<https://github.com/lianggx/mystaging>
* 开发交流QQ群：614072957

---

**如何开始？**

###### 使用构建工具 MyStaging.App
  1. 使用MyStaging.App生成数据库项目，然后在该项目上引用 Mystaging.csproj源码项目，或者，你可以使用 ** nuget ** 命令进行包引用
    ###### # nuget install mystaging

  2. 编辑构建工具下的 @build.bat 文件,配置相关参数，参数配置见*参数说明*
  3. 运行该批处理文件，可以直接生成 proj.db 项目文件
     
###### 参数说明
   * **-h** host，数据库所在服务器地址
   * **-p** port,端口号
   * **-u** username，登录数据库账号名称
   * **-a** password，登录密码
   * **-d** database，数据库名称
   * **-pool** pool,数据库连接池最大值，默认 32
   * **-proj** csproj,生成的 db 层项目名称
   * **-o** output path，生成项目的输出路径

###### 初始化数据库连接
* 在生成的 db 项目文件根目录下，找到： _startup.cs 文件，在程序入口 Program.cs 或者  Startup.cs 的适当位置，使用以下代码，传递日志记录对象和数据库连接字符串进行 db 层初始化
* **初始化示例代码**
``` C#

	string connectionString = "Host=127.0.0.1;Port=5432;Username=postgres;Password=123456;Database=database name;Pooling=true;Maximum Pool Size=100";
	ILogger logger = loggerFactory.CreateLogger<MyStaging.Helpers.PgSqlHelper>();
	_startup.Init(logger, connectionString, null, -1);
```

---

**实体对象说明**
   构建工具会自动生成DAL层，包含实体模型和对象关系，由于执行数据库查询的过程中，高度依赖实体模型映射，所以在生成实体模型时，对实体做了一些ORM的映射设置，这些设置对实体模型的影响非常小。
    

 * **EntityMappingAttribute**
        该特性类接受一个属性：TableName，指明该实体模型映射到数据库中的>模式.表名，如

        ```
        [EntityMapping(name: "public", Schema = "user")]
        public partial class UserModel
        {
        }
        ```
        

* **ForeignKeyMappingAttribute**
        应用该特性类到属性上，表示这个是一个外键引用的属性，如

        ```
        private UserModel user=null;
        [ForeignKeyMapping,JsonIgnore]public UserModel User { get{ if(user==null) user= User.Context.Where(f=>f.Id==this.User_id).ToOne();  return user;} }
        ```
        *以上代码还应用了特性：JsonIgnore ，表示，该外键在对象进行 json 序列化的时候选择忽略该属性*



* **NonDbColumnMappingAttribute**
        应用该特性类到属性上，表示这个是一个自定义的属性，在进行数据库查询的时候将忽略该属性，如

        ```
        [NonDbColumnMappingAttribute,JsonIgnore] public  User.UpdateBuilder UpdateBuilder{get{return new User.UpdateBuilder(this.Id);}}
        ```
        
  *以上代码还应用了特性：JsonIgnore ，表示，该外键在对象进行 json 序列化的时候选择忽略该属性*



#### MyStaging 命名空间

*  **MyStaging.Common**  主要定义公共类型和资源
* **MyStaging.Helpers**     数据库操作帮助类、连接池管理工具类、DAL操作辅助函数
* **MyStaging.Mapping**  动态对象生成管理、实体对象映射定义

**MyStaging.Helpers.QueryContext**
    DAL继承的基类，该类实现了所有对数据库操作的封装，可直接继承使用，如果使用脚手架附带的构建工具，直接进行业务编写即可

---

**数据库操作**


* **插入记录**

``` C#
    UserModel user = new UserModel();
    user.Id = Guid.NewGuid();
    user.Login_name = "test@gmail.com";
    User.Insert(user);
```


* **修改记录**

``` C#
    // 自动根据主键修改
    UserModel user = new UserModel();
    user.UpdateBuilder.SetLogin_time(DateTime.Now).SaveChange(); 

    // 自定义条件修改
    user.UpdateBuilder.SetLogin_time(DateTime.Now).Where(f => f.Sex == true).SaveChange();

    // 直接修改
    User.Update(Guid.Empty).SetLogin_time(DateTime.Now).Where(f => f.Sex == true).SaveChange();

    // 自定义条件的直接修改
    User.UpdateBuilder.SetLogin_time(DateTime.Now).Where(f => f.Id == Guid.Empty).Where(f => f.Sex == true).SaveChange();
```


* **删除记录**

``` C#
    // 根据主键删除
    User.Delete(Guid.Empty);
    // 根据条件删除
    User.DeleteBuilder.Where(f => f.Id == Guid.Empty).Where(f => f.Sex == true).SaveChange();
```


* **查询单条记录**

``` C#
    UserModel user = User.Context.Where(f => f.Login_name == "test@gmail.com").ToOne();
```



* **指定查询列**

``` C#
    UserModel user = User.Context.Where(f => f.Login_name == "test@gmail.com").ToOne("id","login_name");
```


* **指定查询返回类型**

``` C#
    public class UserModel{
        public string Login_name{get;set;}
        public Guid Id{get;set;}
    }
    UserModel user = User.Context.Where(f => f.Login_name == "test@gmail.com").ToOne<UserModel>("id","login_name");
```


* **查询列表**

``` C#
    List<UserModel> list = User.Context.ToList();
    List<UserModel> list = User.Context.Where(f => f.Login_name == "test@gmail.com").ToList();    

    public class UserModel{
        public string Login_name{get;set;}
        public Guid Id{get;set;}
    }

    List<UserModel> list = User.Context.ToList<UserModel>("id","login_name");

```


* **表连接查询**

``` C#
    public class UserModel{
        public string Login_name{get;set;}
        public Guid Id{get;set;}
    }

    List<UserModel> list = User.Context.Union<TopicModel>("b",UnionType.INNER_JOIN,(a,b)=>a.Id==b.User_Id).Where(a=>a.Id=Guid.Empty).Where<TopicModel>(b=>b.Publish==true).ToList<UserModel>("id","login_name");

```


* **分页**

``` C#
    User.Context.Where(f => f.Login_name == "test@gmail.com").OrderBy(f=>f.State).Page(1,10);
```


* **排序**

``` C#
    User.Context.Where(f => f.Login_name == "test@gmail.com").OrderBy(f=>f.State);
    User.Context.Where(f => f.Login_name == "test@gmail.com").OrderDescing(f=>f.State);
```


* **聚合查询**

``` C#
    User.Context.Where(f => f.Login_name == "test@gmail.com").Avg(f=>f.Age);
    User.Context.Where(f => f.Login_name == "test@gmail.com").Sum(f=>f.Blance);
    // Max,Min,GroupBy,Having
```



* **事务**

``` C#
     PgSqlHelper.Transaction(() =>
        {
            UserModel user= User.Context.Where(f => f.Login_name == "test@gmail.com").ToOne();
            user.UpdateBuilder.SetLogin_time(DateTime.Now).SaveChange();
        });
```