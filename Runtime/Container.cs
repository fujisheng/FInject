using System;
using System.Collections.Generic;

namespace FInject
{
    /// <summary>
    /// 依赖注入容器 保存注入的关系
    /// </summary>
    public class Container
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
            var get = bindMapping.TryGetValue(type, out List<BindInfo> bindInfos);
            var bindInfo = BindInfoPool.Pop();
            bindInfo.originType = type;
            if (!get)
            {
                if (bindInfos == null)
                {
                    bindInfos = new List<BindInfo>();
                }
                bindInfos.Add(bindInfo);
                bindMapping.Add(type, bindInfos);
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
            if(!bindMapping.TryGetValue(originType, out var bindInfos))
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