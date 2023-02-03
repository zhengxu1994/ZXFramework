using System;
using System.Collections.Generic;
namespace Movement
{
    public class PriorityQueue<KeyType, PriorityType> where PriorityType : System.IComparable
    {
        struct Element<SubClassKeyType, SubClassPriorityType> where SubClassPriorityType : System.IComparable
        {
            public SubClassKeyType key;

            public SubClassPriorityType priority;

            public Element(SubClassKeyType key, SubClassPriorityType priority)
            {
                this.key = key;
                this.priority = priority;
            }
        }

        List<Element<KeyType, PriorityType>> queue = new List<Element<KeyType, PriorityType>>();

        public void Push(KeyType arg_key, PriorityType arg_priority)
        {
            Element<KeyType, PriorityType> new_elem = new Element<KeyType, PriorityType>();
            int index = 0;
            foreach (var element in queue)
            {
                if (new_elem.priority.CompareTo(element.priority) < 0)
                    break;
                ++index;
            }
            queue.Insert(index, new_elem);
        }

        public KeyType Pop()
        {
            if (IsEmpty())
                throw new Exception("Attempted to pop off an empty queue");
            Element<KeyType, PriorityType> top = queue[0];
            queue.RemoveAt(0);
            return top.key;
        }

        public bool IsEmpty()
        {
            return queue.Count == 0;
        }
    }
}