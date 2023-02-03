using System;
using System.Collections.Generic;
namespace Pool
{
    public class TObjectPool<T>
    {
        private Stack<T> myStack;

        private Func<T> mActionAlloc;
        private Action<T> mActionFree;
        private Action<T> mActionDestroy;

        public TObjectPool(Func<T> rActionAlloc,Action<T> rActionFree,Action<T> rActionDestroy)
        {
            myStack = new Stack<T>();

            this.mActionAlloc = rActionAlloc;
            this.mActionFree = rActionFree;
            this.mActionDestroy = rActionDestroy;
        }

        public T Alloc()
        {
            if (myStack.Count == 0)
                return UtilTool.SafeExecute<T>(mActionAlloc);
            else
                return myStack.Pop();
         }

        public void Free(T rElement)
        {
            if (mActionFree != null)
                mActionFree(rElement);
            myStack.Push(rElement);
        }

        public void Destroy()
        {
            if (myStack == null) return;
            foreach (var t in myStack)
            {
                mActionDestroy(t);
            }
            myStack.Clear();
        }
    }
}