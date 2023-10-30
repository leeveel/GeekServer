# GeekServer 介绍：

GeekServer 是一个开源的[分区分服](https://mp.weixin.qq.com/s?__biz=MzI3MTQ1NzU2NA==&mid=2247483884&idx=1&sn=3547c769a300f1d82cc04e9b1852c6d5&chksm=eac0cd9fddb7448997e38a74e2d26bde259cd2127583e31bc488511bc1fdcd9f35caff27d4a3&scene=21#wechat_redirect)的游戏服务器框架，采用 C# .Netcore 开发，开发效率高，性能强，跨平台，并内置不停服热更新机制。可以满足绝大部分游戏类型的需求，特别是和 Unity3D 协同开发更佳。  
**设计理念:大道至简，以简化繁**

# GeekServer 功能：

### 1.跨平台

使用 C# .Netcore 开发（可以跨平台，可以跨平台，可以跨平台），.Netcore 现在功能和性能都已经十分强大和稳健，不管是在 windows 还是 linux 上部署起来都很简便。

### 2.全面异步编程

全部采用异步编程（async/await），让逻辑代码变得整洁优雅，清晰易懂，让代码写起来行如流水。

### 3.TPL(Task Parallel Library) Actor 模型

GeekServer 的 Actor 模型构建于强大的 TPL DataFlow 之上，让 Actor 模型如虎添翼。（不了解 Actor 模型，可以搜一下相关资料，Akka，Orleans 都是采用的 Actor 模型）[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/1.Actor%E6%A8%A1%E5%9E%8B.md)

### 4.Actor 入队透明化

GeekServer 内部会自动处理线程上下文, 编译期间会通过[Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)自动生成入队代码, 开发人员无需关心多线程以及入队逻辑, 只需要像调用普通函数一样书写逻辑。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/Actor%E5%85%A5%E9%98%9F.md)

### 5.Actor 死锁检测

Actor 模型本身是存在死锁的情况，且不容易被发现。GeekServer 内部可检测环路死锁(即:A->B->C->A)，并采用调用链重入机制消除环路死锁。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/1.Actor%E6%A8%A1%E5%9E%8B.md)

### 6.支持不停服更新

采用组件+状态的设计，状态只有属性，没有方法，组件只用方法，没有属性，并通过代理的方式全部放到热更 dll 中，运行时重新加载 dll 即可热更所有逻辑。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/%E7%83%AD%E6%9B%B4hotfix.md)

### 7.网络模块

网络模块替换了原来的 DotNetty，采用 Asp.Net 的默认服务器 Kestrel，支持协议多（Tcp，udp,Http123,websocket，signalr 等），而且性能比 dotnetty 高很多[了解更多](<https://github.com/leeveel/GeekServer/blob/main/Docs/%E7%BD%91%E7%BB%9CNet(tcp%26http).md>)

### 8.持久化透明

采用 Nosql 作为数据存储，状态的持久化全透明，框架会自动序列化/反序列,让开发人员更加专注于业务逻辑，无需操心数据库。 [了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/2.Actor%26Component%26State.md)

### 9.Timer/Scheduler/Event

内置线程安全的 Timer，Scheduler，Event 系统，轻松应对游戏服务器常见的定时，任务计划，事件触发等业务需求。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/%E4%BA%8B%E4%BB%B6Event-timer.md)

### 10.定期释放不活跃内存数据

以功能系统级别的粒度，定期剔除内存中不活跃的玩家数据，尽最大可能减少服务器内存开销。

### 11.高效的通信协议(基于 MessagePack)

