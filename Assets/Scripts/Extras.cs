using System;

namespace Extras
{
    public class DualValueList<T1, T2>
    {
        private DualValueNode<T1, T2> head;

        public DualValueNode<T1, T2> GetHead()
        {
            return head;
        }

        public int GetLength()
        {
            int length = 1;
            DualValueNode<T1, T2> current = head;
            while (current.GetNext() != null)
            {
                current = current.GetNext();
                length++;
            }
            return length;
        }

        public DualValueNode<T1, T2> GetLast()
        {
            DualValueNode<T1, T2> current = head;

            while (current.GetNext() != null)
            {
                current = current.GetNext();
            }
            
            return current;
        }

        public void Add(T1 valueA, T2 valueB)
        {
            if (head == null)
            {
                head = new DualValueNode<T1, T2>(valueA, valueB);
            }
            else
            {
                GetLast().SetNext(new DualValueNode<T1, T2>(valueA, valueB));
            }
        }

        public DualValueNode<T1, T2> GetAtIndex(int index)
        {
            DualValueNode<T1, T2> current = head;
            for (int i = 0; i < index; i++)
            {
                current = current.GetNext();
            }

            return current;
        }

        public DualValueNode<T1, T2> this[int index] => GetAtIndex(index);
        
        public DualValueList()
        {
            this.head = null;
        }
        
        public DualValueList(DualValueNode<T1, T2> head)
        {
            this.head = head;
        }
    }
    
    public class DualValueNode<T1, T2>
    {
        private DualValueNode<T1, T2> next;

        private T1 valueA;
        private T2 valueB;

        public T1 GetA()
        {
            return valueA;
        }

        public T2 GetB()
        {
            return valueB;
        }

        public DualValueNode<T1, T2> GetNext()
        {
            return next;
        }

        public void SetNext(DualValueNode<T1, T2> value)
        {
            next = value;
        }

        public DualValueNode(T1 valueA, T2 valueB)
        {
            this.valueA = valueA;
            this.valueB = valueB;
        }
    }
}