
__QQ交流群 : 228688193__  

# GeekServer介绍：
GeekServer是一个开源的[分区分服](https://mp.weixin.qq.com/s?__biz=MzI3MTQ1NzU2NA==&mid=2247483884&idx=1&sn=3547c769a300f1d82cc04e9b1852c6d5&chksm=eac0cd9fddb7448997e38a74e2d26bde259cd2127583e31bc488511bc1fdcd9f35caff27d4a3&scene=21#wechat_redirect)的游戏服务器框架，采用C# .Netcore开发，开发效率高，性能强，跨平台，并内置不停服热更新机制，支持http tcp。可以满足绝大部分游戏类型的需求，特别是和Unity3D协同开发更佳。  [分布式分支火热进行中。。。](https://github.com/leeveel/GeekServer/tree/distributed)  
__设计理念:大道至简，以简化繁__

# GeekServer功能：

### 1.跨平台  
使用C# .Netcore开发（可以跨平台，可以跨平台，可以跨平台），.Netcore现在功能和性能都已经十分强大和稳健，不管是在windows还是linux上部署起来都很简便。
### 2.全面异步编程  
全部采用异步编程（async/await），让逻辑代码变得整洁优雅，清晰易懂，让代码写起来行如流水。
### 3.TPL(Task Parallel Library) Actor模型  
GeekServer的Actor模型构建于强大的TPL DataFlow之上，让Actor模型如虎添翼。（不了解Actor模型，可以搜一下相关资料，Akka，Orleans都是采用的Actor模型）[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/1.Actor%E6%A8%A1%E5%9E%8B.md)
### 4.Actor入队透明化  
GeekServer内部会自动处理线程上下文, 编译期间会通过[Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)自动生成入队代码, 开发人员无需关心多线程以及入队逻辑, 只需要像调用普通函数一样书写逻辑。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/Actor%E5%85%A5%E9%98%9F.md)
### 5.Actor死锁检测 
Actor模型本身是存在死锁的情况，且不容易被发现。GeekServer内部可检测环路死锁(即:A->B->C->A)，并采用调用链重入机制消除环路死锁。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/1.Actor%E6%A8%A1%E5%9E%8B.md)
### 6.支持不停服更新 
采用组件+状态的设计，状态只有属性，没有方法，组件只用方法，没有属性，并通过代理的方式全部放到热更dll中，运行时重新加载dll即可热更所有逻辑。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/%E7%83%AD%E6%9B%B4hotfix.md)
### 7.网络模块  
网络模块采用了DotNetty，DotNetty是微软Azure团队，使用C#实现的Netty的版本，性能强劲，功能强大。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/%E7%BD%91%E7%BB%9CNet(tcp%26http).md)
### 8.持久化透明  
采用Mongodb作为数据存储，状态的持久化全透明，框架会自动序列化/反序列（并在编译期通过[Source Generator]自动生成检测代码，高效过滤非变化的状态）让开发人员更加专注于业务逻辑，无需操心数据库。 [了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/2.Actor%26Component%26State.md)
### 9.Timer/Scheduler/Event  
内置线程安全的Timer，Scheduler，Event系统，轻松应对游戏服务器常见的定时，任务计划，事件触发等业务需求。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/%E4%BA%8B%E4%BB%B6Event-timer.md)
### 10.定期释放不活跃内存数据  
以功能系统级别的粒度，定期剔除内存中不活跃的玩家数据，尽最大可能减少服务器内存开销。
### 11.高效的通信协议(GeekProto)  
[GeekProto](https://github.com/leeveel/GeekProto)以扁平数据结构的xbuffer为基础（flatbuffer的简化版），序列化和反序列化效率极高，同时序列化之后的数据极小，数据传输效率很高。(已验证支持ILRunTime热更)[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/%E5%85%B3%E4%BA%8E%E5%8D%8F%E8%AE%AE.md)
### 12.一键导表工具(GeekConfig)  
[GeekConfig](https://github.com/leeveel/GeekConfig)是一个一键导表工具，将策划配置表，转化为二进制数据，并提供了方便快捷的API供游戏调用   

# 运行
1. 安装[.NetCore3.1](https://dotnet.microsoft.com/download/dotnet/3.1)
2. 安装[mongodb4.x](https://www.mongodb.com/try/download/community)
3. 打开git clone本项目https://github.com/leeveel/GeekServer.git
4. 运行Tools/ExcelGen/ExcelConverter.exe 点击[服务器-ALL]导出配置表
5. 用VisualStudio2019打开GeekServer.sln 启动GeekServer.App
6. 启动GeekServer.Test (一个1000人登录的demo)

# 文档&例子&Demo
[文档](https://github.com/leeveel/GeekServer/tree/master/Docs)  

# 代码片段
```c#
//注册Actor组件
RegistServerComp<ServerComp>(EntityType.Server); //server
RegistServerComp<LoginComp>(EntityType.Login);  
RegistRoleComp<RoleComp>(); //role
RegistRoleComp<BagComp>();

//调用Actor组件函数(就像调用普通函数一样,无需关心多线程或入队)
var serverComp = await EntityMgr.GetCompAgent<ServerCompAgent>(ActorType.Server);
await serverComp.CheckCrossDay();

//定义状态(数据)
public class RoleState : DBState
{
    public string RoleName { get; set; }
    public long RoleId { get; set; }
    ...
}
//绑定组件
public class RoleComp : StateComponent<RoleState>{}
//绑定组件Agent(Agent类逻辑可全部热更新)
public class RoleCompAgent : StateComponentAgent<RoleComp, RoleState>{}
```
# 最佳实践
GeekServer有严格的书写规范检查，如不符合规范编译直接报错  
1.CompAgent不能被二次继承，Agent继承的需求理论上很少，如果有请采用组合模式  
2.CompAgent中的所有方法必须以Task为返回值（理由：全面异步，防止出现async void导致异常无法捕捉，致使程序崩溃，详见微软AsyncGuidance.md）  
3.CompAgent中不能书写构造函数（实际上也没有这样的需求）  
4.大部分情况下你都应该使用await等待来书写逻辑，不需要等待的方法请加上[NotAwait]注解，如：通知全服玩家，就没必要等待一个通知完成后再通知下一个。  同时[Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)在编译期间对标记了[NotAwait]的函数做了处理，内部直接返回了Task.CompletedTask，所以外部使用**_**丢弃或是用await都是等价的，为了规范统一，可以全部使用await
```c#
public Task NotifyAllClient()
{
   for(int i=0; i<clients.count; i++)
   {
     //_ = NotifyOneClient(clients[i].roleId);
	 //对于标记了[NotAwait]的函数，等价于上面一行代码
	 await NotifyOneClient(clients[i].roleId);
   }
}

[NotAwait]
public virtual Task NotifyOneClient(long roleId)
{
   //...
   //...
}
```
5.CompAgent中不需要提供给外部访问方法，请尽量声明为非public类型，这样会少一次入队判断，效率会更高 
```c#
public class RoleLoginCompAgent : StateComponentAgent<RoleLoginComp, RoleInfoState>
{
	//agent内部调用，非public
	private Task OnCreate(ReqLogin reqLogin, long roleId)
	{
	    State.CreateTime = DateTime.Now;
	    State.Level = 1;
	    State.VipLevel = 1;
	    State.RoleId = roleId;
	    State.RoleName = new System.Random().Next(1000, 10000).ToString();//随机给一个
	    return Task.CompletedTask;
	}
        //外部接口public
	public async Task OnLogin(ReqLogin reqLogin, bool isNewRole, long roleId)
	{
	    if (isNewRole)
	    {
		await OnCreate(reqLogin, roleId);
	    }
	    State.LoginTime = DateTime.Now;
	}
}
```
6.为明确知道为线程安全的函数加上[ThreadSafe]注解，同样可以减少入队判断提高效率。（这个是非必须的操作，不加也不会有问题，如果你不知道什么线程安全忽略此项） 
```c#
public class RoleLoginCompAgent : StateComponentAgent<RoleLoginComp, RoleInfoState>
{
	[ThreadSafe] //我是线程安全的函数
	public Task<ResLogin> BuildLoginMsg()
        {
            var res = new ResLogin()
            {
                code = 0,
                userInfo = new UserInfo()
                {
                    createTime = State.CreateTime.Ticks,
                    level = State.Level,
                    roleId = State.RoleId,
                    roleName = State.RoleName,
                    vipLevel = State.VipLevel
                }
            };
            return Task.FromResult(res);
        }
}
```
更多异步书写规范请参考微软官方文档[AsyncGuidance.md](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)  

# 推荐项目  
[xbuffer](https://github.com/CodeZeg/xbuffer) 一种简化版本的 flatbuffer 序列化库  
[GeekConfig](https://github.com/leeveel/GeekConfig) 一键从Excel中导出模板代码和二进制数据  
[GeekProto](https://github.com/leeveel/GeekProto) Super Fast Binary Serialization Library  
