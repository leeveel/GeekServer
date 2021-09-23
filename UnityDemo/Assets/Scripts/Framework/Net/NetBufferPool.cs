using System;
using System.Collections.Generic;

namespace Geek.Client
{
    public class NetBufferPool
    {
        static int minSize = 0;
        static int maxSize = 0;
        static int increase = 0;
        const int maxCacheNum = 15;
        static Dictionary<int, Queue<byte[]>> cacheQueue = new Dictionary<int, Queue<byte[]>>();
        public static void Init()
        {
            if (minSize > 0)
                return;

            maxSize = 1024 * 3;
            minSize = 128;
            increase = 128;

            for (int a = minSize; a <= maxSize; a += increase)
            {
                Queue<byte[]> queue = new Queue<byte[]>();
                for (int b = 0; b < 1; ++b)
                    queue.Enqueue(new byte[a]);
                cacheQueue.Add(a, queue);
            }
        }
        
        public static byte[] Alloc(int size)
        {
            int lvl = GetLevelBySize(size);
            if (lvl == -1)
            {
                return new byte[size];
            }
            else
            {
                byte[] result = null;
                if (cacheQueue.TryGetValue(lvl, out Queue<byte[]> queue) && queue != null)
                {
                    lock (queue)
                    {
                        if (queue.Count > 0)
                            result = queue.Dequeue();
                    }

                    if (result == null)
                        result = new byte[lvl];
                }
                return result;
            }
        }

        public static void Free(byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0)
                return;

            int lvl = GetLevelBySize(bytes.Length);
            if (lvl == -1)
            {
                bytes = null;
                return;
            }

            if (cacheQueue.TryGetValue(lvl, out Queue<byte[]> queue) && queue != null)
            {
                if (bytes.Length == lvl)
                {
                    lock (queue)
                    {
                        if (queue.Count < maxCacheNum)
                            queue.Enqueue(bytes);
                    }
                    Array.Clear(bytes, 0, bytes.Length);
                }
                else
                {
                    UnityEngine.Debug.LogError("归还的缓冲区长度不是key：" + lvl + " " + bytes.Length);
                }
            }
        }

        static int GetLevelBySize(int size)
        {
            int needSize = size;
            if (needSize > maxSize)
                return -1;

            if (needSize <= minSize)
                return minSize;

            for (int a = minSize; a <= maxSize; a += increase)
            {
                if (needSize <= a)
                    return a;
            }
            return -1;
        }
    }
}
