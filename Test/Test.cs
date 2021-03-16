using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FInject;

public class Test : MonoBehaviour
{
    private void Start()
    {
        Context.Bind<IDebuger, UnityDebuger>(typeof(DebugUtils));


        DebugUtils.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        //var debugUtils = Context.Create<DebugUtils>();
        //debugUtils.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}
