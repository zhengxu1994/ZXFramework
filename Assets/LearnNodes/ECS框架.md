# ECS框架

### 对ECS框架的理解:

### 解决了项目存在的最大问题，解耦，我理解的有哪些解耦：

##### 1.数据与逻辑分离，纯数据的部分存在component中，逻辑根本不关心数据存放在哪里，只需要拿过来用就可以，如果没有这份数据那我就不关心它。

##### 2.逻辑分离，单个system只关心它关心的部分（拥有N个component的对象），system只需要运行的自己逻辑就可以，不需要关心其他对象的逻辑。这样就保证了只需要当前这个system的逻辑没有问题就可以，不用关心其他的逻辑造成我当前的逻辑出错，或者我当前的逻辑造成其他的逻辑错误。我需要哪个逻辑时只要在Entity上挂在对应的component即可，不需要时移除component，不想传统的设计方式，根本没有办法把某块逻辑移除（除非给删了哈哈）。

##### 3.表现与逻辑分离，加了表现的组件才会做表现的事情，不加就是纯逻辑，这样客户端的逻辑就能直接拿到服务器上。



### ECS介绍：

- E: Entity  一个不代表任何意义的实体（可以理解为Unity里的一个空的GameObject）
- C: Component 一个只包含数据的组件（可以理解为Unity的一个自定义组件，里面只有数据，没有任何方法）
- S: System 一个用来处理数据的系统（可以理解为Unity的一个自定义组件，里面只有方法，没有任何数据）



- 面向过程： 1、摇（所有狗的尾巴） 2、摇（所有猪的尾巴）
- 面向对象： 1、所有狗.（摇尾巴） 2、所有猪.（摇尾巴）
- 面向数据： 1、收集所有的尾巴    2、摇（尾巴）



#### 除了解耦的其他优点：

##### 1.面向数据的方式，让内存排列天然的紧密，非常适合现代cpu的缓存机制，极大增加CPU的缓存命中率，大幅提升性能。





#### C# ECS源码分析

