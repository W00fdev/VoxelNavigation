using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Octrees
{
    public class OctreeGenerator : MonoBehaviour
    {
        public float minNodeSize = 1f;
        
        private Octree ot;
        private List<GameObject> objects = new List<GameObject>(64000);

        public readonly Graph waypoints = new();

        private IEnumerator Start()
        {
            yield return null;
            for (int i = 0; i < transform.childCount; i++)
            {
                objects.Add(transform.GetChild(i).gameObject);
            }

            UnityEngine.Debug.Log($"#{Time.frameCount}: childs {objects.Count}");

            ot = new Octree(objects.ToArray(), minNodeSize, waypoints);
        }

        private void OnDrawGizmos()
        {
            if (ot == null)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(ot.bounds.center, ot.bounds.size);

            ot.root.DrawNode();
            ot.graph.DrawGraph();
        }
    }
}