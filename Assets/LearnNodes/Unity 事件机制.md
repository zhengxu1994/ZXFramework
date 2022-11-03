### Unity 事件机制

#### 为什么需要事件机制，用来解决了什么问题？

#### 当目标状态发生改变时，所有的观察者都可以收到通知。

#### 可以使用到的设计模式：观察者模式

观察者模式：当对象间存在一对多关系时可以使用观察者模式。

观察者模式的好处：

- 解耦
- 消息传递与触发机制

观察者模式的缺点：

- 如果一个对象被多个观察者之间或间接观察时，通知观察者将非常的耗时
- 仅仅只有事件触发那么观察者就不知道目标发生了哪些具体变化（可以使用传递参数解决）



知识点：

- Delegate.Combine(Delegate a ,Delegate b)将两个委托合并成一个委托
- Action.GetInvocationList() 获取一个action里所有的委托集合

**使用时发现如果在消息触发事件中如果有移除其他事件的操作可能会导致后面触发的消息获取不到而报错，所有可以加一个删除队列如果在事件分发的过程中有删除操作 可以加入到删除队列中等待分发执行完毕后在删除。**

- notifacations 用于注册
- notifyTargets 用于检测重复奖励
- objRegistEventList 用于删除监听
- notifactingList 正在触发中的事件
- needRemoveList 需要移除身上所有事件的对象列表



```c#
namespace Notifaction
{
    public static class Unist
    {
        public const string PreLoadAssets = "PreLoadAssets";
    }

    /// <summary>
    /// 消息参数
    /// </summary>
    public class NotifyParam : IDisposable
    {
        private Dictionary<string, object> Objs;

        private NotifyParam() { }

        public NotifyParam Obj(string path,object obj)
        {
            if (Objs == null) Objs = new Dictionary<string, object>();
            Objs[path] = obj;
            return this;
        }
        public static NotifyParam Create()
        {
            return new NotifyParam();//后面需要使用对象池
        }
        public void Dispose()
        {
            Objs?.Clear();
        }
    }
    /// <summary>
    /// 消息中心
    /// </summary>
    public class NotifactionCenter : Singleton<NotifactionCenter>
    {
        /// <summary>
        /// 用于注册消息
        /// </summary>
        private Dictionary<string, Action<NotifyParam>> notifacations = new Dictionary<string, Action<NotifyParam>>();
        /// <summary>
        /// 用于检验消息是否重复注册
        /// </summary>
        private Hashtable notifyTargets = new Hashtable();

        /// <summary>
        /// 用于删除
        /// </summary>
        private Dictionary<object, List<(string, Action<NotifyParam>)>> objRegistEventList = new Dictionary<object, List<(string, Action<NotifyParam>)>>();

        #region 用于防止消息触发时有删除需要触发的消息操作
        /// <summary>
        /// 正在触发中的事件
        /// </summary>
        private HashSet<string> notifactingList = new HashSet<string>();

        private HashSet<object> needRemoveList = new HashSet<object>();
        #endregion
        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="listener"></param>
        public void RegistNotification(string type, object target, Action<NotifyParam> listener)
        {
            if (listener == null || target == null)
                return;
            if (notifyTargets.ContainsKey(listener))
                throw new Exception("one function can not be register to different event!");

            Action<NotifyParam> myListener = null;
            notifacations.TryGetValue(type, out myListener);

            if (myListener != null)
            {
                Delegate[] dlgs = myListener.GetInvocationList();
                for (int i = 0; i < dlgs.Length; i++)
                {
                    if (notifyTargets[dlgs[i]].Equals(target))
                    {
                        Log.BASE.LogError("RegisterObserver {0} Repeat!", type);
                        return;
                    }
                    else
                    {
                        //拼接事件
                        myListener = (Action<NotifyParam>)Delegate.Combine(notifacations[type], dlgs[i]);
                    }
                }
            }
            //拼接事件
            notifacations[type] = (Action<NotifyParam>)Delegate.Combine(myListener, listener);
            notifyTargets.Add(listener, target);

            if (!objRegistEventList.ContainsKey(target)) objRegistEventList.Add(target, new List<(string, Action<NotifyParam>)>());
            objRegistEventList[target].Add((type, listener));
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param"></param>
        public void PostNotification(string type,NotifyParam param = null)
        {
            if(notifactingList.Contains(type))
            {
                Log.EX.LogError($"notifact event repeat type:{type}");
                return;
            }
            notifactingList.Add(type);
            notifacations.TryGetValue(type, out var action);
            Delegate[] dlgs = action.GetInvocationList();
            for (int i = 0; i < dlgs.Length; i++)
            {
                Action<NotifyParam> dlg = (Action<NotifyParam>)dlgs[i];
                if (dlg != null)
                    dlg.Invoke(param);
            }
            notifactingList.Remove(type);


            if(needRemoveList.Count > 0 && notifactingList.Count <= 0)
            {
                needRemoveList.ForEach((r) => {
                    RemoveNotifacation(r);
                });
                needRemoveList.Clear();
            }
        }


        public void RemoveNotifacation(object obj)
        {
            if(notifactingList.Count > 0)
            {
                needRemoveList.Add(obj);
                return;
            }
            objRegistEventList.TryGetValue(obj, out var infos);

            if(infos != null && infos.Count > 0)
            {
                //移除对应的事件
                for (int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];
                    var action = notifacations[info.Item1];
                    var dlg = info.Item2;
                    notifyTargets.Remove(dlg);
                    notifacations[info.Item1] = (Action<NotifyParam>)Delegate.Remove(action, dlg);
                }
                objRegistEventList.Remove(obj);
            }
        }

        public void RemoveAll()
        {
            notifacations.Clear();
            notifactingList.Clear();
            notifyTargets.Clear();
            objRegistEventList.Clear();
            needRemoveList.Clear();
        }
        

        public override void Dispose()
        {
            notifacations.Clear();
            base.Dispose();
        }
    }
}
```