- ECS中的S,也就是system系统，通过system去处理组件和实体的逻辑，ecs中system分为5种system，所有的system最终都继承自ISystem接口。

  ```c#
      /// This is the base interface for all systems.
      /// It's not meant to be implemented.
      /// Use IInitializeSystem, IExecuteSystem,
      /// ICleanupSystem or ITearDownSystem.
      public interface ISystem {
      }
  ```

  

  - IExecuteSytem，每帧都会去执行。

    ```c#
        /// Implement this interface if you want to create a system which should be
        /// executed every frame.
        public interface IExecuteSystem : ISystem {
    
            void Execute();
        }
    ```

    JobSystem继承自IExecuteSystem，主要用于处理多线程任务系统。https://www.cnblogs.com/sifenkesi/p/12258842.html，讲述了JobSystem是如何使用多线程工作的，有什么优势。

    ```c#
        //JobSystem调用带有实体子集的Execute(实体)，并将工作负载分配到指定数量的线程上。在Entitas中编写多线程代码时，不要使用生成的方法，如AddXyz()和ReplaceXyz()。
        public abstract class JobSystem<TEntity> : IExecuteSystem where TEntity : class, IEntity {
    
            readonly IGroup<TEntity> _group;
            readonly int _threads;
            //job system管理job依赖关系，并保证执行时序的正确性
            readonly Job<TEntity>[] _jobs;
    
            int _threadsRunning;
    
            protected JobSystem(IGroup<TEntity> group, int threads) {
                _group = group;
                _threads = threads;
                _jobs = new Job<TEntity>[threads];
                for (int i = 0; i < _jobs.Length; i++) {
                    _jobs[i] = new Job<TEntity>();
                }
            }
    
            protected JobSystem(IGroup<TEntity> group) : this(group, Environment.ProcessorCount) {
            }
    
            public virtual void Execute() {
                _threadsRunning = _threads;
                var entities = _group.GetEntities();
                var remainder = entities.Length % _threads;
                //计算出每个线程处理多少个对象
                var slice = entities.Length / _threads + (remainder == 0 ? 0 : 1);
                for (int t = 0; t < _threads; t++) {
                    var from = t * slice;
                    var to = from + slice;
                    if (to > entities.Length) {
                        to = entities.Length;
                    }
    
                    var job = _jobs[t];
                    job.Set(entities, from, to);
                    if (from != to) {
                        //线程池
                        //将jobs放在一个job queue里面，worker threads从job queue里面获取job然后执行
                        ThreadPool.QueueUserWorkItem(queueOnThread, _jobs[t]);
                    } else {
                        Interlocked.Decrement(ref _threadsRunning);
                    }
                }
    
                while (_threadsRunning != 0) {
                }
    
                foreach (var job in _jobs) {
                    if (job.exception != null) {
                        throw job.exception;
                    }
                }
            }
            
            //线程callback 调用Execute处理对象
            void queueOnThread(object state) {
                var job = (Job<TEntity>)state;
                try {
                    for (int i = job.from; i < job.to; i++) {
                        Execute(job.entities[i]);
                    }
                } catch (Exception ex) {
                    job.exception = ex;
                } finally {
                    Interlocked.Decrement(ref _threadsRunning);
                }
            }
    
            protected abstract void Execute(TEntity entity);
        }
        //完成特定任务的一个小的工作单元。job接收参数并操作数据，类似于函数调用。job之间可以有依赖关系，也就是一个job可以等另一个job完成之后再执行。
        class Job<TEntity> where TEntity : class, IEntity {
    
            public TEntity[] entities;
            public int from;
            public int to;
            public Exception exception;
    
            public void Set(TEntity[] entities, int from, int to) {
                this.entities = entities;
                this.from = from;
                this.to = to;
                exception = null;
            }
        }
    ```

    

  - IInitializeSystem,只会在ecs环境初始化时调用。

    ```c#
      /// Implement this interface if you want to create a system which should be
        /// initialized once in the beginning.
        public interface IInitializeSystem : ISystem {
    
            void Initialize();
        }

  - IReactiveSystem,只某些条件触发时才会被调用,它继承自IExecuteSystem,但是复写的Execute()方法只有在收集器触发时才会调用。

    ```c#
       public interface IReactiveSystem : IExecuteSystem {
            //激活收集器
            void Activate();
            //禁用收集器
            void Deactivate();
            //清空收集器
            void Clear();
        }
    ```

    ReactiveSystem<TEntity>继承IReactiveSystem,继承ReactiveSystem的类需要复写ICollector，Filter，Execute方法。

    - ICollector，关注某类组件，当实体身上的某类组件发生改变时会收集这些实体对象
    - Filter，对收集的实体对象列表再次过滤，条件自定义。
    - Execute(List<TEntity>entities)，传入的参数就是每帧通过上面两个方法收集到的实体对象列表
    - ReactiveSystem构造函数中会初始化收集信息（context.CreateCollector()）context的拓展和一个用于存放实体对象的列表（ _buffer = new List<TEntity>()）
    - Execute(),从收集器中获取目标并通过Filter方法过滤后获得最终list列表传入到有参Execute方法，并清空收集器，在list使用完毕后release entity并清空列表list

    ```c#
     public abstract class ReactiveSystem<TEntity> : IReactiveSystem where TEntity : class, IEntity {
    
            readonly ICollector<TEntity> _collector;
            readonly List<TEntity> _buffer;
            string _toStringCache;
    
            protected ReactiveSystem(IContext<TEntity> context) {
                _collector = GetTrigger(context);
                _buffer = new List<TEntity>();
            }
    
            protected ReactiveSystem(ICollector<TEntity> collector) {
                _collector = collector;
                _buffer = new List<TEntity>();
            }
    
            /// Specify the collector that will trigger the ReactiveSystem.
            protected abstract ICollector<TEntity> GetTrigger(IContext<TEntity> context);
    
            /// This will exclude all entities which don't pass the filter.
            protected abstract bool Filter(TEntity entity);
    
            protected abstract void Execute(List<TEntity> entities);
    
            /// Activates the ReactiveSystem and starts observing changes
            /// based on the specified Collector.
            /// ReactiveSystem are activated by default.
            public void Activate() {
                _collector.Activate();
            }
    
            /// Deactivates the ReactiveSystem.
            /// No changes will be tracked while deactivated.
            /// This will also clear the ReactiveSystem.
            /// ReactiveSystem are activated by default.
            public void Deactivate() {
                _collector.Deactivate();
            }
    
            /// Clears all accumulated changes.
            public void Clear() {
                _collector.ClearCollectedEntities();
            }
    
            /// Will call Execute(entities) with changed entities
            /// if there are any. Otherwise it will not call Execute(entities).
            public void Execute() {
                if (_collector.count != 0) {
                    foreach (var e in _collector.collectedEntities) {
                        if (Filter(e)) {
                            e.Retain(this);
                            _buffer.Add(e);
                        }
                    }
    
                    _collector.ClearCollectedEntities();
    
                    if (_buffer.Count != 0) {
                        try {
                            Execute(_buffer);
                        } finally {
                            for (int i = 0; i < _buffer.Count; i++) {
                                _buffer[i].Release(this);
                            }
                            _buffer.Clear();
                        }
                    }
                }
            }
    
            public override string ToString() {
                if (_toStringCache == null) {
                    _toStringCache = "ReactiveSystem(" + GetType().Name + ")";
                }
    
                return _toStringCache;
            }
    
            ~ReactiveSystem() {
                Deactivate();
            }
        }
    ```

    MultiReactiveSystem继承自IReactiveSystem,多收集器触发系统,与ReactuveSystem差不多，只不过对象一个是通过单个collector收集一个是通过多个collector收集，其他的机制都一样。

    ```c#
     public abstract class MultiReactiveSystem<TEntity, TContexts> : IReactiveSystem
            where TEntity : class, IEntity
            where TContexts : class, IContexts {
    
            readonly ICollector[] _collectors;
            readonly HashSet<TEntity> _collectedEntities;
            readonly List<TEntity> _buffer;
            string _toStringCache;
    
            protected MultiReactiveSystem(TContexts contexts) {
                _collectors = GetTrigger(contexts);
                _collectedEntities = new HashSet<TEntity>();
                _buffer = new List<TEntity>();
            }
    
            protected MultiReactiveSystem(ICollector[] collectors) {
                _collectors = collectors;
                _collectedEntities = new HashSet<TEntity>();
                _buffer = new List<TEntity>();
            }
    
            /// Specify the collector that will trigger the ReactiveSystem.
            protected abstract ICollector[] GetTrigger(TContexts contexts);
    
            /// This will exclude all entities which don't pass the filter.
            protected abstract bool Filter(TEntity entity);
    
            protected abstract void Execute(List<TEntity> entities);
    
            /// Activates the ReactiveSystem and starts observing changes
            /// based on the specified Collector.
            /// ReactiveSystem are activated by default.
            public void Activate() {
                for (int i = 0; i < _collectors.Length; i++) {
                    _collectors[i].Activate();
                }
            }
    
            /// Deactivates the ReactiveSystem.
            /// No changes will be tracked while deactivated.
            /// This will also clear the ReactiveSystem.
            /// ReactiveSystem are activated by default.
            public void Deactivate() {
                for (int i = 0; i < _collectors.Length; i++) {
                    _collectors[i].Deactivate();
                }
            }
    
            /// Clears all accumulated changes.
            public void Clear() {
                for (int i = 0; i < _collectors.Length; i++) {
                    _collectors[i].ClearCollectedEntities();
                }
            }
    
            /// Will call Execute(entities) with changed entities
            /// if there are any. Otherwise it will not call Execute(entities).
            public void Execute() {
                for (int i = 0; i < _collectors.Length; i++) {
                    var collector = _collectors[i];
                    if (collector.count != 0) {
                        _collectedEntities.UnionWith(collector.GetCollectedEntities<TEntity>());
                        collector.ClearCollectedEntities();
                    }
                }
    
                foreach (var e in _collectedEntities) {
                    if (Filter(e)) {
                        e.Retain(this);
                        _buffer.Add(e);
                    }
                }
    
                if (_buffer.Count != 0) {
                    try {
                        Execute(_buffer);
                    } finally {
                        for (int i = 0; i < _buffer.Count; i++) {
                            _buffer[i].Release(this);
                        }
                        _collectedEntities.Clear();
                        _buffer.Clear();
                    }
                }
            }
    
            public override string ToString() {
                if (_toStringCache == null) {
                    _toStringCache = "MultiReactiveSystem(" + GetType().Name + ")";
                }
    
                return _toStringCache;
            }
    
            ~MultiReactiveSystem() {
                Deactivate();
            }
        }
    ```

    

  - IClearUpSystem,在每帧update后调用。

    ```c#
        /// Implement this interface if you want to create a system which should
        /// execute cleanup logic after execution.
        public interface ICleanupSystem : ISystem {
    
            void Cleanup();
        }
    ```

    

  - ITearDownSystem ,游戏结束销毁时调用。

    ```c#
        /// Implement this interface if you want to create a system which should
        /// tear down once in the end.
        public interface ITearDownSystem : ISystem {
    
            void TearDown();
        }
    ```

  - System，管理各个类型的system，将子system加入到system中，system会调用Initialize，Execute，Cleanup,TearDown,ActivateReactiveSystems,DeactivateReactiveSystems,ClearReactiveSystems方法去遍历调用各个system的逻辑接口。

    ```c#
     public class Systems : IInitializeSystem, IExecuteSystem, ICleanupSystem, ITearDownSystem {
    
            protected readonly List<IInitializeSystem> _initializeSystems;
            protected readonly List<IExecuteSystem> _executeSystems;
            protected readonly List<ICleanupSystem> _cleanupSystems;
            protected readonly List<ITearDownSystem> _tearDownSystems;
    
            /// Creates a new Systems instance.
            public Systems() {
                _initializeSystems = new List<IInitializeSystem>();
                _executeSystems = new List<IExecuteSystem>();
                _cleanupSystems = new List<ICleanupSystem>();
                _tearDownSystems = new List<ITearDownSystem>();
            }
    
            /// Adds the system instance to the systems list.
            public virtual Systems Add(ISystem system) {
                var initializeSystem = system as IInitializeSystem;
                if (initializeSystem != null) {
                    _initializeSystems.Add(initializeSystem);
                }
    
                var executeSystem = system as IExecuteSystem;
                if (executeSystem != null) {
                    _executeSystems.Add(executeSystem);
                }
    
                var cleanupSystem = system as ICleanupSystem;
                if (cleanupSystem != null) {
                    _cleanupSystems.Add(cleanupSystem);
                }
    
                var tearDownSystem = system as ITearDownSystem;
                if (tearDownSystem != null) {
                    _tearDownSystems.Add(tearDownSystem);
                }
    
                return this;
            }
    
            /// Calls Initialize() on all IInitializeSystem and other
            /// nested Systems instances in the order you added them.
            public virtual void Initialize() {
                for (int i = 0; i < _initializeSystems.Count; i++) {
                    _initializeSystems[i].Initialize();
                }
            }
    
            /// Calls Execute() on all IExecuteSystem and other
            /// nested Systems instances in the order you added them.
            public virtual void Execute() {
                for (int i = 0; i < _executeSystems.Count; i++) {
                    _executeSystems[i].Execute();
                }
            }
    
            /// Calls Cleanup() on all ICleanupSystem and other
            /// nested Systems instances in the order you added them.
            public virtual void Cleanup() {
                for (int i = 0; i < _cleanupSystems.Count; i++) {
                    _cleanupSystems[i].Cleanup();
                }
            }
    
            /// Calls TearDown() on all ITearDownSystem  and other
            /// nested Systems instances in the order you added them.
            public virtual void TearDown() {
                for (int i = 0; i < _tearDownSystems.Count; i++) {
                    _tearDownSystems[i].TearDown();
                }
            }
    
            /// Activates all ReactiveSystems in the systems list.
            public void ActivateReactiveSystems() {
                for (int i = 0; i < _executeSystems.Count; i++) {
                    var system = _executeSystems[i];
                    var reactiveSystem = system as IReactiveSystem;
                    if (reactiveSystem != null) {
                        reactiveSystem.Activate();
                    }
    
                    var nestedSystems = system as Systems;
                    if (nestedSystems != null) {
                        nestedSystems.ActivateReactiveSystems();
                    }
                }
            }
    
            /// Deactivates all ReactiveSystems in the systems list.
            /// This will also clear all ReactiveSystems.
            /// This is useful when you want to soft-restart your application and
            /// want to reuse your existing system instances.
            public void DeactivateReactiveSystems() {
                for (int i = 0; i < _executeSystems.Count; i++) {
                    var system = _executeSystems[i];
                    var reactiveSystem = system as IReactiveSystem;
                    if (reactiveSystem != null) {
                        reactiveSystem.Deactivate();
                    }
    
                    var nestedSystems = system as Systems;
                    if (nestedSystems != null) {
                        nestedSystems.DeactivateReactiveSystems();
                    }
                }
            }
    
            /// Clears all ReactiveSystems in the systems list.
            public void ClearReactiveSystems() {
                for (int i = 0; i < _executeSystems.Count; i++) {
                    var system = _executeSystems[i];
                    var reactiveSystem = system as IReactiveSystem;
                    if (reactiveSystem != null) {
                        reactiveSystem.Clear();
                    }
    
                    var nestedSystems = system as Systems;
                    if (nestedSystems != null) {
                        nestedSystems.ClearReactiveSystems();
                    }
                }
            }
        }
    ```

- ECS中的C，Component组件，组件中存放的是数据，实体通过添加组件来获取数据，系统通过操作组件来修改数据，实体本身不对数据进行操作。

  所有组件都继承自IComponent接口，自动生成工具会找到所有继承自这个接口的类，生成对应的组件类。

  ```c#
     public interface IComponent {
      }
     [Game]
     public class IUnitIdComponent : IComponent
     { 
       [PrimaryEntityIndex]
        public int uId;
     }
  ```

  

  组件可以通过属性方法去设置组件的属性，所有的属性都最终继承自c#的Attribute类,说一下常用的属性

  - Unique，单例，下文中只有一个实体与该组件存在。

  - [Game]or[Data]or[其他名称]这个代表该组件只在对应的context环境下才能添加

  - FlagPrefix，给组件添加标记比如 FlagPrefix("Has"),在使用组件的时候就可以entity.hasXXX

  - PrimaryEntityIndex,数据唯一，标记组件中的某个数据在context中只能不能有相同值。

  - DontGenerate 不要自动生成对应的组件类。

    自动生成工具使用:Tools/Preference,显示import all 自定设置所有导出数据，然后点击generate生成（需要打开vs工程，因为工具最终找的是Assembly-CSharp.csproj文件），图中的Game和Input就是上下文环境。

    ![image-20210916093015995](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/ECS框架.assets/image-20210916093015995.png)

#### Test，生成组件并使用system对组件操作

```c#
using Entitas;
using Entitas.CodeGeneration.Attributes;


