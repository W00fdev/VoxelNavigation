using System.Collections.Generic;
using UnityEngine;

namespace Octrees
{
    public class Octree
    {
        public OctreeNode root;
        public Bounds bounds;
        public Graph graph;

        private List<OctreeNode> emptyLeaves = new();

        public Octree(GameObject[] worldObjects, float minNodeSize, Graph graph)
        {
            this.graph = graph;

            // too much calculations
            //CalculateBounds(worldObjects);
            //CreateTree(worldObjects, minNodeSize);

            GetEmptyLeaves(root);
            GetEdges();
            Debug.Log(graph.edges.Count);
        }

        private void GetEdges()
        {
            foreach (OctreeNode leaf in emptyLeaves)
            {
                foreach (OctreeNode otherLeaf in emptyLeaves)
                {
                    if (leaf.bounds.Intersects(otherLeaf.bounds))
                    {
                        graph.AddEdge(leaf, otherLeaf);
                    }
                }
            }
        }

        private void GetEmptyLeaves(OctreeNode octreeNode)
        {
            if (octreeNode.IsLeaf && octreeNode.objects.Count == 0)
            {
                emptyLeaves.Add(octreeNode);
                graph.AddNode(octreeNode);
                return;
            }

            if (octreeNode.children == null)
                return;

            foreach (OctreeNode octreeNodeChild in octreeNode.children)
            {
                GetEmptyLeaves(octreeNodeChild);
            }

            for (int i = 0; i < octreeNode.children.Length; i++)
            {
                for (int j = i + 1; j < octreeNode.children.Length; j++)
                {
                    graph.AddEdge(octreeNode.children[i], octreeNode.children[j]);
                }
            }
        }

        void CreateTree(GameObject[] worldObjects, float minNodeSize)
        {
            root = new OctreeNode(bounds, minNodeSize);

            foreach (GameObject worldObject in worldObjects)
            {
                root.Divide(worldObject);
            }
        }

        void CalculateBounds(GameObject[] worldObjects)
        {
            foreach (GameObject worldObject in worldObjects)
            {
                bounds.Encapsulate(worldObject.GetComponent<Collider>().bounds);
            }

            Vector3 size = Vector3.one * Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.6f;
            bounds.SetMinMax(bounds.center - size, bounds.center + size);

            UnityEngine.Debug.Log($"#{Time.frameCount}: bounds: {bounds.center} + size: {bounds.size}");
        }
    }
}