/**
 * Auto generated, do not edit it server
 *
 * 语言表
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geek.Client.Config
{
	public class t_languageContainer : BaseContainer
	{
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
			
			var ta = Resources.Load<TextAsset>("Bin/t_languageBean");
			if(ta == null)
				throw new Exception("can not find t_languageBean");
				
            byte[] data = ta.bytes;
			// FieldCount:int + FieldType:byte(0:int 1:long 2:string 3:float)
			int offset = 58;  
			while (data.Length > offset)
			{
				t_languageBean bean = new t_languageBean();
				bean.LoadData(data, ref offset);
				list.Add(bean);
				if(!map.ContainsKey(bean.t_id))
					map.Add(bean.t_id, bean);
				else
					throw new Exception("Exist duplicate Key: " + bean.t_id + " t_languageBean");
			}
		}
		
	}
}