public class UidComponent : IComponent
{
    //每个单位拥有的uid都不能一样
    [PrimaryEntityIndex]
    public int uid;
}
```

生成的文件:为组件生成了添加组件，修改组件数据，移除组件，和组件匹配方法(配合Group一起使用)。

```c#
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public UidComponent uid { get { return (UidComponent)GetComponent(GameComponentsLookup.Uid); } }
    public bool hasUid { get { return HasComponent(GameComponentsLookup.Uid); } }

    public void AddUid(int newUid) {
        var index = GameComponentsLookup.Uid;
        var component = (UidComponent)CreateComponent(index, typeof(UidComponent));
        component.uid = newUid;
        AddComponent(index, component);
    }

    public void ReplaceUid(int newUid) {
        var index = GameComponentsLookup.Uid;
        var component = (UidComponent)CreateComponent(index, typeof(UidComponent));
        component.uid = newUid;
        ReplaceComponent(index, component);
    }

    public void RemoveUid() {
        RemoveComponent(GameComponentsLookup.Uid);
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class GameMatcher {

    static Entitas.IMatcher<GameEntity> _matcherUid;

    public static Entitas.IMatcher<GameEntity> Uid {
        get {
            if (_matcherUid == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.Uid);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherUid = matcher;
            }

            return _matcherUid;
        }
    }
}

