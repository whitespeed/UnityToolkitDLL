using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;
public class Log
{
    public static bool enable = Debug.logger.logEnabled;
    public static bool toFile = false;
    public static LogType level = LogType.Error;
    protected static readonly string directoryPath;
    protected static readonly string filePath;
    static Log()
    {
        if (Application.isMobilePlatform)
        {
            directoryPath = Path.Combine(Application.persistentDataPath, "Log");
        }
        else if (Application.isEditor && Application.isPlaying)
        {
            directoryPath = Path.Combine(UnityEngine.Application.dataPath, "/../Log");
        }
        else if (Application.isEditor && !Application.isPlaying)
        {
            directoryPath = Path.Combine(UnityEngine.Application.dataPath, "/../EditorLog");
        }
        else
        {
            directoryPath = Path.Combine(Application.dataPath, "Log");
        }
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        filePath = Path.Combine(directoryPath, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Application.logMessageReceived += LogCallback;
        Application.logMessageReceived += LogCallbackInThread;
    }

    protected static void LogCallback(string condition, string stackTrace, LogType type)
    {
        if(enable && toFile && type>= level)
        {
            string str = string.Format("{0}---{1}---{2} {3}\n\n", DateTime.Now.ToString("HH:mm:ss.fff"), type, condition, stackTrace);
            File.AppendAllText(filePath,str);
        }
    }

    protected static void LogCallbackInThread(string condition, string stackTrace, LogType type)
    {
        if (enable && toFile && type >= level)
        {
            string str = string.Format("Thread:{4} {5} {0}---{1}---{2} {3}\n\n", DateTime.Now.ToString("HH:mm:ss.fff"), type, condition, stackTrace, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name);
            File.AppendAllText(filePath, str);
        }
    }

    public static void Print(LogType level, string tag, object message)
    {
        switch (level)
        {
            case LogType.Log:
                Info(tag, message);
                break;
            case LogType.Warning:
                Warning(tag, message);
                break;
            case LogType.Error:
                Error(tag, message);
                break;
            default:
                break;
        }
    }

    public static void Print(LogType level, string tag, string format, params object[] args)
    {
        switch (level)
        {
            case LogType.Log:
                InfoFormat(tag, format, args);
                break;
            case LogType.Warning:
                WarningFormat(tag, format, args);
                break;
            case LogType.Error:
                ErrorFormat(tag, format, args);
                break;
            default:
                break;
        }
    }
    public static void Info(string tag, object message)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, message);
        Debug.Log(str);
    }
    public static void Info(string tag, object message, Object context)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, message);
        Debug.Log(str, context);
    }

    public static void InfoFormat(string tag, string format, params object[] args)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, string.Format(format, args));
        Debug.Log(str);
    }

    public static void InfoFormat(Object context, string tag, string format, params object[] args)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, string.Format(format, args));
        Debug.Log(str, context);
    }


    public static void Warning(string tag, object message)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, message);
        Debug.LogWarning(str);
    }
    public static void Warning(string tag, object message, Object context)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, message);
        Debug.LogWarning(str, context);
    }

    public static void WarningFormat(string tag, string format, params object[] args)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, string.Format(format, args));
        Debug.LogWarning(str);
    }

    public static void WarningFormat(Object context, string tag, string format, params object[] args)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, string.Format(format, args));
        Debug.LogWarning(str, context);
    }

    public static void Error(string tag, object message)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, message);
        Debug.LogError(str);
    }
    public static void Error(string tag, object message, Object context)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, message);
        Debug.LogError(str, context);
    }

    public static void ErrorFormat(string tag, string format, params object[] args)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, string.Format(format, args));
        Debug.LogError(str);
    }

    public static void ErrorFormat(Object context, string tag, string format, params object[] args)
    {
        if (!enable)
            return;
        var str = string.Format("[{0}] {1}", tag, string.Format(format, args));
        Debug.LogWarning(str, context);
    }
}
