using System;
using System.Collections.Generic;
using System.Reflection;

namespace FInject
{
    /// <summary>
    /// 依赖注入的上下文 保存注入的上下关系
    /// </summary>
    public class Context
    {
        Dictionary<Type, List<BindInfo>> bindMapping = new Dictionary<Type, List<BindInfo>>();

        /// <summary>
        /// 绑定要注入的类型
        /// </summary>
        /// <typeparam name="T1">要被注入的类型</typeparam>
        /// <returns>绑定信息</returns>
        public BindInfo Bind<T1>()
        {
            var type = typeof(T1);
            return Bind(type);
        }

        /// <summary>
        /// 绑定要注入的类型
        /// </summary>
        /// <param name="type">要被注入的类型</param>
        /// <returns>绑定信息</returns>
        public BindInfo Bind(Type type)
        {
            return Bind(type, null);
        }

        /// <summary>
        /// 指定某个类型中绑定要注入的类型
        /// </summary>
        /// <typeparam name="T1">要被注入的类型</typeparam>
        /// <typeparam name="T2">指定在这个类型下才采用这种注入方式</typeparam>
        /// <returns></returns>
        public BindInfo Bind<T1, T2>()
        {
            var type1 = typeof(T1);
            var type2 = typeof(T2);
            return Bind(type1, type2);
        }

        /// <summary>
        /// 指定某个类型中绑定要注入的类型
        /// </summary>
        /// <param name="type1">要被注入的类型</param>
        /// <param name="type2">指定在这个类型下才采用这种注入方式</param>
        /// <returns></returns>
        public BindInfo Bind(Type type1, Type type2)
        {
            var get = bindMapping.TryGetValue(type1, out List<BindInfo> bindInfos);
            var bindInfo = BindInfoPool.Pop();
            bindInfo.originType = type1;
            bindInfo.containerType = type2;
            if (!get)
            {
                if (bindInfos == null)
                {
                    bindInfos = new List<BindInfo>();
                }
                bindInfos.Add(bindInfo);
                bindMapping.Add(type1, bindInfos);
                return bindInfo;
            }

            var replace = false;

            for (int i = 0; i < bindInfos.Count; i++)
            {
                if (bindInfos[i].Equals(bindInfo))
                {
                    bindInfos[i] = bindInfo;
                    replace = true;
                }
            }

            if (!replace)
            {
                bindInfos.Add(bindInfo);
            }

            return bindInfo;
        }

        /// <summary>
        /// 获取绑定信息
        /// </summary>
        /// <param name="originType">注入的类型</param>
        /// <param name="containerType">被注入的类型所在的类型</param>
        /// <returns></returns>
        internal BindInfo GetBindInfo(Type originType, Type containerType)
        {
            return null;
        }
    }
}