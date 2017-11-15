using System;

public class ClientDebug
{
    public static void Log(LogLevel requiredLevel, LogLevel level, string text)
    {
        if (requiredLevel <= level)
        {
            UnityEngine.Debug.Log(text);
        }
    }
}