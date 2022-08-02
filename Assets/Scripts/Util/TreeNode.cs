using System.Collections.Generic;
using UnityEngine;

namespace Hauler.Util {
    [System.Serializable]
    public class TreeNode<T> {
        public T item;
        public TreeNode<T> parent;
        public TreeNode<T>[] childs;

        static List<TreeNode<T>> temp = new List<TreeNode<T>>();

        public virtual TreeNode<T> Select(bool considerParent) {
            temp.Clear();
            if(parent != null && parent.item == null) parent = null;
            if(considerParent && parent != null) temp.Add(parent);
            foreach(TreeNode<T> n in childs) {
                n.parent = this;
                temp.Add(n);
            }
            return temp[Random.Range(0, temp.Count)];
        }
    }
}
