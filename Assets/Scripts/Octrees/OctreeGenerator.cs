using System;
using UnityEngine;

namespace Octrees
{
    public class OctreeGenerator : MonoBehaviour
    {
        public GameObject[] objects;
        public float minNodeSize = 1f;
        private Octree ot;

        public readonly Graph waypoints = new();

        private void Awake()
        {
            ot = new Octree(objects, minNodeSize, waypoints);
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