```

![image-20210916093410036](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/ECS框架.assets/image-20210916093410036.png)同时还会生成自定义的上下文和属性

组件在系统中如何使用：

```c#
using System;
using Entitas;
public class GameFeature : Feature
{
    public GameFeature(Contexts contexts)
    {
        //Add方法会根据添加的system类型将system添加到对应类型的列表中
        //比如 initsystem是init类型，会将initsystem添加到initlist中，
        //然后在GameFeature调用Init方法时调用列表中的系统init方法
        Add(new InitSystem(contexts));
        Add(new LogNameSystem(contexts));
        Add(new NameChangeSystem(contexts));
        Add(new MyCleanUpSystem(contexts));
        Add(new GameDisposeSystem(contexts));
    }
}


using Entitas;
public class InitSystem : IInitializeSystem
{
    private GameContext game;

    public InitSystem(Contexts contexts)
    {
        //缓存一下 game 和 input 会在Contexts构造函数中创建
        game = Contexts.sharedInstance.game;
    }
    public void Initialize()
    {
        //初始化 例 创建10个entity
        for (int i = 0; i < 10; i++)
        {
            var entity = game.CreateEntity();
            entity.AddUid(i);//这里的uid不能一样，不然会报错，因为我们定义的属性确保uid不一致。
            entity.AddName(i.ToString());
        }
    }
}


