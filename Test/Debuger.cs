using FInject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuger
{
    [Inject]
    IDebuger debuger;

    public void Log(string msg)
    {
        debuger.Log($"[Debuger]:{msg}");
    }
}
