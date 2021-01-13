namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    class CircularBuffer<T> : ICollection<T>, ICollection where T : class
    {
        public CircularBuffer(int capacity, bool allowOverflow = false)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("Capacity can not be less than zero", "capacity");
            }

            this.capacity = capacity;
            Size = 0;
            head = 0;
            tail = 0;
            buffer = new T[capacity];
            AllowOverflow = allowOverflow;
        }

        bool AllowOverflow { get; set; }

        public int Capacity
        {
            get => capacity;
            set
            {
                if (value == capacity)
                {
                    return;
                }

                if (value < Size)
                {
                    throw new ArgumentOutOfRangeException("value", "Cannot reduce capacity below current size");
                }

                var dst = new T[value];
                if (Size > 0)
                {
                    CopyTo(dst);
                }
                buffer = dst;

                capacity = value;
            }
        }

        public int Size { get; set; }

        int ICollection.Count => Size;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    Interlocked.CompareExchange(ref syncRoot, new object(), null);
                }
                return syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex) => CopyTo((T[])array, arrayIndex);

        public bool Contains(T item)
        {
            var bufferIndex = head;
            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < Size; i++, bufferIndex++)
            {
                if (bufferIndex == capacity)
                {
                    bufferIndex = 0;
                }

                if (item == null && buffer[bufferIndex] == null)
                {
                    return true;
                }
                if (buffer[bufferIndex] != null &&
                    comparer.Equals(buffer[bufferIndex], item))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            Size = 0;
            head = 0;
            tail = 0;
        }

        public void CopyTo(T[] array, int arrayIndex) => CopyTo(0, array, arrayIndex, Size);

        int ICollection<T>.Count => Size;

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item) => Put(item);

        bool ICollection<T>.Remove(T item)
        {
            if (Size == 0)
            {
                return false;
            }

            Get();
            return true;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Put(T[] src) => Put(src, 0, src.Length);

        public int Put(T[] src, int offset, int count)
        {
            if (!AllowOverflow && count > capacity - Size)
            {
                throw new InvalidOperationException("Overflow is not allowed");
            }

            var srcIndex = offset;
            for (var i = 0; i < count; i++, tail++, srcIndex++)
            {
                if (tail == capacity)
                {
                    tail = 0;
                }
                buffer[tail] = src[srcIndex];
            }
            Size = Math.Min(Size + count, capacity);
            return count;
        }

        public void Put(T item)
        {
            if (!AllowOverflow && Size == capacity)
            {
                throw new InvalidOperationException("Overflow is not allowed");
            }

            lock (bufferLock)
            {
                buffer[tail] = item;
                if (++tail == capacity)
                {
                    tail = 0;
                }
                Size++;
            }
        }

        public void Skip(int count)
        {
            head += count;
            if (head >= capacity)
            {
                head -= capacity;
            }
        }

        public T[] Get(int count)
        {
            var dst = new T[count];
            Get(dst);
            return dst;
        }

        public int Get(T[] dst) => Get(dst, 0, dst.Length);

        public int Get(T[] dst, int offset, int count)
        {
            if (Size == 0)
            {
                throw new InvalidOperationException("Buffer is empty");
            }

            var realCount = Math.Min(count, Size);
            var dstIndex = offset;
            for (var i = 0; i < realCount; i++, head++, dstIndex++)
            {
                if (head == capacity)
                {
                    head = 0;
                }
                dst[dstIndex] = buffer[head];
            }
            return realCount;
        }

        public T Get()
        {
            if (Size == 0)
            {
                throw new InvalidOperationException("Buffer is empty");
            }

            lock (bufferLock)
            {
                var item = buffer[head];
                if (++head == capacity)
                {
                    head = 0;
                }

                return item;
            }
        }

        public void CopyTo(T[] array) => CopyTo(array, 0);

        void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _ = index;

            if (count > Size)
            {
                throw new ArgumentOutOfRangeException("count", "Message read count is larger than size");
            }

            var bufferIndex = head;
            for (var i = 0; i < count; i++, bufferIndex++, arrayIndex++)
            {
                if (bufferIndex == capacity)
                {
                    bufferIndex = 0;
                }
                array[arrayIndex] = buffer[bufferIndex];
            }
        }

        IEnumerator<T> GetEnumerator()
        {
            var bufferIndex = head;
            for (var i = 0; i < Size; i++, bufferIndex++)
            {
                if (bufferIndex == capacity)
                {
                    bufferIndex = 0;
                }

                yield return buffer[bufferIndex];
            }
        }

        public T[] GetBuffer() => buffer;

        public T[] ToArray()
        {
            var dst = new T[Size];
            CopyTo(dst);
            return dst;
        }

        int capacity;
        int head;
        int tail;
        T[] buffer;
        object syncRoot;
        object bufferLock = new object();
    }
}