using Entitas;
using UnityEngine;

public class LogNameSystem : IExecuteSystem
{
    private GameContext game;
    private IGroup<GameEntity> hasNameEnities;
    public LogNameSystem(Contexts contexts)
    {
        game = contexts.game;
        //根据组件获取拥有该组件的所有entity
        hasNameEnities = game.GetGroup(GameMatcher.Name);
        //GameMatcher.AllOf
        //GameMatcher.AnyOf
    }
    public void Execute()
    {
        var entities = hasNameEnities.GetEntities();
        for (int i = 0; i < entities.Length; i++)
        {
            Debug.Log(entities[i].name);
        }
    }
}

using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class NameChangeSystem : ReactiveSystem<GameEntity>
{
    public NameChangeSystem(Contexts context) : base(context.game)
    {

    }

    protected override void Execute(List<GameEntity> entities)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Debug.Log(entities[i].name.name);
        }
    }

    protected override bool Filter(GameEntity entity)
    {
        //过滤没有uid的对象
        return entity.hasUid;
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        //拥有名称的entity
        return ((GameContext)context).CreateCollector(GameMatcher.Name);//默认为replace时触发
        //GameMatcher.Name.Added() 添加组件时触发
        //GameMatcher.Name.Removed() 移除组件时触发
    }
}

