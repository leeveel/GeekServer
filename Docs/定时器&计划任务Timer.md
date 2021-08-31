# 定时器&计划任务
GeekServer中没有使用传统意义中的Update，除MMO项目，其他大部分游戏类型的服务器基本没有必要使用Update，需要Update的模块添加一个Timer也可以实现
根据热更新设计方案，定时器和计划任务采用接口方式进行回调，任务使用扩展方法实现，[源码参考](https://github.com/leeveel/GeekServer/tree/master/GeekServer.Hotfix/Logic/Common/TimerExt.cs)
定时器支持：1次性delay,周期性timer。
计划任务：指定时间1次性任务，每天任务，每周任务，每周几任务，每月任务。
GeekServer中没有对定时器&计划任务做持久化，所以你可能需要在开服后、玩家上线或者Component激活时考虑一下计划任务逻辑是否需要被处理了。
回调函数继承TimerHandler<>，重写HandleTimer，里面处理定时器回调逻辑即可。
需要注意的是定时器是接入的Quartz，由于硬件精度问题（windows时间实际精度为10毫秒左右），回调时间可能会提前1-2毫秒，如果对时间依赖特别大的可能需要特殊处理下，比如在Timer回调后延时50毫秒再执行回调逻辑。