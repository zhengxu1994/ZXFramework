## Unity 基础优化

### 1.减少循环调用

```c#
void Update()
{
  if(true) //只需要1 / 100次
  {
     for(int i = 0; i < 100; i ++)
     {
       
     }
  }
  for(int i = 0; i < 100; i ++) // 100次
  {
     if(true) //100次
     {
       
     }
  }
}
```

### 2.仅在改变时更新显示

```c#
private int score;

public void IncrementScore(int incrementBy)
{
   score+=incrementBy;
}

void Update(){
   DisplayScore(score);//错误
}

//正确
public void IncrementScore(int incrementBy)
{
   score+=incrementBy;
   DisplayScore(score);
}


```

###  3.增加代码更新延时

```c#
void Update()
{
   Test();  
}

private int interval = 3;//每3帧执行一次复杂运算
void Update()
{
  if(Time.frameCount % interval == 0)
  {
     Test();
  }
}

```

### 4.在初始化时缓存组件

```c#
void Update()
{
  Renderer myRender = GetComponent<Renderer>(); //错误
  Test(myRender);
}
Renderer myRender;
//正确
void Start()
{
   myRender = GetComponent<Renderer>();
}
```

### 5.避免昂贵的Unity API 

Find() 递归遍历所有的场景内对象 可以使用缓存

SendMessage()  会使用反射 ,可以使用事件 和委托实现效果

Tranform.position / rotation 等属性不要频繁修改，因为当这个属性被修改后它的子对象也会去修改，应该使用一个临时变量去存储，修改临时变量，最后只设置一次属性。

### 6.继承了monobehaviour的脚本，如果不需要用到的事件函数把它删掉

unity每次帧调用前会做安全检测：

检查GameObject的有效性

收集有效的GameObject调用它身上的update等函数，所以空的函数需要删掉

### 7.向量运算

开平方的消耗很大，所以尽量不要使用Vector3.magnitude() 和 Distance()方法

建议使用vector3.SqrMagnitude()

### 8.Camera.main

Camera.main = GameObject.FindWithTag("Main Camera");//这每次还是会去搜索 所以尽量一开始缓存下来

