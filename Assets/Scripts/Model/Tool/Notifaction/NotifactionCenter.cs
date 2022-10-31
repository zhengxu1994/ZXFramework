using System;
using Bepop.Core;
using System.Collections.Generic;
using System.Collections;


namespace Notifaction
{
    /// <summary>
    /// 消息中心
    /// </summary>
    public class NotifactionCenter : Singleton<NotifactionCenter>
    {

        private NotifactionCenter()
        {

        }


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
            if (action == null) return;
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
