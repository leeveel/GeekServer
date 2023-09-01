
using Base;
using UnityEngine;

namespace Logic
{
    public class Startup
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            SyncContextUtil.Init();
            Geek.Server.Proto.PolymorphicRegister.Load();
            var gameMain = new GameObject("GameMain");
            GameObject.DontDestroyOnLoad(gameMain);
            gameMain.AddComponent<GameMain>();
            gameMain.AddComponent<CoroutineManager>();
        }
    }
}