public class MyCleanUpSystem : ICleanupSystem
{
    public MyCleanUpSystem(Contexts contexts)
    {

    }
    //每帧结束前做一些数据清空
    public void Cleanup()
    {
        
    }
}

using Entitas;
public class GameDisposeSystem : ITearDownSystem
{
    public GameDisposeSystem(Contexts contexts)
    {

    }
    //例如：一场战斗结束，清空所有数据。
    public void TearDown()
    {
        
    }
}
```

- ECS中的E,Entity实体，实体本身没有任何功能，需要什么功能就为实体添加某个组件，实体绑定了各种组件信息变换的监听事件，比如Add,Remove,Replace

  - public event EntityComponentChanged OnComponentAdded;当调用了Entity.AddXXX()组件后会触发这个事件。

  - public event EntityComponentChanged OnComponentRemoved;当调用了Entity.RemoveXXX()组件后会触发这个事件。

  - public event EntityComponentReplaced OnComponentReplaced;当调用了Entity.ReplaceXXX()组件后会触发这个事件。

    如果直接修改组件的数据是不会触发这些事件的，同时也不会更新Group绑定的组件列表信息。

    ##### 但是调用Replace方法肯定比直接修改数据内容要耗时，当遇到某个组件的数据在一帧内会被频繁修改那么有什么办法优化呢？

    ##### 可以在逻辑内直接修改组件数据并标记被修改了，在cleanup系统时统一调用replace方法去修改。

  - public event EntityEvent OnEntityReleased; 当实体被回收时触发。（比如战斗中士兵entity死亡）

  - public event EntityEvent OnDestroyEntity; 当实体被销毁时触发。（比如一场战斗结束，战斗中用到的entity需要全部销毁掉）

   所有的Entity最终都继承自IAERC接口

```c#
 public interface IAERC {

        int retainCount { get; }
        void Retain(object owner);
        void Release(object owner);
    }
   //组件修改委托（Add/Remove）
   public delegate void EntityComponentChanged(
        IEntity entity, int index, IComponent component
    );
   //组件移除委托(Replace)
    public delegate void EntityComponentReplaced(
        IEntity entity, int index, IComponent previousComponent, IComponent newComponent
    );
    //实体修改委托（Release/Destroy）
    public delegate void EntityEvent(IEntity entity);

