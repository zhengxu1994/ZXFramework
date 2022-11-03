# C#语言特性

#### 匿名方法

```c#
//匿名方法是通过使用 delegate 关键字创建委托实例来声明的
 delegate void LogNum(int num);
 LogNum n = delegate(int n)
        {
            Debug.Log(n);
        };
```

#### 拓展方法

```c#
/// <summary>
/// Log拓展
/// </summary>
public static class LogExtension
{ 
    /// <summary>
    /// 拓展在log前加时间
    /// </summary>
    /// <param name="str"></param>
    public static void LogByTime(this string str)
    {
        Debug.Log(string.Format("{0}:{1}", System.DateTime.Now, str));
    }
}
```



#### 系统内置委托

```c#
//C# 内置委托 Action / Func 协变指的是委托方法的返回值类型直接或间接继承自委托签名的返回值类型，逆变则是参数类型继承自委托方法的参数类型
//Action是无返回值委托
//Func 是带返回值的委托
  Action<int> actionA = (a) => { Debug.Log(a); };
  Action<int,int> actionB = (a,b) => { Debug.Log(a + b); };
//Func最后一个参数类型为返回值类型
  Func<int, bool> funcA = (a) => {
            return true;
        };

  Func<int, int,bool> funcB = (a,b) => {
            return a > b;
        };
 
```

#### Lambda

``` c#
delegate void LogNum(int num);
LogNum n = (num)=>Debug.Log(num);
```

#### Lambda 与 函数直接调用的开销对比

```c#
   HashSet<int> has = new HashSet<int>();
        has.Add(1);
        UnityEngine.Profiling.Profiler.BeginSample("start");
        for (int i = 0; i < 1000; i++)
        {
            has.RemoveWhere(Check);//Profile差不多开销是 GCAlloc 109.4kb 耗时 0.15ms
        }
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("start1");
        for (int i = 0; i < 1000; i++)
        {
            has.RemoveWhere((a) => a < 1) ; // GCAlloc 112b 耗时0.01ms
        }
        UnityEngine.Profiling.Profiler.EndSample();


    bool Check(int a)
    {
        return a < 1;
    }
```



#### Dynamic

```c#
//params 动态参数 可以传递多个参数，但是参数必须为一维数组类型
public int Add(params int[] nums)
    {
        int a = 0;
        for (int i = 0; i < nums.Length; i++)
        {
            a += nums[i];
        }
        return a;

    }
Add(1, 2, 3, 4);
```

#### Async/Await

**c#语言的作者在编译器中内置了适当的扩展点，允许在异步方法中“等待”不同的类型。  为了使类型成为“awaitable”(即在await表达式上下文中有效)，类型应该遵循一种特殊的模式:  编译器应该能够找到一个名为GetAwaiter的实例或扩展方法。该方法的返回类型应符合一定的要求:  该类型应该实现INotifyCompletion接口。 类型应该具有bool IsCompleted {get;}属性和T  GetResult()方法**

```c#
//C# 5.0 新出的异步方法，异步方法有返回值
https://www.lfzxb.top/dissecting-the-async-methods-in-c/
//实现INotifyCompletion接口
public struct LazyAwaiter<T> : INotifyCompletion
{
    private readonly Lazy<T> _lazy;

    public LazyAwaiter(Lazy<T> lazy) => _lazy = lazy;

    public T GetResult() => _lazy.Value;

    public bool IsCompleted => true;

    public void OnCompleted(Action continuation) { }
}
//拓展Lazy类拥有GetAwaiter方法
public static class LazyAwaiterExtensions
{
    public static LazyAwaiter<T> GetAwaiter<T>(this Lazy<T> lazy)
    {
        return new LazyAwaiter<T>(lazy);
    }
}
async Task Foo()
{
    var lazy = new Lazy<int>(() => 42);
    //这里await就可以接受lazy 因为lazy实现了GetAwaiter方法
    var result = await lazy;
    Debug.Log(result);
}
```



**Unity 异步API支持Async/Await,首先它还是一个单线程的异步，使用的还是unity的StartCoroutine开启一个协程，只不过封装了api使在协程完成时可以调用INotifyCompletion的onCompleted接口**

```c#
  //C# 异步的一个条件  
public static SimpleCoroutineAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
    {
       //返回的是一个实现了INotifyCompletion接口的类，类里存的泛型为AssetBundle类型
        var awaiter = new SimpleCoroutineAwaiter<AssetBundle>();
       //调用当前线程开启一个任务  任务里面是开启一个协程
        RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
            InstructionWrappers.AssetBundleCreateRequest(awaiter, instruction)));
        return awaiter;
    }
 
 static void RunOnUnityScheduler(Action action)
    {
        if (SynchronizationContext.Current == SyncContextUtil.UnitySynchronizationContext)
        {
            action();
        }
        else
        {
            //post
            SyncContextUtil.UnitySynchronizationContext.Post(_ => action(), null);
        }
    }

public static class SyncContextUtil
    {
        //在场景加载前调用这个方法
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            //初始化上下文
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static int UnityThreadId
        {
            get; private set;
        }

        public static SynchronizationContext UnitySynchronizationContext
        {
            get; private set;
        }
    }
   
   //实现 INotifyCompletion接口 满足C#异步的条件之一
   public class SimpleCoroutineAwaiter<T> : INotifyCompletion
    {
        bool _isDone;
        Exception _exception;
        Action _continuation;
        T _result;

        public bool IsCompleted
        {
            get { return _isDone; }
        }

        public T GetResult()
        {
            Assert(_isDone);

            if (_exception != null)
            {
                ExceptionDispatchInfo.Capture(_exception).Throw();
            }

            return _result;
        }
        //完成时将获取对应类型的对象
        public void Complete(T result, Exception e)
        {
            Assert(!_isDone);

            _isDone = true;
            _exception = e;
            _result = result;

            // Always trigger the continuation on the unity thread when awaiting on unity yield
            // instructions
            if (_continuation != null)
            {
                RunOnUnityScheduler(_continuation);
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            Assert(_continuation == null);
            Assert(!_isDone);

            _continuation = continuation;
        }
    }
 //协程方法，还是使用的迭代器，当yield return instruction结束后调用complete方法传递获取的对象
  public static IEnumerator AssetBundleCreateRequest(
            SimpleCoroutineAwaiter<AssetBundle> awaiter, AssetBundleCreateRequest instruction)
        {
            yield return instruction;
            awaiter.Complete(instruction.assetBundle, null);
        }
```



