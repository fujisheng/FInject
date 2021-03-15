﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugUtils
{
    [Inject]
    static IDebuger debuger;

    public static void Log(string str)
    {
        debuger.Log(str);
    } 
}
