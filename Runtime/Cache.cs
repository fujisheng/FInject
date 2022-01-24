using System;
using System.Collections.Generic;
using System.Reflection;

namespace FInject
{
    internal static class Cache
    {
        static readonly BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static;
        static readonly Dictionary<Type, List<FieldInfo>> fieldCache = new Dictionary<Type, List<FieldInfo>>();
        static readonly Dictionary<Type, List<MethodInfo>> methodCache = new Dictionary<Type, List<MethodInfo>>();
        static readonly Dictionary<Type, ConstructorInfo> ctorCache = new Dictionary<Type, ConstructorInfo>();
        static readonly Dictionary<Type, List<PropertyInfo>> propertyCache = new Dictionary<Type, List<PropertyInfo>>();
        static readonly Type InjectAttributeType = typeof(InjectAttribute);

        /// <summary>
        /// 获取添加了Inject的FieldInfo
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static List<FieldInfo> GetFieldInfos(this Type type)
        {
            if(fieldCache.TryGetValue(type, out List<FieldInfo> fieldInfos))
            {
                return fieldInfos;
            }

            fieldInfos = new List<FieldInfo>();
            foreach (var fieldInfo in type.GetFields(bindingFlags))
            {
                if(!fieldInfo.IsDefined(InjectAttributeType, true))
                {
                    continue;
                }

                fieldInfos.Add(fieldInfo);
            }
            fieldCache.Add(type, fieldInfos);
            return fieldInfos;
        }

        /// <summary>
        /// 获取添加了Inject的MethodInfo
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static List<MethodInfo> GetMethodInfos(this Type type)
        {
            if(methodCache.TryGetValue(type, out List<MethodInfo> methodInfos))
            {
                return methodInfos;
            }

            methodInfos = new List<MethodInfo>();
            foreach (var methodInfo in type.GetMethods(bindingFlags))
            {
                if(!methodInfo.IsDefined(InjectAttributeType, true))
                {
                    continue;
                }

                var parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length != 1)
                {
                    continue;
                }

                methodInfos.Add(methodInfo);
            }
            methodCache.Add(type, methodInfos);
            return methodInfos;
        }

        /// <summary>
        /// 获取添加了Inject的PropertyInfo
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static List<PropertyInfo> GetPropertyInfos(this Type type)
        {
            if(propertyCache.TryGetValue(type, out List<PropertyInfo> propertyInfos))
            {
                return propertyInfos;
            }

            propertyInfos = new List<PropertyInfo>();
            foreach (var propertyInfo in type.GetProperties(bindingFlags))
            {
                if(!propertyInfo.IsDefined(InjectAttributeType, true))
                {
                    continue;
                }
                propertyInfos.Add(propertyInfo);
            }
            propertyCache.Add(type, propertyInfos);
            return propertyInfos;
        }

        /// <summary>
        /// 获取添加了Inject的ConstructorInfo
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static ConstructorInfo GetConstructorInfo(this Type type)
        {
            if(ctorCache.TryGetValue(type, out ConstructorInfo constructorInfo))
            {
                return constructorInfo;
            }

            foreach (var ctorInfo in type.GetConstructors(bindingFlags))
            {
                if(!ctorInfo.IsDefined(InjectAttributeType, true))
                {
                    continue;
                }

                var parameterInfos = ctorInfo.GetParameters();
                if (parameterInfos.Length != 1)
                {
                    continue;
                }

                constructorInfo = ctorInfo;
                ctorCache.Add(type, constructorInfo);
                break;
            }

            return constructorInfo;
        }
    }
}