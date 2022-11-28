/**
 * Auto generated, do not edit it server
 *
 * 语言表
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using NLog;

namespace Geek.Server.Config
{   
	[MessagePackObject(true)]
    public class t_languageBeanDeserializeProxyData
    {
        public List<int> t_id;  
        public List<string> content;  
    }

    [MessagePackObject(true)]
    public class t_languageBeanDeserializeProxy
    { 
        public string sheetName;  
		public int rowCount; 
		public t_languageBeanDeserializeProxyData datas;
    }

	public class t_languageContainer : BaseContainer
	{
		private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();
		private List<t_languageBean> list = new List<t_languageBean>();
		private Dictionary<int, t_languageBean> map = new Dictionary<int, t_languageBean>();

		//public override List<t_languageBean> getList()
		public override IList getList()
		{
			return list;
		}

		//public override Dictionary<int, t_languageBean> getMap()
		public override IDictionary getMap()
		{
			return map;
		}
		
		public Type BinType = typeof(t_languageBean);

		public override void loadDataFromBin()
		{    
			map.Clear();
			list.Clear();
			Loaded = true;
			
            string useField = Settings.Language==null?"t_chinese":Settings.Language; 
            
			string binPath = System.Environment.CurrentDirectory + $"/Bytes/t_language{useField.Replace("t_", "")}Bean.bytes";
            byte[] data;
            if (File.Exists(binPath))
                data = File.ReadAllBytes(binPath);
            else
            	throw new Exception("can not find " + binPath);
			if(data != null)
			{
				try
				{
					var proxy = MessagePack.MessagePackSerializer.Deserialize<t_languageBeanDeserializeProxy>(data); 
					var datas = proxy.datas;
					var rowCount = datas.t_id.Count;
					list = new List<t_languageBean>(rowCount);  
                    for (int i = 0; i < rowCount; i++)
                    {
                        var bean = new t_languageBean();
                        list.Add(bean); 
                        bean.t_id = datas.t_id[i];  
                        bean.t_content = datas.content[i];  
                    }

                    foreach (var d in list)
                    {
                        if (!map.ContainsKey(d.t_id))
                            map.Add(d.t_id, d);
                        else
                             LOGGER.Error("Exist duplicate Key: " + d.t_id + " t_languageBean");
                    }
				}
				catch (Exception ex)
				{
					 LOGGER.Error("import data error: t_languageBean >>" + ex.ToString());
				}
			}
			else
			{
				 LOGGER.Error("can not find conf data: t_languageBean.bytes");
			}
		}
		
	}
}


