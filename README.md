#c#版依赖注入框架 目前仅仅支持字段注入 后续添加其他注入方式
##具体依赖注入的概念自行百度
##其目的是为了方便修改具体实现并且不会侵入原有的代码
##目前此框架实现较为简单，具体性能还未测试，仅仅用于学习如何简单的实现依赖注入

```
//定义一个IDebuger接口
public interface IDebuger
{
    void Log(string str);
}

//定义UnityDebuger
public class UnityDebuger : IDebuger
{
    public void Log(string str)
    {
        Debug.Log($"[UnityDebuger]:{str}");
    }
}

//定义NetDebuger
public class NetDebuger : IDebuger
{
    public void Log(string str)
    {
        Debug.Log($"[NetDebuger]:{str}");
    }
}

//定义一个静态Debug工具 将IDebuger标记为Inject
public static class DebugUtils
{
    [Inject]
    static IDebuger debuger;

    public static void Log(string str)
    {
        debuger.Log(str);
    } 
}

//定义一个Debuger将IDebuger标记为可注入
public class Debuger
{
    [Inject]
    IDebuger debuger;

    public void Log(string msg)
    {
        debuger.Log($"[Debuger]:{msg}");
    }
}

public class Test : MonoBehaviour
{
    private void Start()
    {
        //创建一个Context
        Context context = new Context();
        //将Debuger中的IDebuger绑定为NetDebuger
        context.Bind<IDebuger>().To<NetDebuger>().Where((type) => type == typeof(Debuger));
        //为Debuger注入为NetDebuger
        Injecter injecter = new Injecter(context);
        var debuger = injecter.CreateInstance<Debuger>();
        debuger.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        
        //将DebugUtils中的IDebuger绑定为UnityDebuger
        Context context2 = new Context();
        context2.Bind<IDebuger>().To<UnityDebuger>().Where((type) => type == typeof(DebugUtils));
        Injecter injecter2 = new Injecter(context2);
        //将DebugUtils中的IDebuger注入为UnityDebuger
        injecter2.Inject(typeof(DebugUtils));
        DebugUtils.Log("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        
        //为这个注入器切换上下文 因为context中没有绑定DebugUtils中IDebuger的注入信息 所以这儿会报空指针
        injecter2.SwitchContext(context);
        DebugUtils.Log("cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc");
    }
}
```