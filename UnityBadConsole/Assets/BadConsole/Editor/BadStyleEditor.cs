using UnityEditor;
using UnityEngine;

namespace bad_log
{
    public static class BadStyleEditor
    {
        [MenuItem("bad/create-style")]
        public static void CreateStyle()
        {

            BadStyle styles = AssetDatabase.LoadAssetAtPath("Assets/BadConsole/bad-styles.asset", typeof(BadStyle)) as BadStyle;
            if (styles == null)
            {
                BadStyle bs = new BadStyle();
                AssetDatabase.CreateAsset(bs, "Assets/BadConsole/bad-styles.asset");
            }
        }

    }
}
