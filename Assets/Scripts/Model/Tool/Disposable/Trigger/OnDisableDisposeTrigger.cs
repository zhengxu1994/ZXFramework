using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bepop.Core
{
    public class OnDisableDisposeTrigger : MonoBehaviour
    {
        HashSet<IDisposable> mDisposables = new HashSet<IDisposable>();

        public void AddDispose(IDisposable disposable)
        {
            if (!mDisposables.Contains(disposable))
            {
                mDisposables.Add(disposable);
            }
        }

        private void OnDisable()
        {
            //if (Application.IsPlaying)
            {
                foreach (var disposable in mDisposables)
                {
                    disposable.Dispose();
                }

                mDisposables.Clear();
                mDisposables = null;
            }
        }
    }
}