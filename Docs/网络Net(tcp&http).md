# 网络
GeekServer网络层接入DotNetty，目前只支持tcp和http，GeekServer进行了深度包装，tcp和http都开发者来说用起来都非常简单。

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
1. 在[协议目录](https://github.com/leeveel/GeekServer/tree/master/Tools/MessageGen/messages) 创建一个xml格式的协议文件。
2. 运行MessageGen/msg.bat生成协议导[项目中](https://github.com/leeveel/GeekServer/tree/master/GeekServer.Hotfix/Generate/Messages)
3. 创建任意脚本名字Handler，继承BaseTcpHandler/TcpActorHandler，使用TcpMsgMapping标记消息类型，实现ActionAsync函数即可,如果继承制TcpActorHandler还需实现CacheActor函数。BaseTcpHandler中的Msg即为解析好的当前类型协议数据
```csharp
[TcpMsgMapping(typeof(Message.Sample.ReqTest))]
public class SampleTcpHandler : BaseTcpHandler
{
	public override Task ActionAsync()
	{
		var req = (Message.Sample.ReqTest)Msg;
		//your logic here...
		return Task.CompletedTask;
	}
}
```

###### tcp协议
1. 使用模板代码自动生成，类似protocolBuffer，生成后的代码不能修改
2. 分为结构和消息，消息相对结构多1个msgId；一般的，消息和结构可包含结构，消息和结构不可包含消息
3. 协议支持的数据结构：int,long,bool,string,short,float,double,byte[],list,map,任意自定义结构
4. 支持引用其他协议文件类型，支持单协议文件内继承，不支持跨文件继承
5. 支持兼容模式，如协议修改后，搭载新协议的服务器支持使用老协议的客户端（有兼容限制）
6. 支持optional
7. 其他限制
	string/list/map最大长度32767(short.max)
	单个消息/结构最多255个字段，单个文件最多255个消息/结构(byte.max)
	字段消息命名不能以下划线结尾（可能冲突引发编译错误）
	map的key只支持short, int, long, string；list/map不能optional，list不能嵌套list/map，map不能嵌套list/map(可使用结构多装一层)
8. 其他兼容限制（如果每次服务器和客户端同步升级可忽略此条）
	字段只能添加，添加后不能删除，添加字段只能添加到最后，添加消息类型只能添加到最后
	不能修改字段类型（如从bool改为long）
9. 协议配置方法参考[Sample](https://github.com/leeveel/GeekServer/tree/master/Tools/MessageGen/messages/Sample.xml)
10. 根据项目需要可适当修改[协议生成模板](https://github.com/leeveel/GeekServer/Tools/MessageGen/template)
11. 已有线上项目验证支持ILRunTime热更。如果客户端没有使用C#，需要自行书写模板/模板工具，模板工具使用[Scriban](https://github.com/scriban/scriban) 开发