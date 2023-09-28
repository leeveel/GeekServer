import { EncodeMsg, DecodeMsg, Message } from "./proto/message";
import * as Proto from "./proto/gen/proto";

enum NetState {
  Opening,
  Connected,
  Close,
}

var netstate: NetState = NetState.Close;
var ws: WebSocket;
var uid: number = 1;

export async function Connect(
  url: string,
  onOpen: Function,
  onMsg: Function,
  onClose: Function
) {
  netstate = NetState.Opening;
  ws = new WebSocket(url);
  ws.binaryType = "arraybuffer";

  ws.onopen = function (evt) {
    netstate = NetState.Connected;
    onOpen();
  };

  ws.onmessage = function (evt) {
    var msg = DecodeMsg(evt.data);
    onMsg(msg);
  };

  ws.onerror = function (evt) {
    netstate = NetState.Close;
    ws.close();
  };

  ws.onclose = function (evt) {
    netstate = NetState.Close;
    onClose();
  };
}

export function sendMsg(msg: any) {
  if (netstate == NetState.Connected) {
    msg.UniId = uid++;
    var data = EncodeMsg(msg);
    ws.send(data);
  }
}

export function close() {
  if (netstate != NetState.Close) {
    ws.close();
  }
}