[Geek.MsgPackTool](https://github.com/leeveel/Geek.MsgPackTool) [MessagePack]对多态支持不够友好，GeekServer 提供了工具来生成多态注册信息，序列化和反序列化效率极高，同时序列化之后的数据极小，数据传输效率很高。[了解更多](https://github.com/leeveel/GeekServer/blob/main/Docs/%E5%85%B3%E4%BA%8E%E5%8D%8F%E8%AE%AE.md)

### 12.一键导表工具(GeekConfig)

[GeekConfig](https://github.com/leeveel/GeekConfig)是一个一键导表工具，将策划配置表，转化为二进制数据，并提供了方便快捷的 API 供游戏调用

# 运行

1. 安装[.Net 7.x](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
2. 安装[mongodb](https://www.mongodb.com/try/download/community)
3. 打开 git clone 本项目https://github.com/leeveel/GeekServer.git
4. 运行 Tools/ExcelGen/ExcelToCode.exe 点击[服务器-ALL]导出配置表
5. 用 VisualStudio2022 打开 GeekServer.sln 启动 GeekServer.App
6. 启动 GeekServer.Test (一个 1000 人登录的 demo)
7. 打开 UnityDemo 工程，打开 SampleScene，运行查看日志(**检查 Main Camera 上是否有脚本丢失，如果有请挂载 GameMain.cs**)

# 文档&例子&Demo

[十分钟上手教程](https://github.com/leeveel/GeekServer/blob/main/Docs/%E5%8D%81%E5%88%86%E9%92%9F.md)  
[了解更多](https://github.com/leeveel/GeekServer/tree/master/Docs)

群友分享：  
[快速了解 GeekServer(Space_Dark 写)](https://blog.csdn.net/weixin_45394846/article/details/129265794?csdn_share_tail=%7B%22type%22%3A%22blog%22%2C%22rType%22%3A%22article%22%2C%22rId%22%3A%22129265794%22%2C%22source%22%3A%22weixin_45394846%22%7D)

# 代码片段

```c#
//采用注解注册Actor组件
[Comp(ActorType.Role)]
public class BagComp : StateComp<BagState>{}

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

GeekServer 有严格的书写规范检查，如不符合规范编译直接报错  
1.CompAgent 不能被二次继承，Agent 继承的需求理论上很少，如果有请采用组合模式  
2.为 CompAgent 中需要被外部提供服务的接口，添加【Service】注解  
3.CompAgent 中非【Threadsafe】的【Service】接口只能是异步函数  
4.CompAgent 中不要书写构造函数,重写 Active 函数来完成初始化工作  
5.大部分情况下你都应该使用 await 等待来书写逻辑，不需要等待的方法请加上【Discard】注解，如：通知全服玩家，就没必要等待一个通知完成后再通知下一个。 同时[Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)在编译期间对标记了【Discard】的函数做了处理，内部直接返回了 Task.CompletedTask，所以外部使用下划线丢弃或是用 await 都是等价的，为了规范统一，可以全部使用 await。**这样有个好处，就是可以在编译期间检查所有 Agent 中的代码，如有发现使用了弃元运算符(\_ = DoSomething())则提示代码编写不符合规范。**

```c#
public Task NotifyAllClient()
{
   for(int i=0; i<clients.count; i++)
   {
     //_ = NotifyOneClient(clients[i].roleId);
	 //对于标记了[Discard]的函数，等价于上面一行代码
	 await NotifyOneClient(clients[i].roleId);
   }
}

[Service]
[Discard]
public virtual Task NotifyOneClient(long roleId)
{
   //...
   //...
}
```

5.CompAgent 中为需要提供给外部访问接口，标记注解，如果不加外部又有访问，**则会有线程安全问题**，除非此接口本身就是线程安全的(标记了[ThreadSafe]注解)。

```c#
public class ServerCompAgent : StateCompAgent<ServerComp, ServerState>
{
    private Task TestScheduleTimer()
    {
        LOGGER.Debug("ServerCompAgent.TestSchedueTimer.延时1秒执行.每隔3秒执行");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 由于此接口会提供给其他Actor访问，所以需要标记为[Service]
    /// </summary>
    /// <returns></returns>
    [Service]
    public virtual Task<int> GetWorldLevel()
    {
        return Task.FromResult(State.WorldLevel);
    }

}
```

更多异步书写规范请参考微软官方文档[AsyncGuidance.md](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)

# 推荐项目

[GeekConfig](https://github.com/leeveel/GeekConfig) 一键从 Excel 中导出模板代码和二进制数据
