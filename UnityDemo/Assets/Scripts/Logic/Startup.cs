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
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            Initialize();
        }
#endif

    }
}
