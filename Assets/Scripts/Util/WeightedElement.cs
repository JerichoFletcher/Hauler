using UnityEngine;

namespace Hauler.Util {
    [System.Serializable]
    public struct WeightedElement<T> {
        public T item;
        public int weight;

        public static T Select(WeightedElement<T>[] array) {
            if(array.Length == 0) return default(T);
            if(array.Length == 1) return array[0].item;
            int totalWeight = 0;
            foreach(WeightedElement<T> e in array) totalWeight += e.weight;
            int threshold = Random.Range(0, totalWeight);
            foreach(WeightedElement<T> e in array) {
                if(threshold < e.weight) return e.item;
                threshold -= e.weight;
            }
            Debug.LogError($"WeightedEntry<{typeof(T).Name}>.Select() method somehow not returning a valid item");
            return default(T);
        }
    }
}
