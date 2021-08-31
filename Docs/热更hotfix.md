# 热更新
GeekServer支持不停服热更新逻辑。
#### 热更思路
游戏中的所有状态放在App工程中，始终存在于内存，不能热更。Actor和Component的逻辑使用代理模式（Agent）放到Hotfix工程。热更时载入新的dll（GeekServer.Hotfix.dll），清除所有老的Agent，所有新逻辑重新从Actor/Component获取新的Agent汇入新dll中执行热更后的逻辑，达到热更目的。正在执行老dll逻辑的代码获取的Agent依然来自热更前的老Dll中，等待老dll中的逻辑执行完后清理掉内存中老的dll。底层使用接口驱动热更dll中的逻辑。
需要注意的是，热更时新的dll需要放在新的目录下面，然后再载入内存，因为老的dll可能正在运行，是无法直接覆盖的。参考代码[HotfixModule.Load](https://github.com/leeveel/GeekServer/tree/master/GeekServer.Core/Hotfix/HotfixModule.cs)
#### 可以热更部分
可以热更的逻辑都应该放在GeekServer.Hotfix工程中
1. 所有Actor/Component的Agent，Agent中只有逻辑没有状态，状态全部放到Component的State
2. HttpHandler
3. TcpHandler
4. 协议
5. 配置表/配置表代码

#### 热更新流程
1. 游戏后台将新的GeekServer.Hotfix.dll及相关文件（对应pdb，json等）拷贝到游戏服特定目录下
2. 游戏后台向游戏服发送http命令，通知进行热更，并告知dll目录，md5等信息
3. 游戏服中热更HttpHandler根据后台信息，验证热更dll完整性，合法性，修改dllVersion.txt，发起热更调用