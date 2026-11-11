using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 3. Implement as a binary min-heap.

    /// <summary>
    /// A minimum-priority queue implemented as a binary min-heap.
    ///
    /// Items with lower integer priority values should be returned before
    /// items with higher priority values.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    /// </summary>
    public sealed class MinPriorityQueue<T>
    {
        // The list stores the physical representation of the binary heap.
        // Your implementation is responsible for preserving the min-heap invariant.
        private readonly List<Entry> _heap = new List<Entry>();

        /// <summary>
        /// Number of items currently stored in the queue.
        /// </summary>
        public int Count => _heap.Count;

        /// <summary>
        /// Adds an item with the supplied priority.
        /// Lower priority values are removed before higher priority values.
        /// </summary>
        public void Enqueue(T item, int priority)
        {
            _heap.Add(new Entry(item, priority));
            SiftUp(_heap.Count - 1);
        }

        /// <summary>
        /// Returns the minimum-priority item without removing it.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the queue is empty.
        /// </exception>
        public T Peek()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _heap[0].Item;
        }

        /// <summary>
        /// Removes and returns the minimum-priority item.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the queue is empty.
        /// </exception>
        public T Dequeue()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");
            
            var root = _heap[0];

            Swap(0, _heap.Count - 1);
            _heap.RemoveAt(_heap.Count - 1);
            
            if (_heap.Count > 0)
                SiftDown(0);

            return root.Item;
        }
        
        private static int ParentIndex(int index)
        {
            return (index - 1) / 2;
        }

        private static int LeftChildIndex(int index)
        {
            return 2 * index + 1;
        }

        private static int RightChildIndex(int index)
        {
            return 2 * index + 2;
        }
        
        private void SiftUp(int index)
        {
            while (index > 0)
            {
                var parentIndex = ParentIndex(index);

                if (_heap[index].Priority >= _heap[parentIndex].Priority)
                {
                    break;
                }
                
                Swap(index, parentIndex);
                
                index = parentIndex;
            }
        }
        
        private void SiftDown(int index)
        {
            while (true)
            {
                var leftChildIndex = LeftChildIndex(index);
                var rightChildIndex = RightChildIndex(index);
                var smallestIndex = index;
            
                if (leftChildIndex < _heap.Count && _heap[leftChildIndex].Priority < _heap[smallestIndex].Priority)
                    smallestIndex = leftChildIndex;
            
                if (rightChildIndex < _heap.Count && _heap[rightChildIndex].Priority < _heap[smallestIndex].Priority)
                    smallestIndex = rightChildIndex;

                if (smallestIndex == index)
                    break;
                
                Swap(index, smallestIndex);
                
                index = smallestIndex;
            }
        }

        private void Swap(int a, int b)
        {
            Entry temp = _heap[a];
            _heap[a] = _heap[b];
            _heap[b] = temp;
        }

        /// <summary>
        /// Stores one item together with the priority used by the heap.
        /// </summary>
        private readonly struct Entry
        {
            public Entry(T item, int priority)
            {
                Item = item;
                Priority = priority;
            }

            public T Item { get; }
            public int Priority { get; }
        }
    }
}
