using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base
{

    public class CoroutineManager : MonoBehaviour
    {
        /// <summary>
        /// 内部辅助类
        /// </summary>
        private class CoroutineTask
        {
            public Int64 Id { get; set; }
            public bool Running { get; set; }
            public bool Paused { get; set; }

            public CoroutineTask(Int64 id)
            {
                Id = id;
                Running = true;
                Paused = false;
            }

            public IEnumerator coroutineWrapper(IEnumerator co)
            {
                Stack<IEnumerator> stack = new Stack<IEnumerator>();
                IEnumerator now = co;
                while (Running)
                {
                    if (Paused)
                        yield return null;
                    else
                    {
                        if (now != null)
                        {
                            try
                            {
                                bool ret = now.MoveNext();
                                if (ret == false)
                                {
                                    if (stack.Count > 0)
                                        now = stack.Pop();
                                    else
                                        Running = false;
                                    continue;
                                }
                                if (now.Current is IEnumerator next)
                                {
                                    stack.Push(now);
                                    now = next;
                                    continue;
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.Message + "\n" + e.StackTrace);
                            }
                            yield return now.Current;
                        }
                        else
                            Running = false;
                    }
                }
                mCoroutines.Remove(Id.ToString());
            }
        }

        private static Dictionary<string, CoroutineTask> mCoroutines;
        public static CoroutineManager Singleton { get; private set; }

        void Awake()
        {
            Singleton = this;
            mCoroutines = new Dictionary<string, CoroutineTask>();
        }

        private long curId;
        private long newId()
        {
            return ++curId;
        }

        /// <summary>
        /// 启动一个协程
        /// </summary>
        /// <param name="co"></param>
        /// <returns></returns>
        public long startCoroutine(IEnumerator co)
        {
            if (this.gameObject.activeSelf)
            {
                CoroutineTask task = new CoroutineTask(newId());
                mCoroutines.Add(task.Id.ToString(), task);
                StartCoroutine(task.coroutineWrapper(co));
                return task.Id;
            }
            return -1;
        }

#if UNITY_EDITOR
        /*public int CoroutineNum;
        private void LateUpdate()
        {
            CoroutineNum = mCoroutines.Count;
        }*/
#endif

        /// <summary>
        /// 停止一个协程
        /// </summary>
        /// <param name="id"></param>
        public void stopCoroutine(long id)
        {
            if (mCoroutines.ContainsKey(id.ToString()))
            {
                CoroutineTask task = mCoroutines[id.ToString()];
                task.Running = false;
                mCoroutines.Remove(id.ToString());
            }
        }

        /// <summary>
        /// 暂停协程的运行
        /// </summary>
        /// <param name="id"></param>
        public void pauseCoroutine(Int64 id)
        {
            if (mCoroutines.ContainsKey(id.ToString()))
            {
                CoroutineTask task = mCoroutines[id.ToString()];
                task.Paused = true;
            }
            else
            {
                Debug.LogError("coroutine: " + id.ToString() + " is not exist!");
            }
        }

        /// <summary>
        /// 恢复协程的运行
        /// </summary>
        /// <param name="id"></param>
        public void resumeCoroutine(Int64 id)
        {
            if (mCoroutines.ContainsKey(id.ToString()))
            {
                CoroutineTask task = mCoroutines[id.ToString()];
                task.Paused = false;
            }
            else
            {
                Debug.LogError("coroutine: " + id.ToString() + " is not exist!");
            }
        }

        public long delayedCall(float delayedTime, Action callback)
        {
            return startCoroutine(delayedCallImpl(delayedTime, callback));
        }

        private IEnumerator delayedCallImpl(float delayedTime, Action callback)
        {
            if (delayedTime >= 0)
                yield return new WaitForSeconds(delayedTime);
            if (callback != null)
                callback();
        }


        public long delayedCall(float delayedTime, Action<object> callback, object param)
        {
            return startCoroutine(delayedCallImpl(delayedTime, callback, param));
        }

        private IEnumerator delayedCallImpl(float delayedTime, Action<object> callback, object param)
        {
            if (delayedTime >= 0)
                yield return new WaitForSeconds(delayedTime);
            if (callback != null)
                callback(param);
        }

        public long realTimeDelayCall(float delayedTime, Action callback)
        {
            return startCoroutine(realdDelayedCallImpl(delayedTime, callback));
        }

        private IEnumerator realdDelayedCallImpl(float delayedTime, Action callback)
        {
            if (delayedTime >= 0)
                yield return new WaitForSecondsRealtime(delayedTime);
            if (callback != null)
                callback();
        }

        public long realTimeDelayCall(float delayedTime, Action<object> callback, object param)
        {
            return startCoroutine(realdDelayedCallImpl(delayedTime, callback, param));
        }

        private IEnumerator realdDelayedCallImpl(float delayedTime, Action<object> callback, object param)
        {
            if (delayedTime >= 0)
                yield return new WaitForSecondsRealtime(delayedTime);
            if (callback != null)
                callback(param);
        }

        private void OnDestroy()
        {
            foreach (var task in mCoroutines.Values)
                task.Running = false;
            mCoroutines.Clear();
        }
    }
}