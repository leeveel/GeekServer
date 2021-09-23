/**
 * Auto generated, do not edit it client
 * 语言表
 */
namespace Geek.Client.Config
{
    public class t_languageBean : BaseBin
    {

		public int t_id;
		public string t_content;

        public void LoadData(byte[] data, ref int offset)
        {
			string useField = "t_chinese";
            t_id = XBuffer.ReadInt(data, ref offset);

			if (string.IsNullOrEmpty(t_content) && useField == "t_chinese")
			{
				t_content = XBuffer.ReadString(data, ref offset); 
			}
			else
			{
				//不需要的语言字段跳过
				short slen = XBuffer.ReadShort(data, ref offset);
				offset += slen;
			}


			if (string.IsNullOrEmpty(t_content) && useField == "t_chinesetraditional")
			{
				t_content = XBuffer.ReadString(data, ref offset); 
			}
			else
			{
				//不需要的语言字段跳过
				short slen = XBuffer.ReadShort(data, ref offset);
				offset += slen;
			}


			if (string.IsNullOrEmpty(t_content) && useField == "t_english")
			{
				t_content = XBuffer.ReadString(data, ref offset); 
			}
			else
			{
				//不需要的语言字段跳过
				short slen = XBuffer.ReadShort(data, ref offset);
				offset += slen;
			}

        }

    }
}
