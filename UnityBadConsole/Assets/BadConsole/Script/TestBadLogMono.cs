using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bad_log
{
    public class BadLog11 : TBadLog<BadLog11>
    { }
    public class BadLog12 : TBadLog<BadLog12>
    { }
    public class BadLog13 : TBadLog<BadLog13>
    { }
    public class BadLog14 : TBadLog<BadLog14>
    { }
    public class BadLog15 : TBadLog<BadLog15>
    { }
    public class BadLog16 : TBadLog<BadLog16>
    { }
    public class BadLog17 : TBadLog<BadLog17>
    { }
    public class BadLog18 : TBadLog<BadLog18>
    { }
    public class BadLog19 : TBadLog<BadLog19>
    { }
    public class TestBadLogMono : MonoBehaviour
    {
        public List<IBadLog> logs = new List<IBadLog>();
        
        void Start()
        {
            BadLog1.I("123");
            BadLog2.I("123");


            BadLog1.E("123 123");
            BadLog2.E("123 123");


            BadLog3.E("123");
            BadLog4.I("123");
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 50, 20), "测试"))
            {
                Profiler.BeginSample("a1000");
                for (int i = 0; i < 1000; i++)
                {
                    BadLog1.F("12345");
                }
                Profiler.EndSample();
            }
            if (GUI.Button(new Rect(10, 50, 50, 20), "测试2"))
            {
                Profiler.BeginSample("b1000");
                for (int i = 0; i < 1000; i++)
                {
                    Debug.LogError("12345");
                }
                Profiler.EndSample();
            }
        }
    }
}
