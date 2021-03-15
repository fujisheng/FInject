using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public static class Context
{
    static Dictionary<Type, Type> bindMapping = new Dictionary<Type, Type>();

    public static void Bind<T1, T2>()
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);

        if (!type1.IsAssignableFrom(type2))
        {
            throw new Exception($"{type2.FullName} is not assignable from {type1.FullName}");
        }

        var get = bindMapping.TryGetValue(type1, out Type type);
        if (!get)
        {
            bindMapping.Add(type1, type2);
        }
        else
        {
            throw new Exception($"{type1.FullName} is already bind to {type2.FullName}");
        }
    } 

    public static void Bind<T1, T2>(Type staticType)
    {
        Bind<T1, T2>();

        foreach (var fieldInfo in staticType.GetFields(/*BindingFlags.Instance | BindingFlags.GetField | BindingFlags.IgnoreCase |*/ BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
        {
            foreach (var attribute in fieldInfo.GetCustomAttributes(true))
            {
                if (attribute is InjectAttribute)
                {
                    fieldInfo.SetValue(staticType, Create(GetBindType(fieldInfo.FieldType)));
                }
            }
        }
    }

    static Type GetBindType(Type type)
    {
        var get = bindMapping.TryGetValue(type, out Type bindType);
        if (!get)
        {
            throw new Exception($"{type.FullName} is not bind any Type");
        }
        return bindType;
    }

    static object Create(Type type)
    {
        var obj = Activator.CreateInstance(type);
        foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.NonPublic))
        {
            foreach (var attribute in fieldInfo.GetCustomAttributes(true))
            {
                if (attribute is InjectAttribute)
                {
                    fieldInfo.SetValue(obj, Create(GetBindType(fieldInfo.FieldType)));
                }
            }
        }
        return obj;
    }

    public static T CreateInstance<T>()
    {
        var type = typeof(T);
        return (T)Create(type);
    }
}
