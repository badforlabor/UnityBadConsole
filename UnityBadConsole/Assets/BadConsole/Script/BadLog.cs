using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace bad_log
{
    public interface IBadLog
    { }
    public class LogInfo
    {
        public string Content;
        public bad_log.ELoggerType LogType;
        public int CollapseCnt = 1;
    }

    public class LoggerSaver
    {
        static StreamWriter stream;

        static LoggerSaver()
        {
#if UNITY_ANDROID
            //string temporaryCachePath = Application.temporaryCachePath;
            string temporaryCachePath = Application.persistentDataPath + "/log/";
#elif UNITY_IPHONE
            string temporaryCachePath = Application.temporaryCachePath + "/log/";
#else
            string temporaryCachePath = Application.dataPath + "/../log/";
#endif
            if (!Directory.Exists(temporaryCachePath))
            {
                Directory.CreateDirectory(temporaryCachePath);
            }


            string destFileName = Path.Combine(temporaryCachePath, "log_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + ".txt");

            FileStream filestream = null;
            try
            {
                filestream = File.Open(destFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream = new StreamWriter(filestream, new UTF8Encoding(false));
                stream.AutoFlush = true;
            }
            catch (System.Exception)
            {
                if (stream != null)
                {
                    stream.Close();
                }
                else if (stream != null)
                {
                    stream.Close();
                }
            }
            Write("start log! at time:" + DateTime.Now);
            Write("");
        }
        public static void Write(string txt)
        {
            if (stream != null)
            {
                stream.WriteLine(txt);
            }
        }
    }

    public static class BadLogWrapper
    {
        public delegate void LogCallback(string content, ELoggerType LogType);
        public static event LogCallback OnLog;
        public static void Log(string content, ELoggerType LogType)
        {
            Profiler.BeginSample("l1");
            if (OnLog != null)
            {
                OnLog(content, LogType);
            }
            Profiler.EndSample();
            Profiler.BeginSample("l2");
            LoggerSaver.Write(content);
            LoggerSaver.Write("\r\n");
            Profiler.EndSample();
        }
        static System.Text.StringBuilder MyBuffer = new StringBuilder();
        public static void Log(string content, string track, string tag, string tag2, ELoggerType LogType)
        {
            MyBuffer.Remove(0, MyBuffer.Length);
            MyBuffer.Append("[");
            MyBuffer.Append(tag);
            MyBuffer.Append("]");
            MyBuffer.Append(tag2);
            MyBuffer.Append(content);
            MyBuffer.Append("\n");
            MyBuffer.Append(track);
            Log(MyBuffer.ToString(), LogType);
        }
    }

    public static class TBadLogAutoHelper
    {
        public static void SetValue(ref string Tag, ref ELoggerType LogType)
        {
            // 读取配置文件
            GameObject go = Resources.Load<GameObject>("bad_log_config");
            BadLogConfigMono config = go == null ? null : go.GetComponent<BadLogConfigMono>();
            if (config != null)
            {
                string key = Tag;
                if (config.dict.ContainsKey(key))
                {
                    var v = config.dict[key];
                    if (v == null || string.IsNullOrEmpty(v.Key))
                    {

                    }
                    else
                    {
                        Tag = v.Key;
                        LogType = v.Value;
                    }
                }
            }
        }
    }
    public static class TBadLogAuto<T, T1> where T1 : ILogType
    {
        public static string Tag = "";
        public static ELoggerType LogType = ELoggerType.Debug;

        static TBadLogAuto()
        {
            // 读取配置文件：
            UnityEngine.Debug.Log("Init:" + typeof(T).Name);

            if (typeof(T1) == typeof(ErrorLogType))
            {
                LogType = ELoggerType.Error;
            }
            else if (typeof(T1) == typeof(WarningLogType))
            {
                LogType = ELoggerType.Warn;
            }

            Tag = typeof(T).Name;

            TBadLogAutoHelper.SetValue(ref Tag, ref LogType);
        }
    }
    public class TBadLog<T, T1> : IBadLog where T : IBadLog where T1 : ILogType
    {
        public static void SetLogType(ELoggerType logType)
        {
            TBadLogAuto<T, T1>.LogType = logType;
        }
        public static ELoggerType GetLogType()
        {
            return TBadLogAuto<T, T1>.LogType;
        }
        public static void F(string content)
        {
            if (TBadLogAuto<T, T1>.LogType >= ELoggerType.Fatal)
            {
#if OLD_LOG
                UnityEngine.Debug.LogError(content);
#else
                BadLogWrapper.Log(content, StackTraceUtility.ExtractStackTrace(), TBadLogAuto<T, T1>.Tag, "[Fatal] ", ELoggerType.Fatal);
#endif
            }
        }
        public static void E(string content)
        {
            if (TBadLogAuto<T, T1>.LogType >= ELoggerType.Error)
            {
#if OLD_LOG
                UnityEngine.Debug.LogError(string.Format("[{0}][Error] {1} ", TBadLogAuto<T, T1>.Tag, content));
                UnityEngine.Debug.LogError(content);
#else
                BadLogWrapper.Log(content, StackTraceUtility.ExtractStackTrace(), TBadLogAuto<T, T1>.Tag, "[Error] ", ELoggerType.Error);
#endif
            }
        }
        public static void W(string content)
        {
            if (TBadLogAuto<T, T1>.LogType >= ELoggerType.Warn)
            {
#if OLD_LOG
                content = (string.Format("[{0}][Warning] {1} ", TBadLogAuto<T, T1>.Tag, content));
                UnityEngine.Debug.LogWarning(content);
#else
                BadLogWrapper.Log(content, StackTraceUtility.ExtractStackTrace(), TBadLogAuto<T, T1>.Tag, "[Warn] ", ELoggerType.Warn);
#endif
            }
        }
        public static void I(string content)
        {
            if (TBadLogAuto<T, T1>.LogType >= ELoggerType.Info)
            {
#if OLD_LOG
                content = (string.Format("[{0}][Info] {1} ", TBadLogAuto<T, T1>.Tag, content));
                UnityEngine.Debug.Log(content);
#else
                BadLogWrapper.Log(content, StackTraceUtility.ExtractStackTrace(), TBadLogAuto<T, T1>.Tag, "[Info] ", ELoggerType.Info);
#endif
            }
        }
        public static void D(string content)
        {
            if (TBadLogAuto<T, T1>.LogType >= ELoggerType.Debug)
            {
#if OLD_LOG
                content = (string.Format("[{0}][debug] {1} ", TBadLogAuto<T, T1>.Tag, content));
                UnityEngine.Debug.Log(content);
#else
                BadLogWrapper.Log(content, StackTraceUtility.ExtractStackTrace(), TBadLogAuto<T, T1>.Tag, "[Debug]", ELoggerType.Debug);
#endif
            }
        }
    }
    public class TBadLog<T> : TBadLog<T, ErrorLogType> where T : IBadLog
    { }

    public class BadLog1 : TBadLog<BadLog1>
    { }
    public class BadLog2 : TBadLog<BadLog2>
    { }
    public class BadLog3 : TBadLog<BadLog3, WarningLogType>
    { }
    public class BadLog4 : TBadLog<BadLog4, WarningLogType>
    { }
}
