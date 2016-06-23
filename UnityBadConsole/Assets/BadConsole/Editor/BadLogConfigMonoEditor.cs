using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using bad_log;

[CustomEditor(typeof(BadLogConfigMono))]
public class BadLogConfigMonoEditor : Editor 
{
    class DictValueWrapper
    {
        public string Key;
        public BadLogConfigMono.DictValue Value;
    }

    GUIStyle[] InspectorStyleBlue = new GUIStyle[2];

    static Dictionary<object, bool> FoldOutDict = new Dictionary<object,bool>();

    BadLogConfigMono me = null;
    List<DictValueWrapper> SavedDict = new List<DictValueWrapper>();

    void OnEnable()
    {
        if (InspectorStyleBlue[0] == null)
        {
            InspectorStyleBlue[0] = GetStyle("InspectorStyleBlue");
        }
        if (InspectorStyleBlue[1] == null)
        {
            InspectorStyleBlue[1] = GetStyle("InspectorStyleGray");
        }

        me = target as BadLogConfigMono;

        SavedDict = new List<DictValueWrapper>();
        foreach (var it in me.dict)
        {
            SavedDict.Add(new DictValueWrapper() { Key = it.Key, Value = it.Value });
        }
    }

    public static GUIStyle GetStyle(string key)
    {
        BadStyle styles = AssetDatabase.LoadAssetAtPath("Assets/BadConsole/bad-styles.asset", typeof(BadStyle)) as BadStyle;
        return styles == null ? null : styles.Styles.Find(x => x.name == key);
    }

    public override void OnInspectorGUI()
    {
//         bool pressed_enter = false;
//         if (Event.current != null && Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return)
//         {
//             pressed_enter = true;
//         }

        DictValueWrapper del = null;

        EditorGUILayout.BeginVertical();
        EditorGUI.indentLevel++;
        FoldOutDict[me.dict] = EditorGUILayout.Foldout(GetFoldout(me.dict), "dict");
        if (FoldOutDict[me.dict])
        {
            EditorGUILayout.Space();
            int idx = 0;
            foreach (var it in SavedDict)
            {
                idx++;

                EditorGUI.indentLevel++;

                GUIStyle baseStyle = InspectorStyleBlue[idx % 2];
                if (baseStyle != null)
                {
                    if (GUILayout.Button("" + it.Key, baseStyle))
                    {
                        FoldOutDict[it.Key] = !GetFoldout(it.Key);
                    }
                }
                else
                {
                    if (GUILayout.Button("" + it.Key))
                    {
                        FoldOutDict[it.Key] = !GetFoldout(it.Key);
                    }
                }

                EditorGUI.indentLevel++;
                if (GetFoldout(it.Key))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    
                    string newKey = it.Key;
                    newKey = EditorGUILayout.TextField("Key", it.Key);

                    it.Value.Key = EditorGUILayout.TextField("Alias", it.Value.Key);

                    it.Value.Value = (ELoggerType)EditorGUILayout.EnumPopup("LogType", it.Value.Value);

                    EditorGUILayout.EndVertical();
                    if (GUILayout.Button("-"))
                    {
                        del = it; 
                    }
                    EditorGUILayout.EndHorizontal();

                    if (newKey != it.Key)
                    {
                        me.dict.Remove(it.Key);
                        me.dict.Add(newKey, it.Value);

                        var oldv = FoldOutDict[it.Key];
                        FoldOutDict.Remove(it.Key);
                        FoldOutDict.Add(newKey, oldv);

                        it.Key = newKey;
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
            if (GUILayout.Button("+"))
            {
                string key = "new key";
                while (me.dict.ContainsKey(key))
                {
                    key += "1";
                }
                me.dict.Add(key, new BadLogConfigMono.DictValue() { Key = key + " alias", Value = ELoggerType.Fatal });
                SavedDict.Add(new DictValueWrapper() { Key = key, Value = me.dict[key] });
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();

        if (del != null)
        {
            me.dict.Remove(del.Key);
            SavedDict.Remove(del);
        }


    }
    public bool GetFoldout(object key)
    {
        if (!FoldOutDict.ContainsKey(key))
        {
            FoldOutDict.Add(key, false);
        }
        return FoldOutDict[key];
    }
}
