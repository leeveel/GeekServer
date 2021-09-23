public class Debuger
{
    public static void Log(params object[] msg)
    {
        string log = "";
        for (int i = 0; i < msg.Length; ++i)
        {
            log += msg[i] == null ? "Null" : msg[i].ToString();
            if (i != msg.Length - 1) log += ",";
        }
        UnityEngine.Debug.Log(log);
    }

    public static void Wrn(params object[] msg)
    {
        string log = "";
        for (int i = 0; i < msg.Length; ++i)
        {
            log += msg[i] == null ? "Null" : msg[i].ToString();
            if (i != msg.Length - 1) log += ",";
        }
        UnityEngine.Debug.LogWarning(log);
    }

    public static void Err(params object[] msg)
    {
        string log = "";
        for (int i = 0; i < msg.Length; ++i)
        {
            log += msg[i] == null ? "Null" : msg[i].ToString();
            if (i != msg.Length - 1) log += ",";
        }
        UnityEngine.Debug.LogError(log);
    }

}
