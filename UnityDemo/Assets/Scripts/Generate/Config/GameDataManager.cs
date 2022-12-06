/**
 * Auto generated, do not edit it server
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geek.Client.Config
{
	public class GameDataManager
	{
		public DateTime ReloadTime { get; private set; }
		
		public static GameDataManager Instance { get; private set; }

        public static (bool, string) ReloadAll()
        {
            try
            {
                var data = new GameDataManager();
                data.LoadAll(true);
                data.ReloadTime = DateTime.Now;
                Instance = data;
				return (true, "");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
				return (false, e.Message);
            }
        }
		
		public Dictionary<string, DateTime> GetBeanFileTime()
		{
			var folder = System.Environment.CurrentDirectory;
			var map = new Dictionary<string, DateTime>();
			foreach(var kv in t_containerMap)
			{
				var file = new System.IO.FileInfo(folder + "/Bytes/" + kv.Key.Name  + ".bytes");	
				if(file.Exists)
					map[kv.Key.Name] = file.LastWriteTime;
			}
			return map;
		}
		
        t_testContainer t_testContainer = new t_testContainer();
        t_test2Container t_test2Container = new t_test2Container();
        t_languageContainer t_languageContainer = new t_languageContainer();
        t_itemContainer t_itemContainer = new t_itemContainer();
		//@%@%@
		private GameDataManager()
		{
			t_containerMap.Add(t_testContainer.BinType, t_testContainer);
			t_containerMap.Add(t_test2Container.BinType, t_test2Container);
			t_containerMap.Add(t_languageContainer.BinType, t_languageContainer);
			t_containerMap.Add(t_itemContainer.BinType, t_itemContainer);
			//@#@#@
		}
		
		public void LoadAll(bool forceReload = false)
		{
			LoadOneBean(t_testContainer.BinType, forceReload);
			LoadOneBean(t_test2Container.BinType, forceReload);
			LoadOneBean(t_languageContainer.BinType, forceReload);
			LoadOneBean(t_itemContainer.BinType, forceReload);
			//@*@*@
		}
		
		//bin -- container dictionary
		private Dictionary<Type, BaseContainer> t_containerMap = new Dictionary<Type, BaseContainer>();
		
		public T GetBin<T, K>(K key, bool ignoreErrLog) where T : BaseBin
		{
			Type t = typeof(T);
			LoadOneBean(t);
			if(t_containerMap.ContainsKey(t))
			{
				var t_container = t_containerMap[t];
				Dictionary<K, T> map = t_container.getMap() as Dictionary<K, T>;
				if(map != null && map.ContainsKey(key))
					return map[key];
			}
			if(false == ignoreErrLog)
				Debug.LogError("can not find Bin:" + t.Name + " id=" + key);
			return null;
		}
		
		public List<T> GetBinList<T>() where T : BaseBin
		{
			Type t = typeof(T);
			LoadOneBean(t);
			if(t_containerMap.ContainsKey(t))
			{
				var t_container = t_containerMap[t];
				List<T> list = t_container.getList() as List<T>;
				if(list != null)
					return list;
				Debug.LogError("can not find Bin > " + t.Name);
			}
			Debug.LogError("can not find Bin > " + t.Name);
			return null;
		}
		
		public Dictionary<K, T> GetBinMap<T, K>() where T : BaseBin
		{
			Type t = typeof(T);
			LoadOneBean(t);
			if(t_containerMap.ContainsKey(t))
			{
				var t_container = t_containerMap[t];
				Dictionary<K, T> map = t_container.getMap() as Dictionary<K, T>;
				if(map != null)
					return map;
				Debug.LogError("T,K get " + t.Name + "," + typeof(K).Name + " should be " + t_container.getMap());
			}
			return null;
		}
		
		public void LoadOneBean<T>(bool forceReload = false) where T : BaseBin
        {
			Type t = typeof(T);
			LoadOneBean(t, forceReload);
        }
		
		public void LoadOneBean(Type t, bool forceReload = false)
		{
            if (t_containerMap.ContainsKey(t))
            {
                if (!t_containerMap[t].Loaded || forceReload)
                {
					t_containerMap[t].loadDataFromBin();
				}
            }
		}
	}
	
	public class BaseContainer
	{
		public bool Loaded { get; protected set; }
		public virtual IList getList()
		{
			return null;
		}
		
		public virtual IDictionary getMap()
		{
			return null;
		}
		
		public virtual void loadDataFromBin()
		{
		
		}
	}
	
	public class BaseBin
	{
	}
	
	public class ConfigBean
	{
		public static bool IsServer;
		/// <summary>
		///T -----> Bean
		///K -----> id type (int/string)
		/// </summary>
		public static T GetBean<T, K>(K id, bool ignoreErrLog = false) where T : BaseBin
		{
			return GameDataManager.Instance.GetBin<T, K>(id, ignoreErrLog);
		}
		
		/// <summary>
		/// Get Table Data By List
		/// </summary>
		public static List<T> GetBeanList<T>() where T : BaseBin
		{
			return GameDataManager.Instance.GetBinList<T>();
		}
		
		/// <summary>
		/// Get Table Data By Map 
		/// </summary>
		public static Dictionary<K, T> GetBeanMap<T, K>() where T : BaseBin
		{
			return GameDataManager.Instance.GetBinMap<T, K>();
		}
	}

	public static class ConfigExtension
	{
		public static string GetItsLanaugeStr(this int id, string defaultStr = "")
		{
			var bean = ConfigBean.GetBean<t_languageBean, int>(id);
			if(bean != null)
				return bean.t_content;
			return defaultStr;
		}
	}
}