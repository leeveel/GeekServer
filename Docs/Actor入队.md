## Actor入队透明 ##
GeekServer编译期间会自动注入入队代码(**AgentWeaver**), 开发人员无需入队逻辑, 只需要像调用普通函数一样书写逻辑。  
```c#
//编译期间会注入一个继承自xxxCompAgent的wrapper类,来实现自动入队
//同时SendAsync内部自动处理了线程上下文,开发者只需要像调用普通函数一样书写逻辑
public class ServerCompAgentWrapper : ServerCompAgent
{
	public override Task CheckCrossDay()
	{
		return base.Actor.SendAsync((Func<Task>)base.CheckCrossDay, isAwait: false, 10000);
	}

	public override Task<int> GetDaysFromOpenServer()
	{
		return base.Actor.SendAsync((Func<Task<int>>)base.GetDaysFromOpenServer, isAwait: true, 10000);
	}
}

var serverComp = await EntityMgr.GetCompAgent<ServerCompAgent>(ActorType.Server);
//使用方式(就像调用普通函数一样,无需关心多线程或入队)
_ = serverComp.CheckCrossDay();

```
## 线程上下文 ##
GeekServer内部会自动处理线程上下文，由RuntimeContext实现，主要用于环路调用检测，以及判断是否需要入队，其内部使用**AsyncLocal**实现
```c#
internal class RuntimeContext
{
    internal static long Current => callCtx.Value;
    internal static AsyncLocal<long> callCtx = new AsyncLocal<long>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetContext(long callChainId)
    {
        callCtx.Value = callChainId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ResetContext()
    {
        callCtx.Value = 0;
    }
}
```