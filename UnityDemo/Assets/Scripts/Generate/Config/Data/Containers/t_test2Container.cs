/**
 * Auto generated, do not edit it server
 *
 * 测试表
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
    public class t_test2BeanDeserializeProxyData
    {

        public List<int> t_id; 

        public List<int> t_mult; 

        public List<string> t_str; 

        public List<string> t_str2; 

        public List<int> t_int; 

        public List<int> t_int2; 

        public List<long> t_long; 

    }

    [MessagePackObject(true)]
    public class t_test2BeanDeserializeProxy
    { 
        public string sheetName;   
		public t_test2BeanDeserializeProxyData datas;
    }

	public class t_test2Container : BaseContainer
	{ 
		
		private List<t_test2Bean> list = new List<t_test2Bean>();
		private Dictionary<int, t_test2Bean> map = new Dictionary<int, t_test2Bean>();

		//public override List<t_test2Bean> getList()
		public override IList getList()
		{
			return list;
		}

		//public override Dictionary<int, t_test2Bean> getMap()
		public override IDictionary getMap()
		{
			return map;
		}
		
		public Type BinType = typeof(t_test2Bean);

		public override void loadDataFromBin()
		{    
			map.Clear();
			list.Clear();
			Loaded = true;
			
            byte[] data = Resources.Load<TextAsset>($"Bin/t_test2Bean").bytes;
            
			if(data != null)
			{
				try
				{
					var proxy = MessagePack.MessagePackSerializer.Deserialize<t_test2BeanDeserializeProxy>(data); 
					var datas = proxy.datas;
					var rowCount = datas.t_id.Count;
					list = new List<t_test2Bean>(rowCount); 
                    for (int i = 0; i < rowCount; i++)
                    {
                        var bean = new t_test2Bean();
                        list.Add(bean);

						if (datas.t_id != null && datas.t_id.Count > i)
                        { 
							bean.t_id = datas.t_id[i];
                        }

						if (datas.t_mult != null && datas.t_mult.Count > i)
                        { 

                            bean.m_t_mult = datas.t_mult[i];
                        }

						if (datas.t_str != null && datas.t_str.Count > i)
                        { 
							bean.t_str = datas.t_str[i];
                        }

						if (datas.t_str2 != null && datas.t_str2.Count > i)
                        { 
							bean.t_str2 = datas.t_str2[i];
                        }

						if (datas.t_int != null && datas.t_int.Count > i)
                        { 
							bean.t_int = datas.t_int[i];
                        }

						if (datas.t_int2 != null && datas.t_int2.Count > i)
                        { 
							bean.t_int2 = datas.t_int2[i];
                        }

						if (datas.t_long != null && datas.t_long.Count > i)
                        { 
							bean.t_long = datas.t_long[i];
                        }

                    }

                    foreach (var d in list)
                    {
                        if (!map.ContainsKey(d.t_id))
                            map.Add(d.t_id, d);
                        else
                             Debug.LogError("Exist duplicate Key: " + d.t_id + " t_test2Bean");
                    }
				}
				catch (Exception ex)
				{
					 Debug.LogError("import data error: t_test2Bean >>" + ex.ToString());
				}
			}
			else
			{
				 Debug.LogError("can not find conf data: t_test2Bean.bytes");
			}
		}
		
	}
}


