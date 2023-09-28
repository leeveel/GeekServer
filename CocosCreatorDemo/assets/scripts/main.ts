import { _decorator, Component, RichText } from "cc";
import * as Net from "./Net";
import * as Proto from "./proto/gen/proto";
import { Message } from "./proto/message";

const { ccclass, property } = _decorator;

let logObj: RichText | null;
let logArray: string = "";
function log(info: string) {
  logArray += info + "\n";
  (logObj as RichText).string = logArray;
  console.log(info);
}

@ccclass("main")
export class main extends Component {
  start() {
    logObj = this.node.getComponent(RichText);
    var url = "ws://localhost:443/ws";
    log("开始连接websocket server :" + url);
    Net.Connect(url, this.onConnect, this.onMsg, this.onClose);
  }

  onConnect() {
    log("连接成功...");
    var msg = new Proto.ReqLogin();
    msg.UserName = "test";
    msg.Device = "chrome";
    msg.Platform = "android";
    msg.SdkType = 1;

    log("发送登录消息:" + JSON.stringify(msg));
    Net.sendMsg(msg);
  }

  onMsg(msg: Message) {
    log("收到消息:" + JSON.stringify(msg));
  }

  onClose() {}

  update(deltaTime: number) {}
}
