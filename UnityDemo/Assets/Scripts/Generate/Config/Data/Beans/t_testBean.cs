/**
 * Auto generated, do not edit it
 */

namespace Geek.Client.Config
{
	///<summary>测试表</summary>
    public class t_testBean : BaseBin
    {
		///<summary>t_id为key</summary>
        public int t_id;
        private int m_t_mult;
		///<summary>textmult配置语言表中的id</summary>
        public string t_mult
		{
			get           
			{
				if(m_t_mult == 0)
					return "";
				t_languageBean lanBean = ConfigBean.GetBean<t_languageBean, int>(m_t_mult);
				if (lanBean != null)
					return lanBean.t_content;
				else
					return m_t_mult.ToString();
			}
		}
		///<summary>text配置字符串</summary>
        public string t_str;
		///<summary>text配置字符串</summary>
        public string t_str2;
		///<summary>默认为int</summary>
        public int t_int;
		///<summary>手动填int</summary>
        public int t_int2;
		///<summary>long类型</summary>
        public long t_long;

        public void LoadData(byte[] data, ref int offset)
        {
            t_id = XBuffer.ReadInt(data, ref offset);
            m_t_mult = XBuffer.ReadInt(data, ref offset);
			t_str = XBuffer.ReadString(data, ref offset);
			t_str2 = XBuffer.ReadString(data, ref offset);
            t_int = XBuffer.ReadInt(data, ref offset);
            t_int2 = XBuffer.ReadInt(data, ref offset);
            t_long = XBuffer.ReadLong(data, ref offset);
        }

    }
}
