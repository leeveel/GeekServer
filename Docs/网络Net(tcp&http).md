# 网络
GeekServer网络层使用kestrel，GeekServer目前只接入了tcp和http，但是kestrel是支持UDP，websocket，Http123，singalr等几乎所有的常见协议的，如有所需请自行接入（工作量很小）。

### donetty vs kestrel
![](https://github.com/leeveel/GeekServer/blob/main/Docs/imgs/dotnetty.png)
![](https://github.com/leeveel/GeekServer/blob/main/Docs/imgs/kestrel.png)  

### http
创建任意脚本名字Handler，继承BaseHttpHandler，使用HttpMsgMapping标记指令，实现Action函数即可使用。Action中调用或者实现对应指令的逻辑，并返回结果给客户端。此外httpHandler还有2个字段可以重写标示是否验证，CheckSign是否验证，Inner是否使用内部验证方式。GeekServer支持post和get方式访问。
###### 特殊key不要占用
1. cmd：指令，自己定义的任意字符串
2. time：用于安全验证，当前utc时间tick（DateTime.UtcNow.Ticks），同时也做时间校验，客户端时间需要和服务器时间基本一致(超前/延后10秒左右)
3. sign：用于安全验证，安全验证算法后的结果
###### 安全验证算法
验证算法见[BaseHttpHandler](https://github.com/leeveel/GeekServer/tree/master/GeekServer.Core/Net/Http/BaseHttpHandler.cs) GetStringSign函数
sign = 算法函数(code + time)，其中sign和time在客户端发送到服务器的参数里面，code是[server_config.json](https://github.com/leeveel/GeekServer/tree/master/GeekServer.App/Config/server_config.json) 中配置的HttpCode(非inner验证)/HttpInnerCode(inner验证) 
注意，上线的商业项目记得修改HttpCode和HttpInnerCode，否则可能被他人利用。
一般的，非Inner验证用于提供给第3放使用(运营/sdk。。。)，Inner验证用于内部游戏后台（发邮件，封号。。。）
```csharp
//在浏览器中访问地址即可验证：http://localhost:20000/geek/server/logic?cmd=test
//带安全验证的url：http://localhost:20000/geek/server/logic?cmd=test&time=637574338045380003&sign=ad762402fc9c5275bbb68464cf30aac888
[HttpMsgMapping("test")] //指令为test
public class TestHttpHandler : BaseHttpHandler
{
	//不使用内部验证算法
	public override bool Inner => false;
	//是否验证参数，debug模式下始终不验证（debug模式在server_config.json中修改）
	public override bool CheckSign => false;
	public override Task<string> Action(string ip, string url, Dictionary<string, string> paramMap)
	{
		return Task.FromResult((string)HttpResult.Success);
	}
}
```

### tcp
GeekServer的tcp协议为自定义协议，区别于google的protocolBuffer和flatBuffer，但是序列化和反序列化都要优于2者。
###### tcp使用步骤
1. [协议工具目录](https://github.com/leeveel/GeekServer/tree/master/Tools/GeekProto)
2. 运行Geek.Proto.exe生成协议导
3. 创建任意脚本名字Handler，继承FixedIdEntityHandler/TcpCompHandler，使用MsgMapping标记消息类型，实现ActionAsync函数即可。BaseTcpHandler中的Msg即为解析好的当前类型协议数据
```csharp
[MsgMapping(typeof(ReqBagInfo))]
public class ReqBagInfoHandler : TcpCompHandler<BagCompAgent>
{
	public override Task ActionAsync()
	{
		var req = (ReqBagInfo)Msg;
		//your logic here...
		return Task.CompletedTask;
	}
}
```

###### [GeekProto协议](https://github.com/leeveel/GeekServer/blob/main/Docs/%E5%85%B3%E4%BA%8E%E5%8D%8F%E8%AE%AE.md)
