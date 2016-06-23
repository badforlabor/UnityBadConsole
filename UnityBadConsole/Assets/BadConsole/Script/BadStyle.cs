using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bad_log
{
    public class BadStyle : ScriptableObject
    {
        public List<Texture2D> Icons = new List<Texture2D>();
        public List<GUIStyle> Styles = new List<GUIStyle>();
    }
}
