﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace FInject
{
    /// <summary>
    /// 依赖注入器
    /// </summary>
    public class Injecter
    {
        List<(Type, object)> injectedCache = new List<(Type, object)>();
        Context context;

        /// <summary>
        /// 构造方法 需传入上下文
        /// </summary>
        /// <param name="context">上下文</param>
        public Injecter(Context context)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.context = context;
        }

        //TODO  切换的时候有bug 本来另外一个context没有这个注入信息的也会注入进去
        /// <summary>
        /// 切换上下文 会将已经注入的重新注入
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="releaseOldContext">是否释放之前的上下文</param>
        public void SwitchContext(Context context, bool releaseOldContext = false)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (releaseOldContext)
            {
                this.context.Release();
            }

            this.context = context;

            for(int i = 0; i < injectedCache.Count; i++)
            {
                var injected = injectedCache[i];
                Unject(injected.Item1, injected.Item2);
                Inject(injected.Item1, injected.Item2);
            }
        }

        /// <summary>
        /// 注入的入口
        /// </summary>
        /// <param name="type">为哪个类型注入</param>
        /// <param name="instance">要注入的实例</param>
        void Inject(Type type, object instance)
        {
            InjectWithFields(type, instance);
            InjectWithMethod(type, instance);
            InjectWithPropertys(type, instance);
        }

        /// <summary>
        /// 取消注入的入口
        /// </summary>
        /// <param name="type">为哪个类型取消注入</param>
        /// <param name="instance">要取消的实例</param>
        void Unject(Type type, object instance)
        {
            UnjectFields(type, instance);
        }

        /// <summary>
        /// 根据实例注入
        /// </summary>
        /// <typeparam name="T1">实例类型</typeparam>
        /// <param name="instance">实例</param>
        public void Inject<T1>(T1 instance)
        {
            Inject(typeof(T1), instance);
        }

        /// <summary>
        /// 根据实例注入
        /// </summary>
        /// <param name="instance">实例</param>
        public void Inject(object instance)
        {
            Inject(instance.GetType(), instance);
        }

        /// <summary>
        /// 为静态类注入
        /// </summary>
        /// <param name="type">静态类型</param>
        public void Inject(Type type)
        {
            if (!type.IsStatic())
            {
                throw new Exception($"type {type.FullName} is not static");
            }
            Inject(type, null);
        }

        /// <summary>
        /// 根据类型创建实例同时进行注入
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实例</returns>
        public object CreateInstance(Type type)
        {
            var instance = InjectWithConstructor(type);
            if (instance == null)
            {
                instance = Activator.CreateInstance(type);
            }

            Inject(instance);
            return instance;
        }

        /// <summary>
        /// 根据类型创建实例同时进行注入
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="args">参数</param>
        /// <returns>实例</returns>
        public object CreateInstance(Type type, params object[] args)
        {
            var instance = InjectWithConstructor(type);
            if(instance == null)
            {
                instance = Activator.CreateInstance(type, args);
            }

            Inject(instance);
            return instance;
        }

        /// <summary>
        /// 根据类型创建实例同时进行注入
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <returns>实例</returns>
        public T CreateInstance<T>()
        {
            var type = typeof(T);
            return (T)CreateInstance(type);
        }

        /// <summary>
        /// 根据类型创建实例同时进行注入
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="args">参数</param>>
        /// <returns>实例</returns>
        public T CreateInstance<T>(params object [] args)
        {
            var type = typeof(T);
            return (T)CreateInstance(type, args);
        }

        /// <summary>
        /// 根据类型创建实例同时进行注入
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <returns>实例</returns>
        public T CreateInstanceWithNew<T>() where T : new()
        {
            var instance = new T();
            Inject(instance);
            return instance;
        }

        /// <summary>
        /// 根据绑定信息创建实例
        /// </summary>
        /// <param name="bindInfo"></param>
        /// <returns></returns>
        object Create(BindInfo bindInfo)
        {
            var instance = bindInfo.instance ?? CreateInstance(bindInfo.bindType);
            Inject(instance);
            return instance;
        }

        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        void CacheInjected(Type type, object instance = null)
        {
            bool replace = false;
            for (int i = 0; i < injectedCache.Count; i++)
            {
                var injected = injectedCache[i];
                if (type == injected.Item1 && instance == injected.Item2)
                {
                    injectedCache[i] = (type, instance);
                    replace = true;
                    break;
                }
            }

            if (!replace)
            {
                injectedCache.Add((type, instance));
            }
        }

        /// <summary>
        /// 通过属性注入
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例</param>
        void InjectWithPropertys(Type type, object instance = null)
        {
            foreach(var propertyInfo in Cache.GetPropertyInfos(type))
            {
                var bindInfo = context.GetBindInfo(propertyInfo.PropertyType, type);
                if (bindInfo == null || bindInfo.IsEmpty())
                {
                    continue;
                }

                var owner = propertyInfo.IsStatic() ? type : instance;
                propertyInfo.SetValue(owner, Create(bindInfo));
            }

            CacheInjected(type, instance);
        }

        /// <summary>
        /// 通过字段注入
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例</param>
        void InjectWithFields(Type type, object instance = null)
        {
            foreach(var fieldInfo in type.GetFieldInfos())
            {
                var bindInfo = context.GetBindInfo(fieldInfo.FieldType, type);
                if (bindInfo == null || bindInfo.IsEmpty())
                {
                    continue;
                }

                var owner = fieldInfo.IsStatic ? type : instance;
                fieldInfo.SetValue(owner, Create(bindInfo));
            }

            CacheInjected(type, instance);
        }

        /// <summary>
        /// 通过方法注入
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例</param>
        void InjectWithMethod(Type type, object instance = null)
        {
            foreach(var methodInfo in type.GetMethodInfos())
            {
                var parameterInfos = methodInfo.GetParameters();
                var bindInfo = context.GetBindInfo(parameterInfos[0].ParameterType, type);
                if (bindInfo == null || bindInfo.IsEmpty())
                {
                    continue;
                }

                var owner = methodInfo.IsStatic ? type : instance;
                methodInfo.Invoke(owner, new object[] { Create(bindInfo) });
            }

            CacheInjected(type, instance);
        }

        /// <summary>
        /// 通过构造方法注入
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>注入后的实例</returns>
        object InjectWithConstructor(Type type)
        {
            var ctorInfo = type.GetConstructorInfo();
            if(ctorInfo == null)
            {
                return null;
            }
            var parameterInfos = ctorInfo.GetParameters();
            var bindInfo = context.GetBindInfo(parameterInfos[0].ParameterType, type);
            if (bindInfo == null || bindInfo.IsEmpty())
            {
                return null;
            }

            return ctorInfo.Invoke(new object[] { Create(bindInfo) });
        }

        /// <summary>
        /// 取消注入字段
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例</param>
        void UnjectFields(Type type, object instance = null)
        {
            foreach(var fieldInfo in type.GetFieldInfos())
            {
                var bindInfo = context.GetBindInfo(fieldInfo.FieldType, type);
                if (bindInfo == null || bindInfo.IsEmpty())
                {
                    continue;
                }

                var owner = fieldInfo.IsStatic ? type : instance;
                fieldInfo.SetValue(owner, null);
            }
        }

        /// <summary>
        /// 通过这个创建的实例在不使用的时候需要通过这个来进行释放 否则引用会一直存在
        /// </summary>
        /// <param name="instance">实例</param>
        public void Release(object instance)
        {
            for(int i = 0; i < injectedCache.Count; i++)
            {
                var data = injectedCache[i];
                if(data.Item2 == instance)
                {
                    injectedCache.Remove(data);
                }
            }
        }
    }
}