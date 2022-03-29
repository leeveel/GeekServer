# 事件
GeekServer根据热更新设计方案，事件采用接口方式进行监听和回调。监听或者回调事件方法，自定义类型继承EventListener，实现InitListener初始化监听，实现HandleEvent进行回调处理。需要注意的是，EventListener的泛型参数需要是添加事件的Entity对应的Agent，否则会出现类型转换失败。
```csharp
class RoleEH : EventListener<RoleCompAgent>
{
    protected override async Task HandleEvent(RoleCompAgent agent, Event evt)
    {
        switch (evt.EventId)
        {
            case (int)EventID.OnDisconnected:
                await agent.OnDisconnected();
                break;
            case (int)EventID.OnMsgReceived:
                await agent.OnMsgReceived();
                break;
        }
    }

    public override Task InitListener(long entityId)
    {
        GED.AddListener<RoleEH>(entityId, EventID.OnDisconnected);
        GED.AddListener<RoleEH>(entityId, EventID.OnMsgReceived);
        return Task.CompletedTask;
    }
}
```

# Timer或Schedule
```csharp
public override async Task Active()
{
    await base.Active();
    this.Schedule<CorssDayTimerHandler>(ServerComp.CrossDayHour, 0);
}

class CorssDayTimerHandler : TimerHandler<ServerCompAgent>
{
    protected override async Task HandleTimer(ServerCompAgent agent, Param param)
    {
        //排除时间精度问题,Quartz可能产生1,2毫秒误差
        await Task.Delay(100);
        _ = agent.CheckCrossDay();
    }
}
```