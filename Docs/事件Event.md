# 事件
GeekServer根据热更新设计方案，事件采用接口方式进行监听和回调。事件的监听根据泛型参数解析Actor类型，并在Actor.Active后调用。监听或者回调事件方法，自定义类型继承EventListener，实现InitListener初始化监听，实现HandleEvent进行回调处理。需要注意的是，EventListener的泛型参数需要是添加事件的Actor对应的Agent，否则会出现类型转换失败。
```csharp
class Hotfix_EL : EventListener<ServerActorAgent, ServerActorAgent>
{
	//已经派发到EvtDispatcher所有者线程
	protected override Task HandleEvent(ServerActorAgent agent, Event evt)
	{
		//your logic here
		return Task.CompletedTask;
	}

	//对应Actor类型的Active时自动注册
	protected override Task InitListener(ServerActorAgent actor)
	{
		actor.Actor.EvtDispatcher.AddListener(EventID.HotfixEnd, this);
		return Task.CompletedTask;
	}
}
```