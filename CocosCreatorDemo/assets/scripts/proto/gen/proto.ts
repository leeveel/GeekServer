
import {Message} from "../message"


    export enum TestEnum
    {
        A, B, C, D, E, F, G, H, I, J, K, L,
    }

export class ReqBagInfo implements Message {
    MsgID = 1435193915; 
    UniId: number= 0;
}
export class ResBagInfo implements Message {
    MsgID = -1872884227; 
    UniId: number= 0;
    // @ts-ignore
    public ItemDic:Map<number,number>=new Map();
}
export class ReqComposePet implements Message {
    MsgID = 225320501; 
    UniId: number= 0;
    // @ts-ignore
    public FragmentId:number;
}
export class ResComposePet implements Message {
    MsgID = 750865816; 
    UniId: number= 0;
    // @ts-ignore
    public PetId:number;
}
export class ReqUseItem implements Message {
    MsgID = 1686846581; 
    UniId: number= 0;
    // @ts-ignore
    public ItemId:number;
}
export class ReqSellItem implements Message {
    MsgID = -1395845865; 
    UniId: number= 0;
    // @ts-ignore
    public ItemId:number;
}
export class ResItemChange implements Message {
    MsgID = 901279609; 
    UniId: number= 0;
    // @ts-ignore
    public ItemDic:Map<number,number>=new Map();
}
export class NetConnectMessage implements Message {
    MsgID = 667869091; 
    UniId: number= 0;
}
export class NetDisConnectMessage implements Message {
    MsgID = 1245418514; 
    UniId: number= 0;
}
export class TestStruct  {
    // @ts-ignore
    public Age:number;
    // @ts-ignore
    public Name:string;
}
export class A  {
    // @ts-ignore
    public Age:number;
    // @ts-ignore
    public E:TestEnum;
    // @ts-ignore
    public TS:TestStruct;
}
export class B extends A {
    // @ts-ignore
    public Name:string;
    // @ts-ignore
    public Test:string;
}
export class UserInfo  {
    // @ts-ignore
    public RoleName:string;
    // @ts-ignore
    public RoleId:number;
    // @ts-ignore
    public Level:number;
    // @ts-ignore
    public CreateTime:number;
    // @ts-ignore
    public VipLevel:number;
}
export class ReqLogin implements Message {
    MsgID = 1267074761; 
    UniId: number= 0;
    // @ts-ignore
    public UserName:string;
    // @ts-ignore
    public Platform:string;
    // @ts-ignore
    public SdkType:number;
    // @ts-ignore
    public SdkToken:string;
    // @ts-ignore
    public Device:string;
}
export class ResLogin implements Message {
    MsgID = 785960738; 
    UniId: number= 0;
    // @ts-ignore
    public Code:number;
    // @ts-ignore
    public UserInfo:UserInfo;
}
export class ResLevelUp implements Message {
    MsgID = 1587576546; 
    UniId: number= 0;
    // @ts-ignore
    public Level:number;
}
export class HearBeat implements Message {
    MsgID = 1575482382; 
    UniId: number= 0;
    // @ts-ignore
    public TimeTick:number;
}
export class ResErrorCode implements Message {
    MsgID = 1179199001; 
    UniId: number= 0;
    // @ts-ignore
    public ErrCode:number;
    // @ts-ignore
    public Desc:string;
}
export class ResPrompt implements Message {
    MsgID = 537499886; 
    UniId: number= 0;
    // @ts-ignore
    public Type:number;
    // @ts-ignore
    public Content:string;
}
