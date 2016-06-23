/****************************************************************************
 *  （这个用来在真机上动态修改日志级别）
 * 黑科技，比如在UI上监控点击事件（调用RecordClick()），连续点击三次就会开启此界面
 *  开启此界面之后，就可以在运行时设置日志级别
 *****************************************************************************/
#define BAD_LOG_WATCH

#if BAD_LOG_WATCH
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace bad_log
{
    class BadLogWatch : MonoBehaviour
    {
        class BadLogWatchInfo
        {
            public Type Owner;
            public MethodInfo GetMethod;
            public MethodInfo SetMethod;
        }

        List<BadLogWatchInfo> LogTypeInfos = new List<BadLogWatchInfo>();

        void Awake()
        {
            CollectWatch();
            // 关闭其他摄像机
        }
        void OnDestroy()
        { 
            // 开启awake中关闭的摄像机
        }
        void OnGUI()
        {
            yAction = 0;
            xAction = 0;

            PushActionButton("关闭", delegate()
            {
                GameObject.Destroy(this.gameObject);
            });

            for (int i = 0; i < LogTypeInfos.Count; i++)
            {
                if (LogTypeInfos[i].GetMethod == null || LogTypeInfos[i].SetMethod == null)
                    continue;

                ELoggerType oldv = (ELoggerType)LogTypeInfos[i].GetMethod.Invoke(null, null);
                PushActionButton(string.Format("{0}:{1}", LogTypeInfos[i].Owner.Name, oldv), delegate()
                {
                    int newv = (int)oldv;
                    newv++;
                    newv %= (int)ELoggerType.ALL + 1;
                    LogTypeInfos[i].SetMethod.Invoke(null, new object[]{ (ELoggerType)newv });
                });
            }
        }
        int yAction = 50;
        int xAction = 0;
        void PushActionButton(string btn_name, System.Action callback)
        {
            if (GUI.Button(new Rect(Screen.width - 230 - xAction, yAction, 150, 30), btn_name))
            {
                callback();
            }
            yAction += 50;
            if (yAction > Screen.height)
            {
                yAction = 0;
                xAction += 200;
            }
        }
        void CollectWatch()
        {
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
                            LogTypeInfos.Add(new BadLogWatchInfo() { Owner = it });
                        }
                    }
                }
            }

            foreach (var it in LogTypeInfos)
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
        }

        #region 黑科技：天启
        static float LastClickTime = 0;
        static float ClickCounter = 0;
        public static void RecordClick()
        {
            if (LastClickTime == 0)
            {
                LastClickTime = Time.realtimeSinceStartup;
            }
            if (Time.realtimeSinceStartup - LastClickTime > 1)
            {
                LastClickTime = 0;
                ClickCounter = 0;
            }
            else
            {
                ClickCounter++;
            }
            if (ClickCounter > 5)
            {
                LastClickTime = 0;
                ClickCounter = 0;

                new GameObject("BadLogWatch").AddComponent<BadLogWatch>();
            }
        }
        #endregion
    }
}

#endif