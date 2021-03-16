using System;
using System.Collections.Generic;
using System.Reflection;

namespace FInject
{
    /// <summary>
    /// 依赖注入器
    /// </summary>
    public class Injecter
    {
        List<object> injectedCache = new List<object>();
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

            foreach(var injected in injectedCache)
            {
                Inject(injected);
            }
        }

        /// <summary>
        /// 根据实例注入
        /// </summary>
        /// <typeparam name="T1">实例类型</typeparam>
        /// <param name="instance">实例</param>
        public void Inject<T1>(T1 instance)
        {
            Inject(instance);
        }

        /// <summary>
        /// 根据实例注入
        /// </summary>
        /// <param name="instance">实例</param>
        public void Inject(object instance)
        {
            InjectFields(instance);
        }

        /// <summary>
        /// 根据类型创建实例同时进行注入
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实例</returns>
        public object CreateInstance(Type type)
        {
            var instance = Activator.CreateInstance(type);
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
            var instance = Activator.CreateInstance(type, args);
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
        /// 注入字段
        /// </summary>
        /// <param name="instance">实例</param>
        void InjectFields(object instance)
        {
            var type = instance.GetType();
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.NonPublic))
            {
                foreach (var attribute in fieldInfo.GetCustomAttributes(true))
                {
                    if (attribute is InjectAttribute)
                    {
                        var bindInfo = context.GetBindInfo(fieldInfo.FieldType, type);
                        if(bindInfo == null || bindInfo.IsEmpty())
                        {
                            break;
                        }

                        var owner = fieldInfo.IsStatic ? type : instance;
                        fieldInfo.SetValue(owner, Create(bindInfo));
                        break;
                    }
                }
            }

            bool replace = false;
            for(int i = 0; i < injectedCache.Count; i++)
            {
                var injected = injectedCache[i];
                if(instance == injected)
                {
                    injectedCache[i] = instance;
                    replace = true;
                    break;
                }
            }

            if (!replace)
            {
                injectedCache.Add(instance);
            }
        }

        /// <summary>
        /// 通过这个创建的实例在不使用的时候需要通过这个来进行释放 否则引用会一直存在
        /// </summary>
        /// <param name="instance">实例</param>
        public void Release(object instance)
        {
            if (injectedCache.Contains(instance))
            {
                injectedCache.Remove(instance);
            }
        }
    }
}