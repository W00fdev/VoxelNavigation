using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Pathfinding;
using UnityEngine;
using System.Linq;

namespace Octrees
{
    public class Octree
    {
        public OctreeNode root => _root;
        public Bounds bounds => _bounds;
        public AStarGraph aStarGraph => _aStarGraph;
        public FlowFieldGraph flowFieldGraph => _flowFieldGraph;

        readonly AStarGraph _aStarGraph;
        readonly FlowFieldGraph _flowFieldGraph;
        readonly List<OctreeNode> _emptyLeaves = new();

        Bounds _bounds;
        OctreeNode _root;

        public Octree(ref NativeArray<float3> collisionPositions, float minNodeSize, FlowFieldGraph flowFieldGraph)
        {
            _flowFieldGraph = flowFieldGraph;

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

            Debug.Log(_flowFieldGraph.edges.Count);
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
                        //_aStarGraph.AddEdge(leaf, otherLeaf);
                        _flowFieldGraph.AddEdge(leaf, otherLeaf);
                    }
                }
            }
        }

        public OctreeNode GetClosestNode(Vector3 position)
        {
            if (!root.bounds.Contains(position))
            {
                //throw new Exception("New position is out of root bounds");
                Debug.LogWarning($"New position({position}) is out of root bounds");
                return null;
            }

            OctreeNode targetNode = root;

            while (!targetNode.isLeaf)
            {
                OctreeNode node = targetNode.children.FirstOrDefault(node => node.bounds.Contains(position));

                if (node == null)
                    break;

                targetNode = node;
            }

            return targetNode;
        }

        public void AddPosition(Vector3 position)
        {
            OctreeNode node = GetClosestNode(position);
            if (node == null)
                return;

            OctreeObject nodeObject = node.parent.objects.FirstOrDefault(@object => @object.Intersects(node.bounds));
            nodeObject ??= new OctreeObject(position);

            //flowFieldGraph.RemoveNode(node);

            HashSet<OctreeNode> newNodes = new();

            node.Divide(nodeObject, ref newNodes);

            HashSet<OctreeNode> addedLeaves = new();
            foreach (OctreeNode newNode in newNodes)
            {
                GetEmptyLeavesWithMemory(newNode, ref addedLeaves);
            }

            // hashet ребра не перебирать все таки? или сделать метод для всех

            foreach (OctreeNode leaf in addedLeaves)
            {
                foreach (OctreeNode otherLeaf in addedLeaves)
                {
                    if (leaf.bounds.Intersects(otherLeaf.bounds))
                    {
                        //_aStarGraph.AddEdge(leaf, otherLeaf);
                        _flowFieldGraph.AddEdge(leaf, otherLeaf);
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

        void GetEmptyLeavesWithMemory(OctreeNode octreeNode, ref HashSet<OctreeNode> addedLeaves)
        {
            if (octreeNode.isLeaf
             && octreeNode.objects.Count == 0)
            {
                _emptyLeaves.Add(octreeNode);
                _flowFieldGraph.AddNode(octreeNode);

                addedLeaves.Add(octreeNode);

                return;
            }

            if (octreeNode.children == null)
                return;

            foreach (OctreeNode octreeNodeChild in octreeNode.children)
            {
                GetEmptyLeavesWithMemory(octreeNodeChild, ref addedLeaves);
            }
        }

        void GetEmptyLeaves(OctreeNode octreeNode)
        {
            if (octreeNode.isLeaf
             && octreeNode.objects.Count == 0)
            {
                _emptyLeaves.Add(octreeNode);
                _flowFieldGraph.AddNode(octreeNode);

                return;
            }

            if (octreeNode.children == null)
                return;

            foreach (OctreeNode octreeNodeChild in octreeNode.children)
            {
                GetEmptyLeaves(octreeNodeChild);
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

        /*public void AddObstacles(ref NativeArray<float3> newCollisionPositions)
        {
            foreach (float3 position in newCollisionPositions)
            {
                AddPosition(position);
            }
        }*/

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
