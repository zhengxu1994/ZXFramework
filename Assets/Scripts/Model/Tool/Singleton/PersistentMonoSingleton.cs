

using UnityEngine;

namespace Bepop.Core
{
    /// <summary>
    /// 如果跳转到新的场景里已经有了实例，则不创建新的单例（或者创建新的单例后会销毁掉新的单例）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PersistentMonoSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T mInstance;
        protected bool mEnabled;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = FindObjectOfType<T>();
                    if (mInstance == null)
                    {
                        var obj = new GameObject();
                        mInstance = obj.AddComponent<T>();
                    }
                }

                return mInstance;
            }
        }

        /// <summary>
        /// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
        /// </summary>
        protected virtual void Awake()
        {
            //if (!Application.IsPlaying)
            //{
            //    return;
            //}

            if (mInstance == null)
            {
                //If I am the first instance, make me the Singleton
                mInstance = this as T;
                DontDestroyOnLoad(transform.gameObject);
                mEnabled = true;
            }
            else
            {
                //If a Singleton already exists and you find
                //another reference in scene, destroy it!
                if (this != mInstance)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}