using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Pathfinding;
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

            DateTime startBenchmarkUtc = DateTime.Now;

            UnityEngine.Debug.Log($"!@! {Time.frameCount}: start empty leaves gathering (utc = {startBenchmarkUtc})");

            GetEmptyLeaves(_root);

            DateTime secondBenchmarkUtc = DateTime.Now;

            UnityEngine.Debug.Log(
                $"!@! {Time.frameCount}: ends empty leaves gathering (ticks={(secondBenchmarkUtc - startBenchmarkUtc).Ticks}) (duration = {(secondBenchmarkUtc - startBenchmarkUtc).Duration()}) (utc = {secondBenchmarkUtc})"
            );

            GetEdges();

            DateTime thirdBenchmarkUtc = DateTime.Now;

            UnityEngine.Debug.Log(
                $"!@! {Time.frameCount}: ends edges gathering (ticks={(thirdBenchmarkUtc - secondBenchmarkUtc).Ticks}) (duration = {(thirdBenchmarkUtc - secondBenchmarkUtc).Duration()}) (utc = {thirdBenchmarkUtc})"
            );

            Debug.Log(aStarGraph.edges.Count);
        }

        void GetEdges()
        {
            var visitedNodes = new HashSet<OctreeNode>();

            foreach (OctreeNode leaf in _emptyLeaves)
            {
                visitedNodes.Add(leaf);

                foreach (OctreeNode otherLeaf in _emptyLeaves)
                {
                    if (visitedNodes.Contains(otherLeaf))
                        continue;

                    if (leaf.bounds.Intersects(otherLeaf.bounds))
                    {
                        _aStarGraph.AddEdge(leaf, otherLeaf);
                    }
                }
            }
        }

        /*void GetAllIntersectLeavesOf(OctreeNode node, OctreeNode intersectable, List<OctreeNode> result)
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
        }*/

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

            /*for (int i = 0; i < octreeNode.children.Length; i++)
            {
                for (int j = i + 1; j < octreeNode.children.Length; j++)
                {
                    _aStarGraph.AddEdge(octreeNode.children[i], octreeNode.children[j]);
                }
            }*/
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
