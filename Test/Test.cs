using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FInject;

public class Test : MonoBehaviour
{
    private void Start()
    {
        Context context = new Context();
        context.Bind<IDebuger>().To<NetDebuger>().Where((type) => type == typeof(Debuger));

        Injecter injecter = new Injecter(context);
        var debuger = injecter.CreateInstance<Debuger>();
        debuger.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        Context context2 = new Context();
        context2.Bind<IDebuger>().To<UnityDebuger>().Where((type) => type == typeof(DebugUtils));
        Injecter injecter2 = new Injecter(context2);
        injecter2.Inject(typeof(DebugUtils));
        DebugUtils.Log("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

        injecter2.SwitchContext(context);
        DebugUtils.Log("cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc");


        //DebugUtils.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        //var debugUtils = Context.Create<DebugUtils>();
        //debugUtils.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}
