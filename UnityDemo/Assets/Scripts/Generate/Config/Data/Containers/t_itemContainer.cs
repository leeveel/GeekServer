/**
 * Auto generated, do not edit it server
 *
 * 用于测试的道具表
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MessagePack; 
using UnityEngine;

namespace Geek.Client.Config
{
	[MessagePackObject(true)]
    public class t_itemBeanDeserializeProxyData
    {

        public List<int> t_id; 

        public List<string> t_name; 

        public List<int> t_can_sell; 

        public List<int> t_show; 

        public List<string> t_sell_num; 

        public List<string> t_desc; 

        public List<int> t_use_type; 

        public List<string> t_param; 

    }

    [MessagePackObject(true)]
    public class t_itemBeanDeserializeProxy
    { 
        public string sheetName;   
		public t_itemBeanDeserializeProxyData datas;
    }

	public class t_itemContainer : BaseContainer
	{ 
		
		private List<t_itemBean> list = new List<t_itemBean>();
		private Dictionary<int, t_itemBean> map = new Dictionary<int, t_itemBean>();

		//public override List<t_itemBean> getList()
		public override IList getList()
		{
			return list;
		}

		//public override Dictionary<int, t_itemBean> getMap()
		public override IDictionary getMap()
		{
			return map;
		}
		
		public Type BinType = typeof(t_itemBean);

		public override void loadDataFromBin()
		{    
			map.Clear();
			list.Clear();
			Loaded = true;
			
            byte[] data = Resources.Load<TextAsset>($"Bin/t_itemBean").bytes;
            
			if(data != null)
			{
				try
				{
					var proxy = MessagePack.MessagePackSerializer.Deserialize<t_itemBeanDeserializeProxy>(data); 
					var datas = proxy.datas;
					var rowCount = datas.t_id.Count;
					list = new List<t_itemBean>(rowCount); 
                    for (int i = 0; i < rowCount; i++)
                    {
                        var bean = new t_itemBean();
                        list.Add(bean);

						if (datas.t_id != null && datas.t_id.Count > i)
                        { 
							bean.t_id = datas.t_id[i];
                        }

						if (datas.t_name != null && datas.t_name.Count > i)
                        { 
							bean.t_name = datas.t_name[i];
                        }

						if (datas.t_can_sell != null && datas.t_can_sell.Count > i)
                        { 
							bean.t_can_sell = datas.t_can_sell[i];
                        }

						if (datas.t_show != null && datas.t_show.Count > i)
                        { 
							bean.t_show = datas.t_show[i];
                        }

						if (datas.t_sell_num != null && datas.t_sell_num.Count > i)
                        { 
							bean.t_sell_num = datas.t_sell_num[i];
                        }

						if (datas.t_desc != null && datas.t_desc.Count > i)
                        { 
							bean.t_desc = datas.t_desc[i];
                        }

						if (datas.t_use_type != null && datas.t_use_type.Count > i)
                        { 
							bean.t_use_type = datas.t_use_type[i];
                        }

						if (datas.t_param != null && datas.t_param.Count > i)
                        { 
							bean.t_param = datas.t_param[i];
                        }

                    }

                    foreach (var d in list)
                    {
                        if (!map.ContainsKey(d.t_id))
                            map.Add(d.t_id, d);
                        else
                             Debug.LogError("Exist duplicate Key: " + d.t_id + " t_itemBean");
                    }
				}
				catch (Exception ex)
				{
					 Debug.LogError("import data error: t_itemBean >>" + ex.ToString());
				}
			}
			else
			{
				 Debug.LogError("can not find conf data: t_itemBean.bytes");
			}
		}
		
	}
}


