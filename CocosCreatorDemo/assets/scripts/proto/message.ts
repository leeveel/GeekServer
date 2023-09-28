import * as msgpack from "../../msgpack";
import * as Proto from "./gen/proto";

export interface Message {
  MsgID: number;
  UniId: number;
}

export function EncodeMsg(msg: Message): any {
  return msgpack.encode([msg.MsgID, msg], {
    useBigInt64: true,
    ignoreUndefined: true,
  });
}

export function DecodeMsg(data: any): Message {
  var arr = msgpack.decode(data) as Array<any>;
  var msg = arr[1] as Message;
  msg.MsgID = arr[0]; 
  return msg;
}
