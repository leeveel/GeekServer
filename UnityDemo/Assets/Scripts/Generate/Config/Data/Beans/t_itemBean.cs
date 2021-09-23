/**
 * Auto generated, do not edit it
 */

namespace Geek.Client.Config
{
	///<summary>用于测试的道具表</summary>
    public class t_itemBean : BaseBin
    {
		///<summary>道具id</summary>
        public int t_id;
		///<summary>名字</summary>
        public string t_name;
		///<summary>是否可以出售</summary>
        public int t_can_sell;
		///<summary>是否在背包显示</summary>
        public int t_show;
		///<summary>出售价格(金币)</summary>
        public string t_sell_num;
		///<summary>描述</summary>
        public string t_desc;
		///<summary>道具类型(0=普通(不能使用)1=宝箱2=经验丹)</summary>
        public int t_use_type;
		///<summary>参数</summary>
        public string t_param;

        public void LoadData(byte[] data, ref int offset)
        {
            t_id = XBuffer.ReadInt(data, ref offset);
			t_name = XBuffer.ReadString(data, ref offset);
            t_can_sell = XBuffer.ReadInt(data, ref offset);
            t_show = XBuffer.ReadInt(data, ref offset);
			t_sell_num = XBuffer.ReadString(data, ref offset);
			t_desc = XBuffer.ReadString(data, ref offset);
            t_use_type = XBuffer.ReadInt(data, ref offset);
			t_param = XBuffer.ReadString(data, ref offset);
        }

    }
}
