
using UnityEngine;

namespace Logic
{
    public class Startup
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            Geek.Server.Proto.PolymorphicRegister.Load();
            new GameObject("GameMain").AddComponent<GameMain>();
        }
    }
}
