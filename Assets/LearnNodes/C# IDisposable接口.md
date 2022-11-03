###  C# IDisposable接口

参考资料:

这两篇说了为什么使用IDisposalbe接口和怎么实现

 https://blog.csdn.net/zrf2112/article/details/50644652

 https://www.jb51.net/article/54899.htm

这篇说了为什么要在手动Dispose的时候调用GC.SuppressFinalize(this)。（防止多次释放，结合IDisposable接口）

https://baijiahao.baidu.com/s?id=1665406284618223266&wfr=spider&for=pc

- #### IDisposable接口怎么使用

  ```c#
  public class DisposeClass : IDisposable
      {
          //是否回收 标记下资源已经被释放掉了防止手动调用Dispose方法之后GC又再次调用析构函数去释放对象。
          private bool _disposed;
  
          /// <summary>
          /// 当需要回收非托管资源的DisposableClass类，
          /// 就调用Dispoase()方法。而这个方法不会被CLR自动调用，需要手动调用。
          /// </summary>
          public void Dispose()
          {
              //手动调用将非托管代码和托管代码释放
              Dispose(true);
              //告诉GC不要在调用这个对象的析构函数了
              GC.SuppressFinalize(this);
          }
          /// <summary>
          /// 析构函数
          /// 当托管堆上的对象没有被其它对象引用，GC会在回收对象之前，调用对象的析构函数。
          /// 这里的~DisposableClass()析构函数的意义在于告诉GC你可以回收我，
          /// Dispose(false)表示在GC回收的时候，就不需要手动回收了。
          /// </summary>
          ~DisposeClass()
          {
              Dispose(false);
          }
  
          /// <summary>
          /// 通过此方法，所有的托管和非托管资源都能被回收。
          /// 参数disposing表示是否需要释放那些实现IDisposable接口的托管对象。
          /// 设置为虚方法是想让子类也参与到垃圾回收逻辑中，还不会影响到基类。
          /// </summary>
          /// <param name="disposing"></param>
          protected virtual void Dispose(bool disposing)
          {
              if (_disposed) return;//已经回收不在执行
              if(disposing)
              {
                  //TODO:  释放那些实现了IDispose接口的托管对象
                  //如果disposings设置为true，就表示DisposablClass类依赖某些实现了IDisposable接口的托管对象，
                  //可以通过这里的Dispose(bool disposing)方法调用这些托管对象的Dispose()方法进行回收。
  
              }
              //TODO: 释放非托管资源 ，设置对象为null
              //如果disposings设置为false,就表示DisposableClass类依赖某些没有实现IDisposable的非托管资源，
              //那就把这些非托管资源对象设置为null，等待GC调用DisposableClass类的析构函数，把这些非托管资源进行回收。
              //比如说我自定义的一些数据封装对象，设置他们的引用为null，或者将他们放回对象池中
  
              //在.NET 2.0之前，如果一个对象的析构函数抛出异常，这个异常会被CLR忽略。但.NET 2.0以后，如果析构函数抛出异常就会导致应用程序的崩溃。所以，保证析构函数不抛异常变得非常重要。
              //还有，Dispose()方法允许抛出异常吗？答案是否定的。如果Dispose()方法有抛出异常的可能，那就需要使用try / catch来手动捕获
              try
              {
                  
              }catch (Exception ex)
              {
                  //Log.Warn(ex) 记录日志
  
              }
  
              _disposed = true;
          }
      }
  
      public class SubDisposableClass : DisposeClass
      {
          private bool _disposed; //表示是否已经被回收
  
          protected override void Dispose(bool disposing)
          {
              if (!_disposed) //如果还没有被回收
              {
                  if (disposing) //如果需要回收一些托管资源
                  {
                      //TODO:回收托管资源，调用IDisposable的Dispose()方法就可以
                  }
                  //TODO：回收非托管资源，把之设置为null，等待CLR调用析构函数的时候回收
                  _disposed = true;
              }
              base.Dispose(disposing);//再调用父类的垃圾回收逻辑
          }
      }
  ```

  

- #### 为什么要用使用IDisposable接口

  - 处理非托管代码，在C#中，托管资源的垃圾回收是通过CLR的Garbage Collection来实现的，Garbage Collection会调用堆栈上对象的析构函数完成对象的释放工作；而对于一些非托管资源，比如数据库链接对象等，需要实现IDisposable接口进行手动的垃圾回收。
  - 举个例子，如果有两个类A B,如果A中引用了B,B中引用了A,那么按照垃回收机制，只有在对象的引用计数为0时GC才会去调用析构函数去释放对象，按照这个逻辑的话A B两个对象永远得不到释放，他们会一直占用这内存。

- #### 在什么时候下使用IDisposable接口

  - 那当前项目中的使用，比如特效，特效管理里存放当前场景释放和缓存的大量特效资源（特效封装类），我需要在离开场景的时候调用特效管理类的Dispose方法，在方法里依次调用每个特效类的Dispose方法去释放比如特效资源和定时器等对象。

  ##### **总结:当我们自定义的类以及业务逻辑中引用某些托管代码或者非托管代码资源，就需要实现IDisposable接口，实现对这些资源的对象的垃圾回收**

- #### 延伸知识点，什么是托管代码，什么是非托管代码

  - 托管资源就是托管给CLR的资源，CLR能对这些资源进行管理。而非托管资源则是CLR无法对这些资源管理，这些资源的申请、释放必须由使用者自行管理。也就是说，你能在.Net中找到的类产生的对象，都是托管资源。
  -  GC对于实现析构函数和没实现析构函数的类处理方法不一样，简单些说GC对于实现了析构函数的类一定会调用他们的析构函数

- ####   使用析构函数能够最大程度上的保证资源能够被释放，因为有的时候你忘记调用Dispose方法。

  Tip:**但是需要注意一个问题，析构函数会在GC时被调用，那么在此之前已经调用了Dispose已经释放了对象，这样就造成了多次调用dispose方法，所以需要告诉GC不要调用析构函数了，GC提供了接口（ GC.SuppressFinalize(this)）已满足这个需求。**

  

  

  

  

  