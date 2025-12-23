using System.Collections.Generic;
using Pathfinding;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace Octrees
{
    public class Octree
    {
        public OctreeNode root => _root;
        public Bounds bounds => _bounds;
        public AStarGraph aStarGraph => _aStarGraph;

        readonly AStarGraph _aStarGraph;
        readonly List<OctreeNode> _emptyLeaves = new();
        Bounds _bounds;
        OctreeNode _root;

        public Octree(ref NativeArray<float3> collisionPositions, float minNodeSize, AStarGraph aStarGraph)
        {
            _aStarGraph = aStarGraph;

            CalculateBounds(ref collisionPositions);
            CreateTree(ref collisionPositions, minNodeSize);

            GetEmptyLeaves(_root);
            GetEdges();
            Debug.Log(aStarGraph.edges.Count);
        }

        void GetEdges()
        {
            /*foreach (OctreeNode leaf in _emptyLeaves)
            {
                foreach (OctreeNode otherLeaf in _emptyLeaves)
                {
                    if (leaf.bounds.Intersects(otherLeaf.bounds))
                    {
                        _aStarGraph.AddEdge(leaf, otherLeaf);
                    }
                }
            }*/

            // root is leaf

            if (_emptyLeaves.Count == 0)
                return;

            foreach (OctreeNode leaf in _emptyLeaves)
            {
                foreach (OctreeNode neighbour in leaf.parent.children)
                {
                    if (neighbour == leaf
                     || neighbour.children == null)
                        continue;

                    List<OctreeNode> allIntersectLeavesOfNeighbour = new();
                    GetAllIntersectLeavesOf(neighbour, leaf, allIntersectLeavesOfNeighbour);

                    foreach (OctreeNode intersectNeighbourLeaf in allIntersectLeavesOfNeighbour)
                    {
                        _aStarGraph.AddEdge(intersectNeighbourLeaf, leaf);
                    }
                }
            }
        }

        void GetAllIntersectLeavesOf(OctreeNode node, OctreeNode intersectable, List<OctreeNode> result)
        {
            Bounds intersectableBounds = intersectable.bounds;

            foreach (OctreeNode child in node.children)
            {
                if (child.bounds.Intersects(intersectableBounds))
                {
                    if (child.isLeaf)
                    {
                        result.Add(child);
                    }
                    else
                    {
                        GetAllIntersectLeavesOf(child, intersectable, result);
                    }
                }
            }
        }

        void GetEmptyLeaves(OctreeNode octreeNode)
        {
            if (octreeNode.isLeaf
             && octreeNode.objects.Count == 0)
            {
                _emptyLeaves.Add(octreeNode);
                _aStarGraph.AddNode(octreeNode);
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
                    _aStarGraph.AddEdge(octreeNode.children[i], octreeNode.children[j]);
                }
            }
        }

        void CreateTree(ref NativeArray<float3> collisionPositions, float minNodeSize)
        {
            _root = new OctreeNode(null, _bounds, minNodeSize);

            foreach (Vector3 collisionPosition in collisionPositions)
            {
                _root.Divide(collisionPosition);
            }
        }

        void CalculateBounds(ref NativeArray<float3> collisionPositions)
        {
            foreach (Vector3 collisionPosition in collisionPositions)
            {
                _bounds.Encapsulate(new Bounds(collisionPosition, Vector3.one));
            }

            Vector3 size = Vector3.one * Mathf.Max(_bounds.size.x, _bounds.size.y, _bounds.size.z) * 0.6f;
            _bounds.SetMinMax(_bounds.center - size, _bounds.center + size);

            UnityEngine.Debug.Log($"#{Time.frameCount}: bounds: {_bounds.center} + size: {_bounds.size}");
        }
    }
}
