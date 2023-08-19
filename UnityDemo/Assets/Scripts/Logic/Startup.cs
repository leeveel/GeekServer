using Geek.Server.Proto;
using MessagePack;
using MessagePack.Resolvers;

using UnityEngine;

namespace Logic
{
    public class Startup
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            PolymorphicRegister.Load();
            new GameObject("GameMain").AddComponent<GameMain>();
        }
    }
}
