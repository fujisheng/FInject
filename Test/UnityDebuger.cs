using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityDebuger : IDebuger
{
    public void Log(string str)
    {
        Debug.Log($"[UnityDebuger]:{str}");
    }
}
