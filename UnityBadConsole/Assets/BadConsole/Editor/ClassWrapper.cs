using System;
using System.Reflection;

namespace bad_log
{
    public static class ClassWrapper
    {
        // 譬如className为"UnityEditor.AnimationWindow"
        public static object GetObject(string className, params object[] args)
        {
            Type t = GetType(className);

            return System.Activator.CreateInstance(t, args);
        }

        public static Type GetType(string className)
        {
            Type t = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = assembly.GetType(className, false, false);
                if (t != null)
                {
                    break;
                }
            }

            return t;
        }
        public static void CallFunctionImpl(object obj, string func, params object[] args)
        {
            if (obj == null)
                return;

            Type t = obj.GetType();

            CallFunctionImpl(obj, t, func, args);
        }
        public static void CallStaticFunction(Type t, string func, params object[] args)
        {
            CallFunctionImpl(null, t, func, args);
        }
        public static void CallStaticFunction(string t, string func, params object[] args)
        {
            CallStaticFunction(GetType(t), func, args);
        }
        public static MethodInfo GetMethod(string t, string func)
        {
            return GetMethod(GetType(t), func);
        }
        public static MethodInfo GetMethod(Type t, string func)
        {
            if (t == null)
            {
                return null;
            }

            MethodInfo method = null;
            do
            {
                method = t.GetMethod(func, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (method != null)
                {
                    break;
                }
                t = t.BaseType;
            }
            while (t != null);

            return method;
        }
        public static MethodInfo GetMethod(string t, string func, Type[] types)
        {
            return GetMethod(GetType(t), func, types);
        }
        public static MethodInfo GetMethod(Type t, string func, Type[] types)
        {
            if (t == null)
            {
                return null;
            }
            Type bt = t;
            MethodInfo method = null;
            do
            {
                method = t.GetMethod(func, types);
                if (method != null)
                {
                    break;
                }
                t = t.BaseType;
            }
            while (t != null);

            if (method == null)
            {
                MethodInfo[] m = bt.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var it in m)
                {
                    if (it.Name == func)
                    {
                        ParameterInfo[] args = it.GetParameters();
                        bool bmatch = true;
                        for (int i = 0; i < types.Length; i++)
                        {
                            if (types[i] != args[i].ParameterType)
                            {
                                bmatch = false;
                            }
                        }
                        if (bmatch)
                        {
                            method = it;
                            break;
                        }
                    }
                }
            }

            return method;
        }
        public static MethodInfo GetStaticMethod(string t, string func)
        {
            return GetStaticMethod(GetType(t), func);
        }
        public static MethodInfo GetStaticMethod(Type t, string func)
        {
            if (t == null)
            {
                return null;
            }

            MethodInfo method = null;
            do
            {
                method = t.GetMethod(func, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (method != null)
                {
                    break;
                }
                t = t.BaseType;
            }
            while (t != null);

            return method;
        }
        static void CallFunctionImpl(object obj, Type t, string func, params object[] args)
        {
            if (t == null)
                return;

            MethodInfo method = obj == null ? GetStaticMethod(t, func) : GetMethod(t, func);

            if (method != null)
            {
                method.Invoke(obj, args);
            }
        }

        public static PropertyInfo GetStaticProperty(string type, string prop_name)
        {
            Type t = GetType(type);

            if (t == null)
                return null;

            return t.GetProperty(prop_name);
        }
    }
}
