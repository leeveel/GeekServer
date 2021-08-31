/**
 * Auto generated, do not edit it server
 *
 * 测试表
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace Geek.Server.Config
{
	public class t_testContainer : BaseContainer
	{
	    private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

		private List<t_testBean> list = new List<t_testBean>();
		private Dictionary<int, t_testBean> map = new Dictionary<int, t_testBean>();

		//public override List<t_testBean> getList()
		public override IList getList()
		{
			return list;
		}

		//public override Dictionary<int, t_testBean> getMap()
		public override IDictionary getMap()
		{
			return map;
		}
		
		public Type BinType = typeof(t_testBean);

		public override void loadDataFromBin()
		{    
			map.Clear();
			list.Clear();
			Loaded = true;
			
			string binPath = System.Environment.CurrentDirectory + "/Bytes/t_testBean.bytes";
            byte[] data;
            if (File.Exists(binPath))
                data = File.ReadAllBytes(binPath);
            else
            	throw new Exception("can not find " + binPath);
			// FieldCount:int + FieldType:byte(0:int 1:long 2:string 3:float)
			int offset = 63;  
			while (data.Length > offset)
			{
				t_testBean bean = new t_testBean();
				bean.LoadData(data, ref offset);
				list.Add(bean);
				if(!map.ContainsKey(bean.t_id))
					map.Add(bean.t_id, bean);
				else
					throw new Exception("Exist duplicate Key: " + bean.t_id + " t_testBean");
			}
		}
		
	}
}


