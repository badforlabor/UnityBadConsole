/****************************************************************************
 * 控制台窗口
****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace bad_log
{
    class BadConsoleWindow : EditorWindow
    {

        [MenuItem("Window/BadConsole")]
        public static void ShowMe()
        {
            GetWindow<BadConsoleWindow>("BadConsole").Show();
        }

        static internal Texture2D IconInfo, IconWarn, IconError;

        static Vector2 TextScroll;
        static Vector2 ListScroll;
        static int SelectedRow = -1;

        string SelectedText = "";

        static float ScrollListHeight = 0; // 列表显示区域的高度
        static int ScrollHeight = 32;      //  列表的行高
        string SearchText = "";

        object spl = null;
        System.Reflection.MethodInfo BeginVerticalSplit = null;
        System.Reflection.MethodInfo EndVerticalSplit = null;
        System.Reflection.PropertyInfo VisibleRect = null;
        System.Reflection.MethodInfo ToolbarSearchField = null;

        int InitedCounter = 0;
        
        void OnDisable()
        {
            EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
            bad_log.BadLogWrapper.OnLog -= OnReceiveLog;

            BeginVerticalSplit = null;
            EndVerticalSplit = null;
            VisibleRect = null;
            ToolbarSearchField = null;
        }

        void OnEnable()
        {
            // 当EditorWindow打开和关闭的时候，会执行OnEnable/OnDisable；当EditorApplication.isPlaying=true的时候，又会调用一次OnDisable/OnEnable
            InitedCounter++;

            EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
            bad_log.BadLogWrapper.OnLog += OnReceiveLog;

            StaticStorage.RefreshLogValueArray();

            SelectedRow = -1;
            if (spl == null)
            {
                spl = bad_log.ClassWrapper.GetObject("UnityEditor.SplitterState", new float[] { 70, 30 }, new int[] { 32, 32 }, null);
            }
            BeginVerticalSplit = bad_log.ClassWrapper.GetMethod("UnityEditor.SplitterGUILayout", "BeginVerticalSplit", new Type[] { spl.GetType(), typeof(GUILayoutOption) });
            EndVerticalSplit = bad_log.ClassWrapper.GetStaticMethod("UnityEditor.SplitterGUILayout", "EndVerticalSplit");
            VisibleRect = bad_log.ClassWrapper.GetStaticProperty("UnityEngine.GUIClip", "visibleRect");
            //ToolbarSearchField = bad_log.ClassWrapper.GetMethod(typeof(EditorGUILayout), "ToolbarSearchField", new Type[] { typeof(string), typeof(GUILayoutOption[]) });
            ToolbarSearchField = bad_log.ClassWrapper.GetMethod(typeof(EditorGUI), "ToolbarSearchField", new Type[] { typeof(int), typeof(Rect), typeof(string), typeof(bool) });
            LoadStyle();

            // 刚刚初始化的时候
            if (InitedCounter == 1)
            {
                StaticStorage.ClearStorage();
#if TEST
#if TEST_COLLAPSE            
            for (int i = 0; i < 100000; i++ )
                OnReceiveLog("log:" + ((i&(~0xF)) + 1), bad_log.ELoggerType.Error);
#else
                for (int i = 0; i < 100000; i++)
                    OnReceiveLog("log:" + (i + 1), bad_log.ELoggerType.Error);
#endif
                StaticStorage.RefreshShownLog(SearchText);
#endif
            }
            else
            {
                StaticStorage.Init(0);

                StaticStorage.RefreshShownLog(SearchText);
            }
        }

        void LoadStyle()
        {
            IconInfo = EditorGUIUtility.FindTexture("console.infoicon.sml");
            IconWarn = EditorGUIUtility.FindTexture("console.warnicon.sml");
            IconError = EditorGUIUtility.FindTexture("console.erroricon.sml");
        }

        void OnGUI()
        {
            bool brefresh = false;
            Event e = Event.current;

            // 上排按钮
            GUILayout.BeginHorizontal((GUIStyle)"Toolbar");

            if (GUILayout.Button("Clear", (GUIStyle)"ToolbarButton"))
            {
                StaticStorage.ClearStorage();
                GUIUtility.keyboardControl = -1;
            }
            EditorGUILayout.Space();

            bool bNewCollapse = GUILayout.Toggle(StaticStorage.bCollapse, "Collapse", (GUIStyle)"ToolbarButton");
            if (bNewCollapse != StaticStorage.bCollapse)
            {
                StaticStorage.bCollapse = bNewCollapse;
                StaticStorage.RefreshShownLog(SearchText);
                GUI.changed = false;
                brefresh = true;
            }
            StaticStorage.bClearOnPlay = GUILayout.Toggle(StaticStorage.bClearOnPlay, "Clear On Play", (GUIStyle)"ToolbarButton");
            StaticStorage.bErrorPause = GUILayout.Toggle(StaticStorage.bErrorPause, "Error Pause", (GUIStyle)"ToolbarButton");
            EditorGUILayout.Space();

            GUILayout.FlexibleSpace();

            // 搜索栏
            Rect rect = GUILayoutUtility.GetRect(0,
                (EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 5) * 1.5f, 
                16, 16, (GUIStyle)"ToolbarSeachTextField", GUILayout.MinWidth(65), GUILayout.MaxWidth(300));            
            if (ToolbarSearchField != null)
            {
                //a = (string)ToolbarSearchField.Invoke(a, new object[] { "", new GUILayoutOption[0] });
                SearchText = (string)ToolbarSearchField.Invoke(null, new object[] { 11111, rect, SearchText, false });
                if (e.type == EventType.keyUp && e.keyCode == KeyCode.Return)
                {
                    brefresh = DoSearch() | brefresh;
                    e.Use();
                }
            }
            EditorGUILayout.Space();

            GUI.changed = false;
            StaticStorage.bLogInfo = GUILayout.Toggle(StaticStorage.bLogInfo,
                new GUIContent(StaticStorage.InfoCnt > 999 ? "999+" : StaticStorage.InfoCnt.ToString(), IconInfo), (GUIStyle)"ToolbarButton");
            StaticStorage.bLogWarn = GUILayout.Toggle(StaticStorage.bLogWarn,
                new GUIContent(StaticStorage.WarnCnt > 999 ? "999+" : StaticStorage.WarnCnt.ToString(), IconWarn), (GUIStyle)"ToolbarButton");
            StaticStorage.bLogError = GUILayout.Toggle(StaticStorage.bLogError,
                new GUIContent(StaticStorage.ErrorCnt > 999 ? "999+" : StaticStorage.ErrorCnt.ToString(), IconError), (GUIStyle)"ToolbarButton");
            if (GUI.changed)
            {
                StaticStorage.RefreshLogValueArray();
                StaticStorage.RefreshShownLog(SearchText);
                brefresh = true;
                GUI.changed = false;
            }
            GUILayout.EndHorizontal();

            // 下面是列表和文本选择

            if (BeginVerticalSplit != null)
                BeginVerticalSplit.Invoke(null, new object[] {spl, null});

            // 上排是列表
            ListScroll = GUILayout.BeginScrollView(ListScroll, (GUIStyle)"CN Box");
            Rect visibleRect = new Rect();
            if (VisibleRect != null)
            {
                visibleRect = (Rect)VisibleRect.GetValue(null, new object[] { });
            }
            if (visibleRect.height > 0)
            {
                ScrollListHeight = visibleRect.height;
            }

            bool bcalc = e.type == EventType.mouseDown && e.button == 0 && e.clickCount == 1;
            Rect pos0 = GUILayoutUtility.GetRect(1, ScrollHeight * StaticStorage.logs.Count + 3, GUILayout.ExpandWidth(true));
            Rect pos = new Rect(pos0.x, pos0.y, pos0.width, ScrollHeight);

            GUIContent TempContent = new GUIContent();
            for (int i = 0; i < StaticStorage.logs.Count; i++)
            {
                if (bcalc)
                {
                    if (e.mousePosition.y >= ListScroll.y && e.mousePosition.y < ListScroll.y + ScrollListHeight)
                    {
                        SelectedRow = (int)e.mousePosition.y / ScrollHeight;
                        Event.current.Use();
                        GUIUtility.keyboardControl = -1;
                        if (SelectedRow >= 0 && SelectedRow < StaticStorage.logs.Count)
                        {
                            SelectedText = StaticStorage.logs[SelectedRow].Content;
                        }
                    }
                    bcalc = !bcalc;
                }

                if (e.type == EventType.repaint && pos.y >= (ListScroll.y - ScrollHeight) && pos.y < (ListScroll.y + ScrollListHeight + ScrollHeight))
                {
                    // 中排列表
                    GUIStyle s = i % 2 == 0 ? "CN EntryBackEven" : "CN EntryBackodd";
                    GUIStyle errorStyle = GetStyle((ELoggerType)StaticStorage.logs[i].LogType);

                    s.Draw(pos, false, false, SelectedRow == i, false);
                    TempContent.text = string.Format("{0}\n{1}", StaticStorage.logs[i].Content, StaticStorage.logs[i].Content);
                    errorStyle.Draw(pos, TempContent, 0, SelectedRow == i);

                    if (StaticStorage.bCollapse)
                    {
                        GUIStyle CountStyle = "CN CountBadge";
                        TempContent.text = StaticStorage.logs[i].CollapseCnt.ToString();
                        Vector2 a = CountStyle.CalcSize(TempContent);
                        Rect count_pos = pos;
                        count_pos.xMin = count_pos.xMax - a.x;
                        count_pos.yMin += (count_pos.height - a.y) * 0.5f;  // 居中显示
                        count_pos.x -= 5;                        
                        GUI.Label(count_pos, TempContent, CountStyle);
                    }
                }
                pos.y += ScrollHeight;
            }
            GUILayout.EndScrollView();
            
            // 下排文本选择
            TextScroll = GUILayout.BeginScrollView(TextScroll, (GUIStyle)"CN Box");
            EditorGUILayout.SelectableLabel(SelectedText, (GUIStyle)"CN Message", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            if (EndVerticalSplit != null)
                EndVerticalSplit.Invoke(null, new object[]{});

            if (brefresh)
            {
                StaticStorage.RefreshShownLog(SearchText);
            }
        }

        void OnPlayModeStateChanged()
        {
            if (StaticStorage.bClearOnPlay && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                StaticStorage.ClearStorage();
            }
        }
        void OnReceiveLog(string content, bad_log.ELoggerType logType)
        {
            Profiler.BeginSample("OnReceiveLog");
            StaticStorage.OnReceiveLog(content, logType);

            if (StaticStorage.bErrorPause && (logType == bad_log.ELoggerType.Error || logType == bad_log.ELoggerType.Fatal))
            {
                EditorApplication.isPaused = true;
            }
            Repaint();
            Profiler.EndSample();
        }
        string OldSearchText = "";
        bool DoSearch()
        {
            if (OldSearchText != SearchText)
            {
                OldSearchText = SearchText;
                return true;
            }
            return false;
        }
        GUIStyle GetStyle(ELoggerType logType)
        {
            switch (logType)
            { 
                case ELoggerType.Fatal:
                case ELoggerType.Error:
                    return "CN EntryError";
                case ELoggerType.Warn:
                    return "CN EntryWarn";
                default:
                    return "CN EntryInfo";
            }
            
        }
    }
}
