/*
 * 额，在Editor模式下，点击“Play”按钮后，Editor中的Static变量、List类型的成员变量都会清零
 * 所以，转到C++存储数据咯
 */

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace bad_log
{
    public class StaticStorage
    {
        [DllImport("BadConsoleCppStorage")]
        public static extern int MyAdd(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        public struct CLogInfo
        {
            public string Content;
            public int LogType;
            public int CollapseCnt;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct CLogInfoKeyPair
        {
            public string Content;
            public int CollapseCnt;
        }
        [DllImport("BadConsoleCppStorage")]
        public static extern int GetLogStoreCnt();
        [DllImport("BadConsoleCppStorage")]
        public static extern int SaveLogStoreInfo(CLogInfo data);
        [DllImport("BadConsoleCppStorage")]
        public static extern CLogInfo GetLogStoreInfo(int idx);
        [DllImport("BadConsoleCppStorage")]
        public static extern int GetCollapsedLogCnt();
        [DllImport("BadConsoleCppStorage")]
        public static extern int PushCollapsedLog(CLogInfo data);
        [DllImport("BadConsoleCppStorage")]
        public static extern int SetCollapsedCount(int idx, int cnt);
        [DllImport("BadConsoleCppStorage")]
        public static extern CLogInfo GetCollapsedLog(int idx);
        //         [DllImport("BadConsoleCppStorage")]
        //         private static extern int PushCollapsedDict(string key, int v);
        //         [DllImport("BadConsoleCppStorage")]
        //         private static extern int GetCollapsedDictValue(string key);
        [DllImport("BadConsoleCppStorage")]
        public static extern int ClearAllStorage();
        [DllImport("BadConsoleCppStorage")]
        public static extern int GetFlagValue(int idx);
        [DllImport("BadConsoleCppStorage")]
        public static extern int SetFlagValue(int idx, int v);

        enum EStaticFlag
        {
            ESF_None,
            ESF_Collapse,
            ESF_ClearOnPlay,
            ESF_ErrorPause,
            ESF_LogInfo,
            ESF_LogWarn,
            ESF_LogError
        }
        static bool GetFlagValue(EStaticFlag flag)
        {
            return GetFlagValue((int)flag) > 0;
        }
        static void SetFlagValue(EStaticFlag flag, bool v)
        {
            if (v)
                SetFlagValue((int)flag, 1);
            else
                SetFlagValue((int)flag, 0);
        }

        public static bool bCollapse
        {
            get { return GetFlagValue(EStaticFlag.ESF_Collapse); }
            set { SetFlagValue(EStaticFlag.ESF_Collapse, value); }
        }
        public static bool bClearOnPlay
        {
            get { return GetFlagValue(EStaticFlag.ESF_ClearOnPlay); }
            set { SetFlagValue(EStaticFlag.ESF_ClearOnPlay, value); }
        }
        public static bool bErrorPause
        {
            get { return GetFlagValue(EStaticFlag.ESF_ErrorPause); }
            set { SetFlagValue(EStaticFlag.ESF_ErrorPause, value); }
        }

        public static bool bLogInfo
        {
            get { return GetFlagValue(EStaticFlag.ESF_LogInfo); }
            set { SetFlagValue(EStaticFlag.ESF_LogInfo, value); }
        }
        public static bool bLogWarn
        {
            get { return GetFlagValue(EStaticFlag.ESF_LogWarn); }
            set { SetFlagValue(EStaticFlag.ESF_LogWarn, value); }
        }
        public static bool bLogError
        {
            get { return GetFlagValue(EStaticFlag.ESF_LogError); }
            set { SetFlagValue(EStaticFlag.ESF_LogError, value); }
        }

        public static bool[] LogValueArray = new bool[(int)bad_log.ELoggerType.ALL];

        // 这几个static字段本来想用来存储数据，结果，当点击“Play”的时候，发现dll重新加载了，导致static变量数值被还原了。
        static List<LogInfo> LogStore = new List<LogInfo>();
        static List<LogInfo> CollapseLogStore = new List<LogInfo>();
        static Dictionary<string, int> Dict = new Dictionary<string, int>();

        public static List<LogInfo> logs = new List<LogInfo>();

        public static int ErrorCnt = 0;
        public static int WarnCnt = 0;
        public static int InfoCnt = 0;

        public static void Init(int InitedCounter)
        {
            // 设置缓存数据
            Dict.Clear();
            LogStore.Clear();
            int cnt = GetLogStoreCnt();
            for (int i = 0; i < cnt; i++)
            {
                LogStore.Add(CLogInfo2LogInfo(GetLogStoreInfo(i)));
            }

            CollapseLogStore.Clear();
            cnt = GetCollapsedLogCnt();
            for (int i = 0; i < cnt; i++)
            {
                CLogInfo info = GetCollapsedLog(i);
                Dict.Add(info.Content, CollapseLogStore.Count + 1);
                CollapseLogStore.Add(CLogInfo2LogInfo(info));
            }
            UnityEngine.Debug.Log("refresh storage.");
        }
        public static void RefreshLogValueArray()
        {
            LogValueArray[(int)bad_log.ELoggerType.Info] = bLogInfo;
            LogValueArray[(int)bad_log.ELoggerType.Debug] = bLogInfo;
            LogValueArray[(int)bad_log.ELoggerType.Warn] = bLogWarn;
            LogValueArray[(int)bad_log.ELoggerType.Error] = bLogError;
            LogValueArray[(int)bad_log.ELoggerType.Fatal] = bLogError;
        }
        public static void OnReceiveLog(string content, bad_log.ELoggerType logType)
        {
            LogInfo li = new LogInfo() { Content = content, LogType = logType };
            li.CollapseCnt = 1;
            CLogInfo cli = LogInfo2CLogInfo(li);
            LogStore.Add(li);
            SaveLogStoreInfo(cli);

            bool newAdded = false;
            int cnt = Dict.ContainsKey(content) ? Dict[content] : 0;
            if (cnt > 0)
            {
                int idx = cnt - 1;

                CollapseLogStore[idx].CollapseCnt++;
                SetCollapsedCount(idx, CollapseLogStore[idx].CollapseCnt);
            }
            else
            {
                Dict.Add(content, CollapseLogStore.Count + 1);
                CollapseLogStore.Add(li);
                PushCollapsedLog(cli);
                newAdded = true;
            }

            if ((StaticStorage.bCollapse && newAdded) || (!StaticStorage.bCollapse))
            {
                if (LogTypePass(logType) && (string.IsNullOrEmpty(FilterText) || li.Content.ToLower().Contains(FilterText)))
                {
                    logs.Add(li);
                }
            }
        }
        static bool LogTypePass(bad_log.ELoggerType logType)
        {
            return StaticStorage.LogValueArray[(int)logType];
        }
        public static void RefreshShownLog()
        {
            RefreshShownLog("");
        }
        static string FilterText = "";
        public static void RefreshShownLog(string filter)
        {
            logs.Clear();
            ErrorCnt = 0;
            WarnCnt = 0;
            InfoCnt = 0;

            FilterText = filter.ToLower();
            bool bfilter = !string.IsNullOrEmpty(filter);
            List<LogInfo> storage = StaticStorage.bCollapse ? StaticStorage.CollapseLogStore : StaticStorage.LogStore;
            foreach (var it in storage)
            {
                if (LogTypePass(it.LogType))
                {
                    if (!bfilter || it.Content.ToLower().Contains(FilterText))
                    {
                        logs.Add(it);
                    }
                }

                ELoggerType logType = (ELoggerType)it.LogType;
                if (logType == ELoggerType.Error || logType == ELoggerType.Fatal)
                    ErrorCnt++;
                if (logType == ELoggerType.Warn)
                    ErrorCnt++;
                if (logType == ELoggerType.Debug || logType == ELoggerType.Info)
                    InfoCnt++;
            }
        }

        static bool LogTypePass(int logType)
        {
            return LogTypePass((ELoggerType)logType);
        }
        public static void ClearStorage()
        {
            ErrorCnt = 0;
            WarnCnt = 0;
            InfoCnt = 0;
            logs.Clear();
            Dict.Clear();
            LogStore.Clear();
            CollapseLogStore.Clear();
            ClearAllStorage();
        }
        static LogInfo CLogInfo2LogInfo(CLogInfo info)
        {
            return new LogInfo() { CollapseCnt = info.CollapseCnt, Content = info.Content, LogType = (ELoggerType)info.LogType };
        }
        static CLogInfo LogInfo2CLogInfo(LogInfo info)
        {
            return new CLogInfo() { CollapseCnt = info.CollapseCnt, Content = info.Content, LogType = (int)info.LogType };
        }
    }
}