```

```c#
public interface IEntity : IAERC {
        //上面说的几个事件
        event EntityComponentChanged OnComponentAdded;
        event EntityComponentChanged OnComponentRemoved;
        event EntityComponentReplaced OnComponentReplaced;
        event EntityEvent OnEntityReleased;
        event EntityEvent OnDestroyEntity;

        int totalComponents { get; }
        int creationIndex { get; }
        bool isEnabled { get; }

        Stack<IComponent>[] componentPools { get; }
        //上下文信息，当前实体是在哪个上下文中的
        ContextInfo contextInfo { get; }
        IAERC aerc { get; }
    
        void Initialize(int creationIndex,
            int totalComponents,
            Stack<IComponent>[] componentPools,
            ContextInfo contextInfo = null,
            IAERC aerc = null);
      
        void Reactivate(int creationIndex);
        //组件添加方法
        void AddComponent(int index, IComponent component);
        void RemoveComponent(int index);
        void ReplaceComponent(int index, IComponent component);
        //实体上绑定的组件信息
        IComponent GetComponent(int index);
        IComponent[] GetComponents();
        int[] GetComponentIndices();
        
        bool HasComponent(int index);
        bool HasComponents(int[] indices);
        bool HasAnyComponent(int[] indices);

        void RemoveAllComponents();

        Stack<IComponent> GetComponentPool(int index);
        IComponent CreateComponent(int index, Type type);
        T CreateComponent<T>(int index) where T : new();

        void Destroy();
        void InternalDestroy();
        void RemoveAllOnEntityReleasedHandlers();
    }
```

当调用Entity调用Add 和 Remove方法就是判断组件列表中是否存在该组件，如果存在则添加或移除并触发对应的事件.

Replace特殊一点

1.它会先判断是否存在该组件如果没有则添加并触发Add事件。

2.存在的情况下判断组件是否不一致，一致的情况下触发replace

3.不一致，则判断传入的组件是否为空，为空触发remove，不为空触发replace。

###### 从上面的逻辑可以发现，即使调用Entity.ReplaceXXX()方法也不一定会触发replace事件，这时候关注该组件replace的ReactiveSystem就不会收集到这个Entity,逻辑也许就不是想要的效果。

```c#
   public void ReplaceComponent(int index, IComponent component) {
            if (!_isEnabled) {
                throw new EntityIsNotEnabledException(
                    "Cannot replace component '" +
                    _contextInfo.componentNames[index] + "' on " + this + "!"
                );
            }

            if (HasComponent(index)) {
                replaceComponent(index, component);
            } else if (component != null) {
                AddComponent(index, component);
            }
        }  


void replaceComponent(int index, IComponent replacement) {
            // TODO VD PERFORMANCE
            // _toStringCache = null;

            var previousComponent = _components[index];
            if (replacement != previousComponent) {
                _components[index] = replacement;
                _componentsCache = null;
                if (replacement != null) {
                    if (OnComponentReplaced != null) {
                        OnComponentReplaced(
                            this, index, previousComponent, replacement
                        );
                    }
                } else {
                    _componentIndicesCache = null;

                    // TODO VD PERFORMANCE
                    _toStringCache = null;

                    if (OnComponentRemoved != null) {
                        OnComponentRemoved(this, index, previousComponent);
                    }
                }

                GetComponentPool(index).Push(previousComponent);

            } else {
                if (OnComponentReplaced != null) {
                    OnComponentReplaced(
                        this, index, previousComponent, replacement
                    );
                }
            }
        }
```

