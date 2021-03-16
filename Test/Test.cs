using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FInject;

public class Test : MonoBehaviour
{
    private void Start()
    {
        Context context = new Context();
        context.Bind<IDebuger>().WithInstance(new UnityDebuger());
        context.Bind<IDebuger>().To<NetDebuger>().Where((type) => type == typeof(DebugUtils) );
        context.Bind<IDebuger, Debuger>().To<UnityDebuger>();


        DebugUtils.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        //var debugUtils = Context.Create<DebugUtils>();
        //debugUtils.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}
