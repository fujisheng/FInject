using UnityEngine;

public class NetDebuger : IDebuger
{
    public void Log(string str)
    {
        Debug.Log($"[NetDebuger]:{str}");
    }
}
