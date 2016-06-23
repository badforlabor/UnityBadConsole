using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace bad_log
{
    [CustomEditor(typeof(BadLogDebugger))]
    class BadLogDebuggerEditor : Editor
    {
        class BadLogWatchInfo
        {
            public Type Owner;
            public MethodInfo GetMethod;
            public MethodInfo SetMethod;
        }
        static List<BadLogWatchInfo> LogTypeInfos = new List<BadLogWatchInfo>();

        void OnEnable()
        {
            if (LogTypeInfos.Count == 0)
            {
                LogTypeInfos = CollectWatch();
            }
        }
        static bool foldout = false;
        public override void OnInspectorGUI()
        {
            foldout = EditorGUILayout.Foldout(foldout, "内容");
            if (foldout)
            {
                foreach (var it in LogTypeInfos)
                {
                    if (it.Owner == null || it.GetMethod == null || it.SetMethod == null)
                        continue;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(it.Owner.Name);
                    ELoggerType oldv = (ELoggerType)it.GetMethod.Invoke(null, null);
                    ELoggerType newv = (ELoggerType)EditorGUILayout.EnumPopup(oldv);
                    if (newv != oldv)
                    {
                        it.SetMethod.Invoke(null, new object[] { newv });
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        List<BadLogWatchInfo> CollectWatch()
        {
            List<BadLogWatchInfo> logTypeInfos = new List<BadLogWatchInfo>();

            Assembly[] assems = System.AppDomain.CurrentDomain.GetAssemblies();
            if (assems != null)
            {
                for (int i = 0; i < assems.Length; i++)
                {
                    Type[] types = assems[i].GetTypes();
                    foreach (var it in types)
                    {
                        // it.IsGenericType表示泛型类，比如TBadLog<T,T1>，忽略掉。
                        if (!it.IsGenericType && it.GetInterface(typeof(IBadLog).Name) != null)
                        {
                            logTypeInfos.Add(new BadLogWatchInfo() { Owner = it });
                        }
                    }
                }
            }

            foreach (var it in logTypeInfos)
            {
                Type t = it.Owner;
                do
                {
                    // 囧，不知道为啥默认的类取不到这两个Static方法，只能循环遍历父类
                    it.GetMethod = t.GetMethod("GetLogType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    it.SetMethod = t.GetMethod("SetLogType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    t = t.BaseType;
                }
                while (t != null && (it.GetMethod == null || it.SetMethod == null));

                if (it.GetMethod == null || it.SetMethod == null)
                {
                    Debug.LogError("error bad log type, type=" + it.Owner.Name);
                }
                else
                {
                    Debug.Log("bad log type, info=" + it.Owner.Name + ", default log type=" + it.GetMethod.Invoke(null, new object[] { }));
                }
            }

            return logTypeInfos;
        }

    }
}
