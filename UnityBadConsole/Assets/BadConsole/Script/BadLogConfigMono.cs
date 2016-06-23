/****************************************************************************
 * （这个相当于ini配置文件:bad_log_config.prefab）
 * 用来控制日志级别
 *****************************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bad_log
{
    public class BadLogConfigMono : MonoBehaviour
    {
        [Serializable]
        public class BadLogConfigMonoDictionary : UnityDictionary<string, DictValue> { }

        [Serializable]
        public class DictValue
        {
            public string Key;
            public ELoggerType Value;
        }

        [SerializeField]
        public BadLogConfigMonoDictionary dict = new BadLogConfigMonoDictionary();
    }
}
