using System;
using System.Collections.Generic;

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
        /// <param name="originType">要被注入的类型</param>
        /// <param name="containerType">被注入的类型所在的类型</param>
        /// <returns></returns>
        internal BindInfo GetBindInfo(Type originType, Type containerType)
        {
            var get = bindMapping.TryGetValue(originType, out List<BindInfo> bindInfos);
            if (!get)
            {
                return null;
            }

            if(bindInfos.Count <= 0)
            {
                return null;
            }

            //TODO 优化  修bug 当只有一条绑定信息的时候 无论如何都能选择到注入信息
            bindInfos.Sort((l, r) =>
            {
                var linfo = CheckBindInfo(l, containerType);
                var rinfo = CheckBindInfo(r, containerType);
                if(linfo.sameCont != rinfo.sameCont)
                {
                    return linfo.sameCont == true ? 1 : -1;
                }
                else if(linfo.checkerT != rinfo.checkerT)
                {
                    return linfo.checkerT == true ? 1 : -1;
                }
                else if(linfo.hasInst != rinfo.hasInst)
                {
                    return linfo.hasInst == true ? 1 : -1;
                }
                else
                {
                    return 1;
                }
            });

            return bindInfos[0];
        }

        /// <summary>
        /// 检查信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="containerType"></param>
        /// <returns></returns>
        (bool sameCont, bool checkerT, bool hasInst) CheckBindInfo(BindInfo info, Type containerType)
        {
            var sameCont = info.containerType == containerType;
            var checkerT = info.checker != null && info.checker.Invoke(containerType);
            var hasInst = info.instance != null;
            return (sameCont, checkerT, hasInst);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            foreach(var kv in bindMapping)
            {
                var bindInfos = kv.Value;
                foreach(var bindInfo in bindInfos)
                {
                    BindInfoPool.Push(bindInfo);
                }
            }

            bindMapping.Clear();
        }
    }
}