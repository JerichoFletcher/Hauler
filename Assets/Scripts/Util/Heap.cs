using System;

namespace Hauler.Util {
	// Thanks to Sebastian Lague for providing a very in-depth tutorial series on this
	public class Heap<T> where T : IHeapItem<T> {
		T[] items;
		int count;

		public int Count => count;

		public Heap(int maxHeapSize) {
			items = new T[maxHeapSize];
		}

		public void Add(T item) {
			item.HeapIndex = count;
			items[count] = item;
			SortUp(item);
			count++;
		}

		public T RemoveFirst() {
			T first = items[0];
			count--;
			items[0] = items[count];
			items[0].HeapIndex = 0;
			SortDown(items[0]);
			return first;
		}

		public void Clear() {
			for(int i = 0; i < count; i++) items[i] = default(T);
			count = 0;
        }

		public void UpdateItem(T item) {
			SortUp(item);
		}

		public bool Contains(T item) {
			return Equals(items[item.HeapIndex], item);
		}

		void SortDown(T item) {
			while(true) {
				int childIndexLeft = item.HeapIndex * 2 + 1;
				int childIndexRight = item.HeapIndex * 2 + 2;
				int swapIndex;

				if(childIndexLeft < count) {
					// Determine the highest priority child
					swapIndex = childIndexLeft;
					if(childIndexRight < count) {
						if(items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) {
							swapIndex = childIndexRight;
						}
					}
					// Swap with child if it has a higher priority
					if(item.CompareTo(items[swapIndex]) < 0) {
						Swap(item, items[swapIndex]);
					} else {
						return;
					}
				} else {
					return;
				}
			}
		}

		void SortUp(T item) {
			int parentIndex = (item.HeapIndex - 1) / 2;
			// Swap with parent if it has a lower priority
			while(parentIndex >= 0) {
				T parentItem = items[parentIndex];
				if(item.CompareTo(parentItem) > 0) {
					Swap(item, parentItem);
				} else {
					return;
				}
				parentIndex = (item.HeapIndex - 1) / 2;
			}
		}

		void Swap(T item1, T item2) {
			// Swap positions in array
			items[item1.HeapIndex] = item2;
			items[item2.HeapIndex] = item1;
			// Swap stored index
			int itemAIndex = item1.HeapIndex;
			item1.HeapIndex = item2.HeapIndex;
			item2.HeapIndex = itemAIndex;
		}
	}


	public interface IHeapItem<T> : IComparable<T> {
		int HeapIndex { get; set; }
	}
}